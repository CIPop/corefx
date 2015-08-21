﻿using System.IO;
using System.Net.Sockets;
using System.Net.Test.Common;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

using Xunit;
using Xunit.Abstractions;

namespace System.Net.Security.Tests
{
    public class ServerNoEncryptionTest
    {
        private readonly ITestOutputHelper _log;
        private DummyTcpServer serverNoEncryption;

        public ServerNoEncryptionTest()
        {
            _log = TestLogging.GetInstance();
            serverNoEncryption = new DummyTcpServer(
                new IPEndPoint(IPAddress.Loopback, 402), EncryptionPolicy.NoEncryption);
        }

        // The following method is invoked by the RemoteCertificateValidationDelegate.
        public bool AllowAnyServerCertificate(
              object sender,
              X509Certificate certificate,
              X509Chain chain,
              SslPolicyErrors sslPolicyErrors)
        {
            return true;  // allow everything
        }

        [Fact]
        public void ServerNoEncryption_ClientRequireEncryption_NoConnect()
        {
            
            using (var client = new TcpClient())
            {
                client.Connect(serverNoEncryption.RemoteEndPoint);

                using (var sslStream = new SslStream(client.GetStream(), false, AllowAnyServerCertificate, null, EncryptionPolicy.RequireEncryption))
                {
                    Assert.Throws<IOException>(() => {
                        sslStream.AuthenticateAsClient("localhost", null, TestConfiguration.DefaultSslProtocols, false);
                    });
                }
            }
        }

        [Fact]
        public void ServerNoEncryption_ClientAllowNoEncryption_ConnectWithNoEncryption()
        {
            SslStream sslStream;
            TcpClient client;

            client = new TcpClient();
            client.Connect(serverNoEncryption.RemoteEndPoint);

            sslStream = new SslStream(client.GetStream(), false, AllowAnyServerCertificate, null, EncryptionPolicy.AllowNoEncryption);
            sslStream.AuthenticateAsClient("localhost", null, TestConfiguration.DefaultSslProtocols, false);

            _log.WriteLine("Client({0}) authenticated to server({1}) with encryption cipher: {2} {3}-bit strength",
                client.Client.LocalEndPoint, client.Client.RemoteEndPoint,
                sslStream.CipherAlgorithm, sslStream.CipherStrength);

            CipherAlgorithmType expected = CipherAlgorithmType.Null;
            Assert.Equal(expected, sslStream.CipherAlgorithm);
            Assert.Equal(0, sslStream.CipherStrength);
            sslStream.Dispose();
            client.Dispose();
        }

        [Fact]
        public void ServerNoEncryption_ClientNoEncryption_ConnectWithNoEncryption()
        {
            SslStream sslStream;
            TcpClient client;

            client = new TcpClient();
            client.Connect(serverNoEncryption.RemoteEndPoint);

            sslStream = new SslStream(client.GetStream(), false, AllowAnyServerCertificate, null, EncryptionPolicy.NoEncryption);
            sslStream.AuthenticateAsClient("localhost", null, TestConfiguration.DefaultSslProtocols, false);
            _log.WriteLine("Client({0}) authenticated to server({1}) with encryption cipher: {2} {3}-bit strength",
                client.Client.LocalEndPoint, client.Client.RemoteEndPoint,
                sslStream.CipherAlgorithm, sslStream.CipherStrength);

            CipherAlgorithmType expected = CipherAlgorithmType.Null;
            Assert.Equal(expected, sslStream.CipherAlgorithm);
            Assert.Equal(0, sslStream.CipherStrength);
            sslStream.Dispose();
            client.Dispose();
        }
    }
}

