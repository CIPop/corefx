// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;
using System.Net.Sockets;
using Microsoft.Win32.SafeHandles;

internal static partial class Interop
{
    internal static partial class Winsock
    {
        [DllImport(Interop.Libraries.Ws2_32, SetLastError = true)]
        internal static extern SocketError WSAEnumNetworkEvents(
            [In] SafeCloseSocket socketHandle,
            [In] SafeWaitHandle Event,
            [In, Out] ref NetworkEvents networkEvents);
    }
}
