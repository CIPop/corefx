// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

using Microsoft.Win32.SafeHandles;

namespace System.Net.NetworkInformation
{
    internal class HostInformation
    {
        private static FIXED_INFO s_fixedInfo;
        private static bool s_fixedInfoInitialized = false;

        //changing these require a reboot, so we'll cache them instead.
        private static volatile string s_hostName = null;
        private static volatile string s_domainName = null;

        private static object s_syncObject = new object();

        internal static FIXED_INFO GetFixedInfo()
        {
            uint size = 0;
            SafeLocalAllocHandle buffer = null;
            FIXED_INFO fixedInfo = new FIXED_INFO();

            //first we need to get the size of the buffer
            uint result = Interop.IpHlpApi.GetNetworkParams(SafeLocalAllocHandle.InvalidHandle, ref size);

            while (result == Interop.IpHlpApi.ERROR_BUFFER_OVERFLOW)
            {
                try
                {
                    // Now we allocate the buffer and read the network parameters.
                    buffer = Interop.mincore_obsolete.LocalAlloc(0, (UIntPtr)size);
                    if (buffer.IsInvalid)
                    {
                        throw new OutOfMemoryException();
                    }

                    result = Interop.IpHlpApi.GetNetworkParams(buffer, ref size);
                    if (result == Interop.IpHlpApi.ERROR_SUCCESS)
                    {
                        fixedInfo = Marshal.PtrToStructure<FIXED_INFO>(buffer.DangerousGetHandle());
                    }
                }
                finally
                {
                    if (buffer != null)
                    {
                        buffer.Dispose();
                    }
                }
            }

            //if the result include there being no information, we'll still throw
            if (result != Interop.IpHlpApi.ERROR_SUCCESS)
            {
                throw new NetworkInformationException((int)result);
            }
            return fixedInfo;
        }


        internal static FIXED_INFO FixedInfo
        {
            get
            {
                if (!s_fixedInfoInitialized)
                {
                    lock (s_syncObject)
                    {
                        if (!s_fixedInfoInitialized)
                        {
                            s_fixedInfo = GetFixedInfo();
                            s_fixedInfoInitialized = true;
                        }
                    }
                }
                return s_fixedInfo;
            }
        }

        /// <summary>Specifies the host name for the local computer.</summary>
        internal static string HostName
        {
            get
            {
                if (s_hostName == null)
                {
                    lock (s_syncObject)
                    {
                        if (s_hostName == null)
                        {
                            s_hostName = FixedInfo.hostName;
                            s_domainName = FixedInfo.domainName;
                        }
                    }
                }
                return s_hostName;
            }
        }
        /// <summary>Specifies the domain in which the local computer is registered.</summary>
        internal static string DomainName
        {
            get
            {
                if (s_domainName == null)
                {
                    lock (s_syncObject)
                    {
                        if (s_domainName == null)
                        {
                            s_hostName = FixedInfo.hostName;
                            s_domainName = FixedInfo.domainName;
                        }
                    }
                }
                return s_domainName;
            }
        }
    }
}

