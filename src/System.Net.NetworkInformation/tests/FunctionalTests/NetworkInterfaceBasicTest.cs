// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net.Test.Common;

using Xunit;
using Xunit.Abstractions;

namespace System.Net.NetworkInformation.Tests
{
    public class NetworkInterfaceBasicTest
    {
        private readonly ITestOutputHelper _log;

        public NetworkInterfaceBasicTest()
        {
            _log = TestLogging.GetInstance();
        }

        [Fact]
        public void BasicTest_GetNetworkInterfaces_AtLeastOne()
        {
            Assert.NotEqual<int>(0, NetworkInterface.GetAllNetworkInterfaces().Length);
        }

        [Fact]
        public void BasicTest_AccessInstanceProperties_NoExceptions()
        {
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                _log.WriteLine("- NetworkInterface -");
                _log.WriteLine("Name: " + nic.Name);
                _log.WriteLine("Description: " + nic.Description);
                _log.WriteLine("ID: " + nic.Id);
                _log.WriteLine("IsReceiveOnly: " + nic.IsReceiveOnly);
                _log.WriteLine("Type: " + nic.NetworkInterfaceType);
                _log.WriteLine("Status: " + nic.OperationalStatus);
                _log.WriteLine("Speed: " + nic.Speed);
                Assert.True(nic.Speed >= 0, "Overflow");
                _log.WriteLine("SupportsMulticast: " + nic.SupportsMulticast);
                _log.WriteLine("GetPhysicalAddress(): " + nic.GetPhysicalAddress());
            }
        }

