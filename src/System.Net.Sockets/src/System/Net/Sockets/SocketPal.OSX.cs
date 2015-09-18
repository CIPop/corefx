// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace System.Net.Sockets
{
    internal static partial class SocketPal
    {
        public const bool SupportsMultipleConnectAttempts = false;

        public static void PrimeForNextConnectAttempt(int fileDescriptor, int socketAddressLen)
        {
            Debug.Fail("This should never be called!");
        }
    }
}
