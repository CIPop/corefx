// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.Net
{
    // Defines the transport type allowed for the socket.
    public enum TransportType
    {
        // Udp connections are allowed.
        Udp = 0x1,
        Connectionless = 1,

        // TCP connections are allowed.
        Tcp = 0x2,
        ConnectionOriented = 2,

        // Any connection is allowed.
        All = 0x3
    }
}