        [Fact]
        [Trait("IPv4", "true")]
        public void BasicTest_StaticLoopbackIndex_MatchesLoopbackNetworkInterface()
        {
            Assert.True(Capability.IPv4Support());

            _log.WriteLine("Loopback IPv4 index: " + NetworkInterface.LoopbackInterfaceIndex);
            
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                foreach (UnicastIPAddressInformation unicast in nic.GetIPProperties().UnicastAddresses)
                {
                    if (unicast.Address.Equals(IPAddress.Loopback))
                    {
                        Assert.Equal<int>(nic.GetIPProperties().GetIPv4Properties().Index, 
                            NetworkInterface.LoopbackInterfaceIndex);
                        return; // Only check IPv4 loopback
                    }
                }
            }
        }

        [Fact]
        [Trait("IPv4", "true")]
        public void BasicTest_StaticLoopbackIndex_ExceptionIfV4NotSupported()
        {
            Assert.True(Capability.IPv4Support());
            
            _log.WriteLine("Loopback IPv4 index: " + NetworkInterface.LoopbackInterfaceIndex);
        }

        [Fact]
        [Trait("IPv6", "true")]
        public void BasicTest_StaticIPv6LoopbackIndex_MatchesLoopbackNetworkInterface()
        {
            Assert.True(Capability.IPv6Support());

            _log.WriteLine("Loopback IPv6 index: " + NetworkInterface.IPv6LoopbackInterfaceIndex);

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                foreach (UnicastIPAddressInformation unicast in nic.GetIPProperties().UnicastAddresses)
                {
                    if (unicast.Address.Equals(IPAddress.IPv6Loopback))
                    {
                        Assert.Equal<int>(nic.GetIPProperties().GetIPv6Properties().Index, 
                            NetworkInterface.IPv6LoopbackInterfaceIndex);
                        return; // Only check IPv6 loopback
                    }
                }
            }
        }

        [Fact]
        [Trait("IPv6", "true")]
        public void BasicTest_StaticIPv6LoopbackIndex_ExceptionIfV6NotSupported()
        {
            Assert.True(Capability.IPv6Support());
            _log.WriteLine("Loopback IPv6 index: " + NetworkInterface.IPv6LoopbackInterfaceIndex);
        }

        [Fact]
        public void BasicTest_GetIPv4InterfaceStatistics_Success()
        {
            // This API is not actually IPv4 specific.
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                IPv4InterfaceStatistics stats = nic.GetIPv4Statistics();

                _log.WriteLine("- Stats for : " + nic.Name);
                _log.WriteLine("BytesReceived: " + stats.BytesReceived);
                _log.WriteLine("BytesSent: " + stats.BytesSent);
                _log.WriteLine("IncomingPacketsDiscarded: " + stats.IncomingPacketsDiscarded);
                _log.WriteLine("IncomingPacketsWithErrors: " + stats.IncomingPacketsWithErrors);
                _log.WriteLine("IncomingUnknownProtocolPackets: " + stats.IncomingUnknownProtocolPackets);
                _log.WriteLine("NonUnicastPacketsReceived: " + stats.NonUnicastPacketsReceived);
                _log.WriteLine("NonUnicastPacketsSent: " + stats.NonUnicastPacketsSent);
                _log.WriteLine("OutgoingPacketsDiscarded: " + stats.OutgoingPacketsDiscarded);
                _log.WriteLine("OutgoingPacketsWithErrors: " + stats.OutgoingPacketsWithErrors);
                _log.WriteLine("OutputQueueLength: " + stats.OutputQueueLength);
                _log.WriteLine("UnicastPacketsReceived: " + stats.UnicastPacketsReceived);
                _log.WriteLine("UnicastPacketsSent: " + stats.UnicastPacketsSent);
            }
        }
        
        [Fact]
        public void BasicTest_CompareGetIPv4InterfaceStatisticsWithGetIPInterfaceStatictics_Success()
        {
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                // These APIs call the same native API that returns more than just IPv4 results
                // so we added GetIPStatistics() as a more accurate name.
                IPv4InterfaceStatistics v4stats = nic.GetIPv4Statistics();
                IPInterfaceStatistics stats = nic.GetIPStatistics();

                // Verify that the by the second call the stats should have increased, or at least not decreased.
                Assert.True(v4stats.BytesReceived <= stats.BytesReceived, "BytesReceived did not increase");
                Assert.True(v4stats.BytesSent <= stats.BytesSent, "BytesSent decreased");
                Assert.True(v4stats.IncomingPacketsDiscarded <= stats.IncomingPacketsDiscarded,
                    "IncomingPacketsDiscarded decreased");
                Assert.True(v4stats.IncomingPacketsWithErrors <= stats.IncomingPacketsWithErrors,
                    "IncomingPacketsWithErrors decreased");
                Assert.True(v4stats.IncomingUnknownProtocolPackets <= stats.IncomingUnknownProtocolPackets,
                    "IncomingUnknownProtocolPackets decreased");
                Assert.True(v4stats.NonUnicastPacketsReceived <= stats.NonUnicastPacketsReceived,
                    "NonUnicastPacketsReceived decreased");
                Assert.True(v4stats.NonUnicastPacketsSent <= stats.NonUnicastPacketsSent,
                    "NonUnicastPacketsSent decreased");
                Assert.True(v4stats.OutgoingPacketsDiscarded <= stats.OutgoingPacketsDiscarded,
                    "OutgoingPacketsDiscarded decreased");
                Assert.True(v4stats.OutgoingPacketsWithErrors <= stats.OutgoingPacketsWithErrors,
                    "OutgoingPacketsWithErrors decreased");
                Assert.True(v4stats.OutputQueueLength <= stats.OutputQueueLength,
                    "OutputQueueLength decreased");
                Assert.True(v4stats.UnicastPacketsReceived <= stats.UnicastPacketsReceived,
                    "UnicastPacketsReceived decreased");
                Assert.True(v4stats.UnicastPacketsSent <= stats.UnicastPacketsSent,
                    "UnicastPacketsSent decreased");
            }
        }

        [Fact]
        public void BasicTest_GetIsNetworkAvailable_Success()
        {
            Assert.True(NetworkInterface.GetIsNetworkAvailable());
        }
    }
}
