// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32;
using System.Collections.Generic;

namespace System.Net.Sockets
{
    //
    //  OverlappedAsyncResult - used to take care of storage for async Socket operation
    //   from the BeginSend, BeginSendTo, BeginReceive, BeginReceiveFrom calls.
    //
    internal partial class OverlappedAsyncResult : BaseOverlappedAsyncResult
    {
        // These two are used only as alternatives
        internal WSABuffer m_SingleBuffer;
        internal WSABuffer[] m_WSABuffers;

        //
        // the following two will be used only on WinNT to enable completion ports
        //

        //
        internal IntPtr GetSocketAddressPtr()
        {
            return Marshal.UnsafeAddrOfPinnedArrayElement(_socketAddress.Buffer, 0);
        }
        //
        internal IntPtr GetSocketAddressSizePtr()
        {
            return Marshal.UnsafeAddrOfPinnedArrayElement(_socketAddress.Buffer, _socketAddress.GetAddressSizeOffset());
        }

        internal unsafe int GetSocketAddressSize()
        {
            return *(int*)GetSocketAddressSizePtr();
        }

        //
        // SetUnmanagedStructures -
        // Fills in Overlapped Structures used in an Async Overlapped Winsock call
        //   these calls are outside the runtime and are unmanaged code, so we need
        //   to prepare specific structures and ints that lie in unmanaged memory
        //   since the Overlapped calls can be Async
        //

        internal void SetUnmanagedStructures(byte[] buffer, int offset, int size, Internals.SocketAddress socketAddress, bool pinSocketAddress)
        {
            //
            // Fill in Buffer Array structure that will be used for our send/recv Buffer
            //
            _socketAddress = socketAddress;
            if (pinSocketAddress && _socketAddress != null)
            {
                object[] objectsToPin = null;
                objectsToPin = new object[2];
                objectsToPin[0] = buffer;

                _socketAddress.CopyAddressSizeIntoBuffer();
                objectsToPin[1] = _socketAddress.Buffer;

                base.SetUnmanagedStructures(objectsToPin);
            }
            else
            {
                base.SetUnmanagedStructures(buffer);
            }

            m_SingleBuffer.Length = size;
            m_SingleBuffer.Pointer = Marshal.UnsafeAddrOfPinnedArrayElement(buffer, offset);
        }

        internal void SetUnmanagedStructures(BufferOffsetSize[] buffers)
        {
            //
            // Fill in Buffer Array structure that will be used for our send/recv Buffer
            //
            m_WSABuffers = new WSABuffer[buffers.Length];

            object[] objectsToPin = new object[buffers.Length];
            for (int i = 0; i < buffers.Length; i++)
                objectsToPin[i] = buffers[i].Buffer;

            // has to be called now to pin memory
            base.SetUnmanagedStructures(objectsToPin);

            for (int i = 0; i < buffers.Length; i++)
            {
                m_WSABuffers[i].Length = buffers[i].Size;
                m_WSABuffers[i].Pointer = Marshal.UnsafeAddrOfPinnedArrayElement(buffers[i].Buffer, buffers[i].Offset);
            }
        }

        internal void SetUnmanagedStructures(IList<ArraySegment<byte>> buffers)
        {
            // Fill in Buffer Array structure that will be used for our send/recv Buffer
            //

            //make sure we don't let the app mess up the buffer array enough to cause
            //corruption.
            int count = buffers.Count;
            ArraySegment<byte>[] buffersCopy = new ArraySegment<byte>[count];

            for (int i = 0; i < count; i++)
            {
                buffersCopy[i] = buffers[i];
                RangeValidationHelpers.ValidateSegment(buffersCopy[i]);
            }

            m_WSABuffers = new WSABuffer[count];

            object[] objectsToPin = new object[count];
            for (int i = 0; i < count; i++)
                objectsToPin[i] = buffersCopy[i].Array;

            base.SetUnmanagedStructures(objectsToPin);

            for (int i = 0; i < count; i++)
            {
                m_WSABuffers[i].Length = buffersCopy[i].Count;
                m_WSABuffers[i].Pointer = Marshal.UnsafeAddrOfPinnedArrayElement(buffersCopy[i].Array, buffersCopy[i].Offset);
            }
        }

        //
        // This method is called after an asynchronous call is made for the user,
        // it checks and acts accordingly if the IO:
        // 1) completed synchronously.
        // 2) was pended.
        // 3) failed.
        //
        internal override object PostCompletion(int numBytes)
        {
            if (ErrorCode == 0)
            {
                if (Logging.On) LogBuffer(numBytes);
            }
            return (int)numBytes;
        }

        private void LogBuffer(int size)
        {
            GlobalLog.Assert(Logging.On, "OverlappedAsyncResult#{0}::LogBuffer()|Logging is off!", Logging.HashString(this));
            if (size > -1)
            {
                if (m_WSABuffers != null)
                {
                    foreach (WSABuffer wsaBuffer in m_WSABuffers)
                    {
                        Logging.Dump(Logging.Sockets, AsyncObject, "PostCompletion", wsaBuffer.Pointer, Math.Min(wsaBuffer.Length, size));
                        if ((size -= wsaBuffer.Length) <= 0)
                            break;
                    }
                }
                else
                {
                    Logging.Dump(Logging.Sockets, AsyncObject, "PostCompletion", m_SingleBuffer.Pointer, Math.Min(m_SingleBuffer.Length, size));
                }
            }
        }
    }; // class OverlappedAsyncResult
} // namespace System.Net.Sockets
