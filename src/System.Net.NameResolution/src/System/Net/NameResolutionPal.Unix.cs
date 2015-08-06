// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net.Sockets;

namespace System.Net
{
    internal static class NameResolutionPal
    {
        private static unsafe IPHostEntry CreateHostEntry(Interop.libc.hostent* hostent)
        {
            var hostEntry = new IPHostEntry();
            if (hostent->h_name != null)
            {
                hostEntry.HostName = Marshal.PtrToStringAnsi((IntPtr)hostent->h_name);
            }

            int numAddresses;
            for (numAddresses = 0; hostent->h_addr_list[numAddresses] != null; numAddresses++)
            {
            }

            IPAddress[] ipAddresses;
            if (numAddresses == 0)
            {
                ipAddresses = Array.Empty<IPAddress>();
            }
            else
            {
                ipAddresses = new IPAddress[numAddresses];
                for (int i = 0; i < numAddresses; i++)
                {
                    Debug.Assert(hostent->h_addr_list[i] != null);
                    ipAddresses[i] = *(int*)hostent->h_addr_list[i];
                }
            }
            hostEntry.AddressList = ipAddresses;

            int numAliases;
            for (numAliases = 0; hostent->h_aliases[numAliases] != null; numAliases++)
            {
            }

            string[] aliases;
            if (numAliases == 0)
            {
                aliases = Array.Empty<string>();
            }
            else
            {
                aliases = new string[numAliases];
                for (int i = 0; i < numAddresses; i++)
                {
                    Debug.Assert(hostent->h_addr_list[i] != null);
                    aliases[i] = Marshal.PtrToStringAnsi((IntPtr)hostent->h_addr_list[i]);
                }
            }
        }

        private static SocketError GetSocketErrorForNativeError(uint error)
        {
            switch (error)
            {
                case 0:
                    return SocketError.Success;
                case Interop.libc.EAI_AGAIN:
                    return SocketError.TryAgain;
                case Interop.libc.EAI_BADFLAGS:
                    return SocketError.InvalidArgument;
                case Interop.libc.EAI_FAIL:
                    return SocketError.NoRecovery;
                case Interop.libc.EAI_FAMILY:
                    return SocketError.AddressFamilyNotSupported;
                case Interop.libc.EAI_NONAME:
                    return SocketError.HostNotFound;
            }
        }

        public static IPHostEntry GetHostByName(string hostName)
        {
            Interop.libc.hostent* hostent = Interop.NativeNameResolution.GetHostByName(hostName);
            if (hostent == null)
            {
                throw new SocketException();
            }

            IPHostEntry hostEntry = CreateHostEntry(hostent);
            Interop.NativeNameResolution.FreeHostEntry(hostent);
            return hostEntry;
        }


        public static IPHostEntry GetHostByAddr(IPAddress address)
        {
            // TODO: Optimize this (or decide if this legacy code can be removed):
            byte [] addressBytes = address.GetAddressBytes();
            int address = BitConverter.ToInt32(addressBytes, 0);

            Interop.libc.hostent* hostent = Interop.NativeNameResolution.GetHostByAddr(address);
            if (hostent == null)
            {
                throw new SocketException();
            }

            IPHostEntry hostEntry = CreateHostEntry(hostent);
            Interop.NativeNameResolution.FreeHostEntry(hostent);
            return hostEntry;
        }

        public static unsafe SocketError TryGetAddrInfo(string name, out IPHostEntry hostinfo)
        {
            var hints = new Interop.libc.addrinfo {
                ai_family = Interop.libc.AF_UNSPEC, // Get all address families
                ai_flags = Interop.libc.AI_CANONNAME
            };

            AddrInfoHandle root;
            uint errorCode = Interop.libc.getaddrinfo(name, null, &hints, out root);
            if (errorCode != 0)
            {
                Debug.Assert(root == null);
                return NameResolutionUtilities.GetUnresolvedAnswer(name);
            }

            string canonicalName = null;
            IPAddress[] ipAddresses;
            using (root)
            {
                var addrinfo = (Interop.libc.addrinfo*)root.DangerousGetHandle();

                int numAddresses = 0;
                for (Interop.libc.addrinfo* ai = addrinfo; ai != null; ai = ai.ai_next)
                {
                    if (canonicalName == null && ai->ai_canonname != null)
                    {
                        canonicalName = Marshal.PtrToStringAnsi((IntPtr)ai->ai_canonname);
                    }

                    if ((ai->ai_family != Interop.libc.AF_INET) &&
                        (ai->ai_family != Interop.libc.AF_INET6 || !Socket.OSSupportsIPv6))
                    {
                        continue;
                    }

                    numAddresses++;
                }

                if (numAddresses == 0)
                {
                    ipAddresses = Array.Empty<IPAddress>();
                }
                else
                {
                    ipAddresses = new IPAddress[numAddresses];
                    for (int i = 0; i < numAddresses; addrinfo = addrinfo.ai_next)
                    {
                        Debug.Assert(addrinfo != null);

                        if ((addrinfo->ai_family != Interop.libc.AF_INET) &&
                            (addrinfo->ai_family != Interop.libc.AF_INET6 || Socket.OSSupportsIPv6))
                        {
                            continue;
                        }

                        var sockaddr = new SocketAddress(addrinfo->ai_family, addrinfo->ai_addrlen);
                        for (int d = 0; d < addrinfo->ai_addrlen; d++)
                        {
                            sockaddr[d] = ((byte*)addrinfo->ai_addr)[d];
                        }

                        if (addrinfo->ai_family == Interop.libc.AF_INET)
                        {
                            ipAddresses[i] = (IPEndPoint)IPEndPointStatics.Any.Create(sockaddr).Address;
                        }
                        else
                        {
                            ipAddresses[i] = (IPEndPoint)IPEndPointStatics.IPv6Any.Create(sockaddr).Address;
                        }

                        i++;
                    }
                }
            }

            hostinfo = new IPHostEntry {
                HostName = canonicalName ?? name,
                Aliases = Array.Empty<string>(),
                AddressList = ipAddresses
            };
            return SocketError.Success;
        }

        public static string TryGetNameInfo(IPAddress address, out SocketError socketError, out int nativeError)
        {
            SocketAddress address = (new IPEndPoint(addr, 0)).Serialize();
            StringBuilder hostname = new StringBuilder(Interop.libc.NI_MAXHOST);

            // TODO: Remove the copying step to improve performance. This requires a change in the contracts.
            byte[] addressBuffer = new byte[address.Size];
            for (int i = 0; i < address.Size; i++)
            {
                addressBuffer[i] = address[i];
            }

            uint error = Interop.libc.getnameinfo(
                addressBuffer,
                addressBuffer.Length,
                hostname,
                hostname.Capacity,
                null,
                0,
                Interop.libc.NI_NAMEREQD);

            socketError = GetSocketErrorForNativeError(error);
            nativeError = (int)error;

            return socketError != SocketError.Success ? null : hostname.ToString();
        }

        public static string GetHostName()
        {
            return Interop.libc.gethostname();
        }
    }
}
