// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;
using System.Diagnostics;

namespace System.Net.Sockets
{
    internal sealed class SafeCloseSocketAndEvent : SafeCloseSocket
    {
        internal SafeCloseSocketAndEvent() : base() { }
        private AutoResetEvent _waitHandle;

        override protected bool ReleaseHandle()
        {
            bool result = base.ReleaseHandle();
            DeleteEvent();
            return result;
        }

        internal static SafeCloseSocketAndEvent CreateWSASocketWithEvent(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType, bool autoReset, bool signaled)
        {
            SafeCloseSocketAndEvent result = new SafeCloseSocketAndEvent();
            CreateSocket(InnerSafeCloseSocket.CreateWSASocket(addressFamily, socketType, protocolType), result);
            if (result.IsInvalid)
            {
                throw new SocketException();
            }

            result._waitHandle = new AutoResetEvent(false);
            CompleteInitialization(result);
            return result;
        }

        internal static void CompleteInitialization(SafeCloseSocketAndEvent socketAndEventHandle)
        {
            SafeWaitHandle handle = socketAndEventHandle._waitHandle.GetSafeWaitHandle();
            bool b = false;
            try
            {
                handle.DangerousAddRef(ref b);
            }
            catch
            {
                if (b)
                {
                    handle.DangerousRelease();
                    socketAndEventHandle._waitHandle = null;
                    b = false;
                }
            }
            finally
            {
                if (b)
                {
                    handle.Dispose();
                }
            }
        }

        private void DeleteEvent()
        {
            try
            {
                if (_waitHandle != null)
                {
                    var waitHandleSafeWaitHandle = _waitHandle.GetSafeWaitHandle();
                    waitHandleSafeWaitHandle.DangerousRelease();
                }
            }
            catch
            {
            }
        }

        internal WaitHandle GetEventHandle()
        {
            return _waitHandle;
        }
    }
}
