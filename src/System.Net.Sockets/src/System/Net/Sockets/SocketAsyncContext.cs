// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace System.Net.Sockets
{
    // TODO
    // - ReceiveMessageFrom
    // - Plumb status through async APIs to avoid callbacks on synchronous completion
    //     - NOTE: this will require refactoring in the *Async APIs to accommodate the lack
    //             of completion posting

    sealed class SocketAsyncContext
    {
        private abstract class AsyncOperation
        {
            public AsyncOperation Next;
            public SocketError ErrorCode;

            public AsyncOperation()
            {
                Next = this;
            }

            public abstract void Complete();
        }

        private sealed class SentinelAsyncOperation : AsyncOperation
        {
            public override void Complete()
            {
                Debug.Fail("SentinelAsyncOperation.Complete() should never be called");
            }
        }

        private sealed class TransferOperation : AsyncOperation
        {
            public Action<int, byte[], int, int, SocketError> Callback;
            public byte[] Buffer;
            public BufferList Buffers;
            public int BufferIndex;
            public int Offset;
            public int Count;
            public int Flags;
            public byte[] SocketAddress;
            public int SocketAddressLen;
            public int BytesTransferred;
            public int ReceivedFlags;

            public override void Complete()
            {
                Debug.Assert(Callback != null);

                Callback(BytesTransferred, SocketAddress, SocketAddressLen, ReceivedFlags, ErrorCode);
            }
        }

        private abstract class AcceptOrConnectOperation : AsyncOperation
        {
            public byte[] SocketAddress;
            public int SocketAddressLen;
        }

        private sealed class AcceptOperation : AcceptOrConnectOperation
        {
            public Action<int, byte[], int, SocketError> Callback;
            public int AcceptedFileDescriptor;

            public override void Complete()
            {
                Debug.Assert(Callback != null);

                Callback(AcceptedFileDescriptor, SocketAddress, SocketAddressLen, ErrorCode);
            }
        }

        private sealed class ConnectOperation : AcceptOrConnectOperation
        {
            public Action<SocketError> Callback;

            public override void Complete()
            {
                Debug.Assert(Callback != null);

                Callback(ErrorCode);
            }
        }

        enum State
        {
            Stopped = -1,
            Clear = 0,
            Set = 1,
        }

        struct Queue<TOperation>
            where TOperation : AsyncOperation
        {
            private AsyncOperation _tail;

            public State State { get; set; }
            public bool IsStopped { get { return State == State.Stopped; } }
            public bool IsEmpty { get { return _tail == null; } }

            public TOperation Head
            {
                get
                {
                    Debug.Assert(!IsStopped);
                    return (TOperation)_tail.Next;
                }
            }

            public TOperation Tail
            {
                get
                {
                    Debug.Assert(!IsStopped);
                    return (TOperation)_tail;
                }
            }

            public void Enqueue(TOperation operation)
            {
                Debug.Assert(!IsStopped);

                if (!IsEmpty)
                {
                    operation.Next = _tail.Next;
                    _tail.Next = operation;
                }

                _tail = operation;
            }

            public void Dequeue()
            {
                Debug.Assert(!IsStopped);
                Debug.Assert(!IsEmpty);

                AsyncOperation head = _tail.Next;
                if (head == _tail)
                {
                    _tail = null;
                }
                else
                {
                    _tail.Next = head.Next;
                }
            }

            public Queue<TOperation> Stop()
            {
                Queue<TOperation> result = this;
                _tail = null;
                State = State.Stopped;
                return result;
            }
        }

        int _fileDescriptor;
        GCHandle _handle;
        Queue<TransferOperation> _receiveQueue;
        Queue<TransferOperation> _sendQueue;
        Queue<AcceptOrConnectOperation> _acceptOrConnectQueue;
        object _closeLock = new object();
        object _queueLock = new object();
        SocketAsyncEngine _engine;
        uint _registeredEvents;

        public SocketAsyncContext(int fileDescriptor, SocketAsyncEngine engine)
        {
            _fileDescriptor = fileDescriptor;
            _engine = engine;

            var events = Interop.libc.EPOLLIN | Interop.libc.EPOLLOUT | Interop.libc.EPOLLRDHUP | Interop.libc.EPOLLET;

            Interop.Error errorCode;
            if (!_engine.TryRegister(this, _fileDescriptor, events, 0, ref _handle, out errorCode))
            {
                throw new Exception(string.Format("SocketAsyncContext: {0}", errorCode));
            }

            _registeredEvents = events;
        }

        public void Close()
        {
            lock (_closeLock)
            lock (_queueLock)
            {
                // Force a HUP event in order to drain the queues.
                HandleEvents(Interop.libc.EPOLLHUP);
            }
        }

        private bool TryBeginOperation<TOperation>(ref Queue<TOperation> queue, TOperation operation, out bool isStopped)
            where TOperation : AsyncOperation
        {
            lock (_queueLock)
            {
                switch (queue.State)
                {
                    case State.Stopped:
                        isStopped = true;
                        return false;

                    case State.Clear:
                        break;

                    case State.Set:
                        isStopped = false;
                        queue.State = State.Clear;
                        return false;
                }

                queue.Enqueue(operation);
                isStopped = false;
                return true;
            }
        }

        private void EndOperation<TOperation>(ref Queue<TOperation> queue)
            where TOperation : AsyncOperation
        {
            lock (_queueLock)
            {
                Debug.Assert(!queue.IsStopped);

                queue.Dequeue();
            }
        }

        public SocketError AcceptAsync(byte[] socketAddress, int socketAddressLen, Action<int, byte[], int, SocketError> callback)
        {
            Debug.Assert(callback != null);

            int acceptedFd;
            SocketError errorCode;
            if (TryCompleteAccept(_fileDescriptor, socketAddress, ref socketAddressLen, out acceptedFd, out errorCode))
            {
                ThreadPool.QueueUserWorkItem(_ => callback(acceptedFd, socketAddress, socketAddressLen, errorCode), null);
                return errorCode;
            }

            var operation = new AcceptOperation {
                Callback = callback,
                SocketAddress = socketAddress,
                SocketAddressLen = socketAddressLen
            };

            bool isStopped;
            while (!TryBeginOperation(ref _acceptOrConnectQueue, operation, out isStopped))
            {
                if (isStopped)
                {
                    // TODO: is this error reasonable for a closed socket? Check with Winsock.
                    operation.ErrorCode = SocketError.Shutdown;
                    QueueCompletion(operation);
                    return SocketError.Shutdown;
                }

                if (TryCompleteAccept(_fileDescriptor, operation))
                {
                    QueueCompletion(operation);
                    break;
                }
            }
            return SocketError.IOPending;
        }

        private static bool TryCompleteAccept(int fileDescriptor, AcceptOperation operation)
        {
            return TryCompleteAccept(fileDescriptor, operation.SocketAddress, ref operation.SocketAddressLen, out operation.AcceptedFileDescriptor, out operation.ErrorCode);
        }

        private static unsafe bool TryCompleteAccept(int fileDescriptor, byte[] socketAddress, ref int socketAddressLen, out int acceptedFd, out SocketError errorCode)
        {
            int fd;
            uint sockAddrLen = (uint)socketAddressLen;
            fixed (byte* rawSocketAddress = socketAddress)
            {
                fd = Interop.libc.accept(fileDescriptor, (Interop.libc.sockaddr*)rawSocketAddress, &sockAddrLen);
            }

            if (fd != -1)
            {
                socketAddressLen = (int)sockAddrLen;
                errorCode = SocketError.Success;
                acceptedFd = fd;
                return true;
            }
            acceptedFd = -1;

            Interop.Error errno = Interop.Sys.GetLastError();
            if (errno != Interop.Error.EAGAIN && errno != Interop.Error.EWOULDBLOCK)
            {
                errorCode = SocketPal.GetSocketErrorForErrorCode(errno);
                return true;
            }

            errorCode = SocketError.Success;
            return false;
        }

        public SocketError ConnectAsync(byte[] socketAddress, int socketAddressLen, Action<SocketError> callback)
        {
            Debug.Assert(socketAddress != null);
            Debug.Assert(socketAddressLen > 0);
            Debug.Assert(callback != null);

            SocketError errorCode;
            if (TryCompleteConnect(_fileDescriptor, socketAddress, socketAddressLen, out errorCode))
            {
                ThreadPool.QueueUserWorkItem(_ => callback(errorCode), null);
                return errorCode;
            }

            var operation = new ConnectOperation {
                Callback = callback,
                SocketAddress = socketAddress,
                SocketAddressLen = socketAddressLen
            };

            bool isStopped;
            while (!TryBeginOperation(ref _acceptOrConnectQueue, operation, out isStopped))
            {
                if (isStopped)
                {
                    // TODO: is this error code reasonable for a closed socket? Check with Winsock.
                    operation.ErrorCode = SocketError.Shutdown;
                    QueueCompletion(operation);
                    return SocketError.Shutdown;
                }

                if (TryCompleteConnect(_fileDescriptor, operation))
                {
                    QueueCompletion(operation);
                    break;
                }
            }
            return SocketError.IOPending;
        }

        private static bool TryCompleteConnect(int fileDescriptor, ConnectOperation operation)
        {
            return TryCompleteConnect(fileDescriptor, operation.SocketAddress, operation.SocketAddressLen, out operation.ErrorCode);
        }

        private static unsafe bool TryCompleteConnect(int fileDescriptor, byte[] socketAddress, int socketAddressLen, out SocketError errorCode)
        {
            Debug.Assert(socketAddress != null);
            Debug.Assert(socketAddressLen > 0);

            int err;
            fixed (byte* rawSocketAddress = socketAddress)
            {
                var sockAddr = (Interop.libc.sockaddr*)rawSocketAddress;
                err = Interop.libc.connect(fileDescriptor, sockAddr, (uint)socketAddressLen);
            }

            if (err == 0)
            {
                errorCode = SocketError.Success;
                return true;
            }

            Interop.Error errno = Interop.Sys.GetLastError();
            if (errno != Interop.Error.EINPROGRESS)
            {
                errorCode = SocketPal.GetSocketErrorForErrorCode(errno);
                return true;
            }

            errorCode = SocketError.Success;
            return false;
        }

        private static unsafe int Receive(int fd, int flags, int available, byte[] buffer, int offset, int count, byte[] socketAddress, ref int socketAddressLen, out int receivedFlags, out Interop.Error errno)
        {
            Debug.Assert(socketAddress != null || socketAddressLen == 0);

            var pinnedSocketAddress = default(GCHandle);
            Interop.libc.sockaddr* sockAddr = null;
            uint sockAddrLen = 0;

            int received;
            try
            {
                if (socketAddress != null)
                {
                    pinnedSocketAddress = GCHandle.Alloc(socketAddress, GCHandleType.Pinned);
                    sockAddr = (Interop.libc.sockaddr*)pinnedSocketAddress.AddrOfPinnedObject();
                    sockAddrLen = (uint)socketAddressLen;
                }

                fixed (byte* b = buffer)
                {
                    var iov = new Interop.libc.iovec {
                        iov_base = &b[offset],
                        iov_len = (IntPtr)count
                    };

                    var msghdr = new Interop.libc.msghdr {
                        msg_name = sockAddr,
                        msg_namelen = sockAddrLen,
                        msg_iov = &iov,
                        msg_iovlen = (IntPtr)1,
                        msg_control = null,
                        msg_controllen = IntPtr.Zero,
                    };

                    received = (int)Interop.libc.recvmsg(fd, &msghdr, flags);
                    receivedFlags = msghdr.msg_flags;
                }
            }
            finally
            {
                if (pinnedSocketAddress.IsAllocated)
                {
                    pinnedSocketAddress.Free();
                }
            }

            if (received == -1)
            {
                errno = Interop.Sys.GetLastError();
                return -1;
            }

            socketAddressLen = (int)sockAddrLen;
            errno = Interop.Error.SUCCESS;
            return received;
        }

        private static unsafe int Receive(int fd, int flags, int available, BufferList buffers, byte[] socketAddress, ref int socketAddressLen, out int receivedFlags, out Interop.Error errno)
        {
            // Pin buffers and set up iovecs
            int maxBuffers = buffers.Count;
            var handles = new GCHandle[maxBuffers];
            var iovecs = new Interop.libc.iovec[maxBuffers];

            var pinnedSocketAddress = default(GCHandle);
            Interop.libc.sockaddr* sockAddr = null;
            uint sockAddrLen = 0;

            int received = 0;
            int toReceive = 0, iovCount = maxBuffers;
            try
            {
                for (int i = 0; i < maxBuffers; i++)
                {
                    ArraySegment<byte> buffer = buffers[i];
                    handles[i] = GCHandle.Alloc(buffer.Array, GCHandleType.Pinned);
                    iovecs[i].iov_base = &((byte*)handles[i].AddrOfPinnedObject())[buffer.Offset];

                    int space = buffer.Count;
                    toReceive += space;
                    if (toReceive >= available)
                    {
                        iovecs[i].iov_len = (IntPtr)(space - (toReceive - available));
                        toReceive = available;
                        iovCount = i + 1;
                        break;
                    }

                    iovecs[i].iov_len = (IntPtr)space;
                }

                if (socketAddress != null)
                {
                    pinnedSocketAddress = GCHandle.Alloc(socketAddress, GCHandleType.Pinned);
                    sockAddr = (Interop.libc.sockaddr*)pinnedSocketAddress.AddrOfPinnedObject();
                    sockAddrLen = (uint)socketAddressLen;
                }

                // Make the call
                fixed (Interop.libc.iovec* iov = iovecs)
                {
                    var msghdr = new Interop.libc.msghdr {
                        msg_name = sockAddr,
                        msg_namelen = sockAddrLen,
                        msg_iov = iov,
                        msg_iovlen = (IntPtr)iovCount,
                        msg_control = null,
                        msg_controllen = IntPtr.Zero,
                    };

                    received = (int)Interop.libc.recvmsg(fd, &msghdr, flags);
                    receivedFlags = msghdr.msg_flags;
                }
            }
            finally
            {
                // Free GC handles
                for (int i = 0; i < iovCount; i++)
                {
                    if (handles[i].IsAllocated)
                    {
                        handles[i].Free();
                    }
                }

                if (pinnedSocketAddress.IsAllocated)
                {
                    pinnedSocketAddress.Free();
                }
            }

            if (received == -1)
            {
                errno = Interop.Sys.GetLastError();
                return -1;
            }

            socketAddressLen = (int)sockAddrLen;
            errno = Interop.Error.SUCCESS;
            return received;
        }

        public SocketError ReceiveAsync(byte[] buffer, int offset, int count, int flags, Action<int, byte[], int, int, SocketError> callback)
        {
            return ReceiveFromAsync(buffer, offset, count, flags, null, 0, callback);
        }

        public SocketError ReceiveFromAsync(byte[] buffer, int offset, int count, int flags, byte[] socketAddress, int socketAddressLen, Action<int, byte[], int, int, SocketError> callback)
        {
            int bytesReceived;
            int receivedFlags;
            SocketError errorCode;
            if (TryCompleteReceiveFrom(_fileDescriptor, buffer, offset, count, flags, socketAddress, ref socketAddressLen, out bytesReceived, out receivedFlags, out errorCode))
            {
                ThreadPool.QueueUserWorkItem(_ => callback(bytesReceived, socketAddress, socketAddressLen, receivedFlags, errorCode), null);
                return errorCode;
            }

            var operation = new TransferOperation {
                Callback = callback,
                Buffer = buffer,
                Offset = offset,
                Count = count,
                Flags = flags,
                SocketAddress = socketAddress,
                SocketAddressLen = socketAddressLen,
                BytesTransferred = bytesReceived,
                ReceivedFlags = receivedFlags
            };

            bool isStopped;
            while (!TryBeginOperation(ref _receiveQueue, operation, out isStopped))
            {
                if (isStopped)
                {
                    // TODO: is this error code reasonable for a closed socket? Check with Winsock.
                    operation.ErrorCode = SocketError.Shutdown;
                    QueueCompletion(operation);
                    return SocketError.Shutdown;
                }

                if (TryCompleteReceiveFrom(_fileDescriptor, operation))
                {
                    QueueCompletion(operation);
                    break;
                }
            }
            return SocketError.IOPending;
        }

        public SocketError ReceiveAsync(IList<ArraySegment<byte>> buffers, int flags, Action<int, byte[], int, int, SocketError> callback)
        {
            return ReceiveFromAsync(buffers, flags, null, 0, callback);
        }

        public SocketError ReceiveFromAsync(IList<ArraySegment<byte>> buffers, int flags, byte[] socketAddress, int socketAddressLen, Action<int, byte[], int, int, SocketError> callback)
        {
            int bytesReceived;
            int receivedFlags;
            SocketError errorCode;
            if (TryCompleteReceiveFrom(_fileDescriptor, buffers, flags, socketAddress, ref socketAddressLen, out bytesReceived, out receivedFlags, out errorCode))
            {
                ThreadPool.QueueUserWorkItem(_ => callback(bytesReceived, socketAddress, socketAddressLen, receivedFlags, errorCode), null);
                return errorCode;
            }

            var operation = new TransferOperation {
                Callback = callback,
                Buffers = new BufferList(buffers),
                Flags = flags,
                BytesTransferred = bytesReceived,
                ReceivedFlags = receivedFlags
            };

            bool isStopped;
            while (!TryBeginOperation(ref _receiveQueue, operation, out isStopped))
            {
                if (isStopped)
                {
                    // TODO: is this error code reasonable for a closed socket? Check with Winsock.
                    operation.ErrorCode = SocketError.Shutdown;
                    QueueCompletion(operation);
                    return SocketError.Shutdown;
                }

                if (TryCompleteReceiveFrom(_fileDescriptor, operation))
                {
                    QueueCompletion(operation);
                    break;
                }
            }
            return SocketError.IOPending;
        }

        private static bool TryCompleteReceiveFrom(int fileDescriptor, byte[] buffer, int offset, int count, int flags, byte[] socketAddress, ref int socketAddressLen, out int bytesReceived, out int receivedFlags, out SocketError errorCode)
        {
            return TryCompleteReceiveFrom(fileDescriptor, buffer, default(BufferList), offset, count, flags, socketAddress, ref socketAddressLen, out bytesReceived, out receivedFlags, out errorCode);
        }

        private static bool TryCompleteReceiveFrom(int fileDescriptor, IList<ArraySegment<byte>> buffers, int flags, byte[] socketAddress, ref int socketAddressLen, out int bytesReceived, out int receivedFlags, out SocketError errorCode)
        {
            return TryCompleteReceiveFrom(fileDescriptor, null, new BufferList(buffers), 0, 0, flags, socketAddress, ref socketAddressLen, out bytesReceived, out receivedFlags, out errorCode);
        }

        private static bool TryCompleteReceiveFrom(int fileDescriptor, TransferOperation operation)
        {
            return TryCompleteReceiveFrom(fileDescriptor, operation.Buffer, operation.Buffers, operation.Offset, operation.Count, operation.Flags, operation.SocketAddress, ref operation.SocketAddressLen, out operation.BytesTransferred, out operation.ReceivedFlags, out operation.ErrorCode);
        }

        private static unsafe bool TryCompleteReceiveFrom(int fileDescriptor, byte[] buffer, BufferList buffers, int offset, int count, int flags, byte[] socketAddress, ref int socketAddressLen, out int bytesReceived, out int receivedFlags, out SocketError errorCode)
        {
            int available;
            int err = Interop.libc.ioctl(fileDescriptor, (UIntPtr)Interop.libc.FIONREAD, &available);
            if (err == -1)
            {
                bytesReceived = 0;
                receivedFlags = 0;
                errorCode = SocketPal.GetLastSocketError();
                return true;
            }
            if (available == 0)
            {
                // Always request at least one byte.
                available = 1;
            }

            int received;
            Interop.Error errno;
            if (buffer != null)
            {
                received = Receive(fileDescriptor, flags, available, buffer, offset, count, socketAddress, ref socketAddressLen, out receivedFlags, out errno);
            }
            else
            {
                Debug.Assert(buffers.IsInitialized);
                received = Receive(fileDescriptor, flags, available, buffers, socketAddress, ref socketAddressLen, out receivedFlags, out errno);
            }

            if (received != -1)
            {
                bytesReceived = received;
                errorCode = SocketError.Success;
                return true;
            }

            bytesReceived = 0;

            if (errno != Interop.Error.EAGAIN && errno != Interop.Error.EWOULDBLOCK)
            {
                errorCode = SocketPal.GetSocketErrorForErrorCode(errno);
                return true;
            }

            errorCode = SocketError.Success;
            return false;
        }

        private static unsafe int Send(int fd, int flags, byte[] buffer, ref int offset, ref int count, byte[] socketAddress, int socketAddressLen, out Interop.Error errno)
        {
            var pinnedSocketAddress = default(GCHandle);
            Interop.libc.sockaddr* sockAddr = null;
            uint sockAddrLen = 0;

            int sent;
            try
            {
                if (socketAddress != null)
                {
                    pinnedSocketAddress = GCHandle.Alloc(socketAddress, GCHandleType.Pinned);
                    sockAddr = (Interop.libc.sockaddr*)pinnedSocketAddress.AddrOfPinnedObject();
                    sockAddrLen = (uint)socketAddressLen;
                }

                fixed (byte* b = buffer)
                {
                    sent = (int)Interop.libc.sendto(fd, &b[offset], (IntPtr)count, flags, sockAddr, sockAddrLen);
                }
            }
            finally
            {
                if (pinnedSocketAddress.IsAllocated)
                {
                    pinnedSocketAddress.Free();
                }
            }

            if (sent == -1)
            {
                errno = Interop.Sys.GetLastError();
                return -1;
            }

            errno = Interop.Error.SUCCESS;
            offset += sent;
            count -= sent;
            return sent;
        }

        private static unsafe int Send(int fd, int flags, BufferList buffers, ref int bufferIndex, ref int offset, byte[] socketAddress, int socketAddressLen, out Interop.Error errno)
        {
            // Pin buffers and set up iovecs
            int startIndex = bufferIndex, startOffset = offset;

            var pinnedSocketAddress = default(GCHandle);
            Interop.libc.sockaddr* sockAddr = null;
            uint sockAddrLen = 0;

            int maxBuffers = buffers.Count - startIndex;
            var handles = new GCHandle[maxBuffers];
            var iovecs = new Interop.libc.iovec[maxBuffers];

            int sent;
            int toSend = 0, iovCount = maxBuffers;
            try
            {
                for (int i = 0; i < maxBuffers; i++, startOffset = 0)
                {
                    ArraySegment<byte> buffer = buffers[startIndex + i];
                    handles[i] = GCHandle.Alloc(buffer.Array, GCHandleType.Pinned);
                    iovecs[i].iov_base = &((byte*)handles[i].AddrOfPinnedObject())[buffer.Offset + startOffset];

                    toSend += (buffer.Count - startOffset);
                    iovecs[i].iov_len = (IntPtr)(buffer.Count - startOffset);
                }

                if (socketAddress != null)
                {
                    pinnedSocketAddress = GCHandle.Alloc(socketAddress, GCHandleType.Pinned);
                    sockAddr = (Interop.libc.sockaddr*)pinnedSocketAddress.AddrOfPinnedObject();
                    sockAddrLen = (uint)socketAddressLen;
                }

                // Make the call
                fixed (Interop.libc.iovec* iov = iovecs)
                {
                    var msghdr = new Interop.libc.msghdr {
                        msg_name = sockAddr,
                        msg_namelen = sockAddrLen,
                        msg_iov = iov,
                        msg_iovlen = (IntPtr)iovCount,
                        msg_control = null,
                        msg_controllen = IntPtr.Zero,
                    };

                    sent = (int)Interop.libc.sendmsg(fd, &msghdr, flags);
                }
                errno = Interop.Sys.GetLastError();
            }
            finally
            {
                // Free GC handles
                for (int i = 0; i < iovCount; i++)
                {
                    if (handles[i].IsAllocated)
                    {
                        handles[i].Free();
                    }
                }

                if (pinnedSocketAddress.IsAllocated)
                {
                    pinnedSocketAddress.Free();
                }
            }

            if (sent == -1)
            {
                return -1;
            }

            // Update position
            int endIndex = bufferIndex, endOffset = offset, unconsumed = sent;
            for (; endIndex < buffers.Count && unconsumed > 0; endIndex++, endOffset = 0)
            {
                int space = buffers[endIndex].Count - endOffset;
                if (space > unconsumed)
                {
                    endOffset += unconsumed;
                    break;
                }
                unconsumed -= space;
            }

            bufferIndex = endIndex;
            offset = endOffset;

            return sent;
        }

        public SocketError SendAsync(byte[] buffer, int offset, int count, int flags, Action<int, byte[], int, int, SocketError> callback)
        {
            return SendToAsync(buffer, offset, count, flags, null, 0, callback);
        }

        public SocketError SendToAsync(byte[] buffer, int offset, int count, int flags, byte[] socketAddress, int socketAddressLen, Action<int, byte[], int, int, SocketError> callback)
        {
            int bytesSent = 0;
            SocketError errorCode;
            if (TryCompleteSendTo(_fileDescriptor, buffer, ref offset, ref count, flags, socketAddress, socketAddressLen, ref bytesSent, out errorCode))
            {
                ThreadPool.QueueUserWorkItem(_ => callback(bytesSent, socketAddress, socketAddressLen, 0, errorCode), null);
                return errorCode;
            }

            var operation = new TransferOperation {
                Callback = callback,
                Buffer = buffer,
                Offset = offset,
                Count = count,
                Flags = flags,
                SocketAddress = socketAddress,
                SocketAddressLen = socketAddressLen,
                BytesTransferred = bytesSent
            };

            bool isStopped;
            while (!TryBeginOperation(ref _sendQueue, operation, out isStopped))
            {
                if (isStopped)
                {
                    // TODO: is this error code reasonable for a closed socket? Check with Winsock.
                    operation.ErrorCode = SocketError.Shutdown;
                    QueueCompletion(operation);
                    return SocketError.Shutdown;
                }

                if (TryCompleteSendTo(_fileDescriptor, operation))
                {
                    QueueCompletion(operation);
                    break;
                }
            }
            return SocketError.IOPending;
        }

        public SocketError SendAsync(BufferList buffers, int flags, Action<int, byte[], int, int, SocketError> callback)
        {
            return SendToAsync(buffers, flags, null, 0, callback);
        }

        public SocketError SendToAsync(BufferList buffers, int flags, byte[] socketAddress, int socketAddressLen, Action<int, byte[], int, int, SocketError> callback)
        {
            int bufferIndex = 0;
            int offset = 0;
            int bytesSent = 0;
            SocketError errorCode;
            if (TryCompleteSendTo(_fileDescriptor, buffers, ref bufferIndex, ref offset, flags, socketAddress, socketAddressLen, ref bytesSent, out errorCode))
            {
                ThreadPool.QueueUserWorkItem(_ => callback(bytesSent, socketAddress, socketAddressLen, 0, errorCode), null);
                return errorCode;
            }

            var operation = new TransferOperation {
                Callback = callback,
                Buffers = buffers,
                BufferIndex = bufferIndex,
                Offset = offset,
                Flags = flags,
                SocketAddress = socketAddress,
                SocketAddressLen = socketAddressLen,
                BytesTransferred = bytesSent
            };

            bool isStopped;
            while (!TryBeginOperation(ref _sendQueue, operation, out isStopped))
            {
                if (isStopped)
                {
                    // TODO: is this error code reasonable for a closed socket? Check with Winsock.
                    operation.ErrorCode = SocketError.Shutdown;
                    QueueCompletion(operation);
                    return SocketError.Shutdown;
                }

                if (TryCompleteSendTo(_fileDescriptor, operation))
                {
                    QueueCompletion(operation);
                    break;
                }
            }
            return SocketError.IOPending;
        }

        private static bool TryCompleteSendTo(int fileDescriptor, byte[] buffer, ref int offset, ref int count, int flags, byte[] socketAddress, int socketAddressLen, ref int bytesSent, out SocketError errorCode)
        {
            int bufferIndex = 0;
            return TryCompleteSendTo(fileDescriptor, buffer, default(BufferList), ref bufferIndex, ref offset, ref count, flags, socketAddress, socketAddressLen, ref bytesSent, out errorCode);
        }

        private static bool TryCompleteSendTo(int fileDescriptor, BufferList buffers, ref int bufferIndex, ref int offset, int flags, byte[] socketAddress, int socketAddressLen, ref int bytesSent, out SocketError errorCode)
        {
            int count = 0;
            return TryCompleteSendTo(fileDescriptor, null, buffers, ref bufferIndex, ref offset, ref count, flags, socketAddress, socketAddressLen, ref bytesSent, out errorCode);
        }

        private static bool TryCompleteSendTo(int fileDescriptor, TransferOperation operation)
        {
            return TryCompleteSendTo(fileDescriptor, operation.Buffer, operation.Buffers, ref operation.BufferIndex, ref operation.Offset, ref operation.Count, operation.Flags, operation.SocketAddress, operation.SocketAddressLen, ref operation.BytesTransferred, out operation.ErrorCode);
        }

        private static bool TryCompleteSendTo(int fileDescriptor, byte[] buffer, BufferList buffers, ref int bufferIndex, ref int offset, ref int count, int flags, byte[] socketAddress, int socketAddressLen, ref int bytesSent, out SocketError errorCode)
        {
            for (;;)
            {
                int sent;
                Interop.Error errno;
                if (buffer != null)
                {
                    sent = Send(fileDescriptor, flags, buffer, ref offset, ref count, socketAddress, socketAddressLen, out errno);
                }
                else
                {
                    Debug.Assert(buffers.IsInitialized);
                    sent = Send(fileDescriptor, flags, buffers, ref bufferIndex, ref offset, socketAddress, socketAddressLen, out errno);
                }

                if (sent == -1)
                {
                    if (errno != Interop.Error.EAGAIN && errno != Interop.Error.EWOULDBLOCK)
                    {
                        errorCode = SocketPal.GetSocketErrorForErrorCode(errno);
                        return true;
                    }

                    errorCode = SocketError.Success;
                    return false;
                }

                bytesSent += sent;

                bool isComplete = sent == 0 ||
                    (buffer != null && count == 0) ||
                    (buffers.IsInitialized && bufferIndex == buffers.Count);
                if (isComplete)
                {
                    errorCode = SocketError.Success;
                    return true;
                }
            }
        }

        private static void QueueCompletion(AsyncOperation operation)
        {
            ThreadPool.QueueUserWorkItem(o => ((AsyncOperation)o).Complete(), operation);
        }

        public unsafe void HandleEvents(uint events)
        {
            lock (_closeLock)
            {
                if ((events & Interop.libc.EPOLLERR) != 0)
                {
                    int errno;
                    uint optLen = (uint)sizeof(int);
                    int err = Interop.libc.getsockopt(_fileDescriptor, Interop.libc.SOL_SOCKET, Interop.libc.SO_ERROR, &errno, &optLen);
                    if (err == -1)
                    {
                        // TODO: throw an appropiate exception
                        throw new Exception(string.Format("HandleEvents getsockopt: {0}", Interop.Sys.GetLastError()));
                    }

                    // TODO: error handling
                }

                // TODO: rethink ordering of HUP/RDHUP w.r.t. IN/OUT.

                if ((events & Interop.libc.EPOLLHUP) != 0)
                {
                    // Drain queues and unregister fd

                    Queue<AcceptOrConnectOperation> acceptOrConnectQueue;
                    Queue<TransferOperation> sendQueue;
                    Queue<TransferOperation> receiveQueue;
                    lock (_queueLock)
                    {
                        acceptOrConnectQueue = _acceptOrConnectQueue.Stop();
                        sendQueue = _sendQueue.Stop();
                        receiveQueue = _receiveQueue.Stop();

                        if (_registeredEvents != 0)
                        {
                            Interop.Error errorCode;
                            if (!_engine.TryUnregister(ref _handle, _fileDescriptor, 0, out errorCode))
                            {
                                if (errorCode != Interop.Error.EBADF)
                                {
                                    // TODO: throw an appropiate exception
                                    throw new Exception(string.Format("HandleEvents HUP: {0}", errorCode));
                                }
                                _handle.Free();
                            }

                            _registeredEvents = 0;
                        }

                        // TODO: assert that queues are all empty if _registeredEvents was zero?

                        Debug.Assert(!_handle.IsAllocated);
                    }

                    while (!acceptOrConnectQueue.IsEmpty)
                    {
                        AcceptOrConnectOperation op = acceptOrConnectQueue.Head;

                        bool completed;

                        var acceptOp = op as AcceptOperation;
                        if (acceptOp != null)
                        {
                            completed = TryCompleteAccept(_fileDescriptor, acceptOp);
                        }
                        else
                        {
                            completed = TryCompleteConnect(_fileDescriptor, (ConnectOperation)op);
                        }

                        Debug.Assert(completed);
                        acceptOrConnectQueue.Dequeue();
                        QueueCompletion(op);
                    }

                    while (!sendQueue.IsEmpty)
                    {
                        TransferOperation op = sendQueue.Head;
                        bool completed = TryCompleteSendTo(_fileDescriptor, op);
                        Debug.Assert(completed);
                        sendQueue.Dequeue();
                        QueueCompletion(op);
                    }

                    while (!receiveQueue.IsEmpty)
                    {
                        TransferOperation op = receiveQueue.Head;
                        bool completed = TryCompleteReceiveFrom(_fileDescriptor, op);
                        Debug.Assert(completed);
                        receiveQueue.Dequeue();
                        QueueCompletion(op);
                    }

                    return;
                }

                if ((events & Interop.libc.EPOLLRDHUP) != 0)
                {
                    // Drain read queue and unregister read operations
                    Debug.Assert(_acceptOrConnectQueue.IsEmpty);

                    Queue<TransferOperation> receiveQueue;
                    lock (_queueLock)
                    {
                        receiveQueue = _receiveQueue.Stop();
                    }

                    while (!receiveQueue.IsEmpty)
                    {
                        TransferOperation op = receiveQueue.Head;
                        bool completed = TryCompleteReceiveFrom(_fileDescriptor, op);
                        Debug.Assert(completed);
                        receiveQueue.Dequeue();
                        QueueCompletion(op);
                    }

                    lock (_queueLock)
                    {
                        Interop.Error errorCode;
                        uint evts = _registeredEvents & ~(Interop.libc.EPOLLIN | Interop.libc.EPOLLRDHUP);
                        if (!_engine.TryUnregister(ref _handle, _fileDescriptor, evts, out errorCode))
                        {
                            // TODO: throw an appropiate exception
                            throw new Exception(string.Format("HandleEvents RDHUP: {0}", errorCode));
                        }

                        _registeredEvents = evts;
                    }

                    // Don't process received data if we saw a RDHUP.
                    events &= ~Interop.libc.EPOLLIN;
                }

                // TODO: optimize locking and completions:
                // - Dequeues (and therefore locking) for multiple contiguous operations can be combined
                // - Contiguous completions can happen in a single thread

                if ((events & Interop.libc.EPOLLIN) != 0)
                {
                    AcceptOrConnectOperation acceptTail;
                    TransferOperation receiveTail;
                    lock (_queueLock)
                    {
                        acceptTail = _acceptOrConnectQueue.Tail;
                        _acceptOrConnectQueue.State = State.Set;

                        receiveTail = _receiveQueue.Tail;
                        _receiveQueue.State = State.Set;
                    }

                    if (acceptTail != null)
                    {
                        AcceptOperation op;
                        do
                        {
                            op = (AcceptOperation)_acceptOrConnectQueue.Head;
                            if (TryCompleteAccept(_fileDescriptor, op))
                            {
                                EndOperation(ref _acceptOrConnectQueue);
                                QueueCompletion(op);
                            }
                            break;
                        } while (op != acceptTail);
                    }

                    if (receiveTail != null)
                    {
                        TransferOperation op;
                        do
                        {
                            op = _receiveQueue.Head;
                            if (TryCompleteReceiveFrom(_fileDescriptor, op))
                            {
                                EndOperation(ref _receiveQueue);
                                QueueCompletion(op);
                            }
                            break;
                        } while (op != receiveTail);
                    }
                }

                if ((events & Interop.libc.EPOLLOUT) != 0)
                {
                    AcceptOrConnectOperation connectTail;
                    TransferOperation sendTail;
                    lock (_queueLock)
                    {
                        connectTail = _acceptOrConnectQueue.Tail;
                        _acceptOrConnectQueue.State = State.Set;

                        sendTail = _sendQueue.Tail;
                        _sendQueue.State = State.Set;
                    }

                    if (connectTail != null)
                    {
                        ConnectOperation op;
                        do
                        {
                            op = (ConnectOperation)_acceptOrConnectQueue.Head;
                            if (TryCompleteConnect(_fileDescriptor, op))
                            {
                                EndOperation(ref _acceptOrConnectQueue);
                                QueueCompletion(op);
                            }
                            break;
                        } while (op != connectTail);
                    }

                    if (sendTail != null)
                    {
                        TransferOperation op;
                        do
                        {
                            op = _sendQueue.Head;
                            if (TryCompleteSendTo(_fileDescriptor, op))
                            {
                                EndOperation(ref _sendQueue);
                                QueueCompletion(op);
                            }
                            break;
                        } while (op != sendTail);
                    }
                }
            }
        }
    }
}
