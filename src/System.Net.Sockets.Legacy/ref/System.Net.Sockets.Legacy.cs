// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// ------------------------------------------------------------------------------
// Changes to this file must follow the http://aka.ms/api-review process.
// ------------------------------------------------------------------------------


namespace System.Net.Sockets
{
   
    public static class Legacy
    {
        public static System.IAsyncResult BeginAccept(this System.Net.Sockets.Socket thisSocket, System.AsyncCallback callback, object state) { return default(System.IAsyncResult); }
        public static System.IAsyncResult BeginAccept(this System.Net.Sockets.Socket thisSocket, int receiveSize, System.AsyncCallback callback, object state) { return default(System.IAsyncResult); }
        public static System.IAsyncResult BeginAccept(this System.Net.Sockets.Socket thisSocket, System.Net.Sockets.Socket acceptSocket, int receiveSize, System.AsyncCallback callback, object state) { return default(System.IAsyncResult); }
        public static System.IAsyncResult BeginConnect(this System.Net.Sockets.Socket thisSocket, System.Net.EndPoint remoteEP, System.AsyncCallback callback, object state) { return default(System.IAsyncResult); }
        public static System.IAsyncResult BeginConnect(this System.Net.Sockets.Socket thisSocket, System.Net.IPAddress address, int port, System.AsyncCallback requestCallback, object state) { return default(System.IAsyncResult); }
        public static System.IAsyncResult BeginConnect(this System.Net.Sockets.Socket thisSocket, System.Net.IPAddress[] addresses, int port, System.AsyncCallback requestCallback, object state) { return default(System.IAsyncResult); }
        public static System.IAsyncResult BeginConnect(this System.Net.Sockets.Socket thisSocket, string host, int port, System.AsyncCallback requestCallback, object state) { return default(System.IAsyncResult); }
        public static System.IAsyncResult BeginDisconnect(this System.Net.Sockets.Socket thisSocket, bool reuseSocket, System.AsyncCallback callback, object state) { return default(System.IAsyncResult); }
        public static System.IAsyncResult BeginReceive(this System.Net.Sockets.Socket thisSocket, byte[] buffer, int offset, int size, System.Net.Sockets.SocketFlags socketFlags, System.AsyncCallback callback, object state) { return default(System.IAsyncResult); }
        public static System.IAsyncResult BeginReceive(this System.Net.Sockets.Socket thisSocket, byte[] buffer, int offset, int size, System.Net.Sockets.SocketFlags socketFlags, out System.Net.Sockets.SocketError errorCode, System.AsyncCallback callback, object state) { errorCode = default(System.Net.Sockets.SocketError); return default(System.IAsyncResult); }
        public static System.IAsyncResult BeginReceive(this System.Net.Sockets.Socket thisSocket, System.Collections.Generic.IList<System.ArraySegment<byte>> buffers, System.Net.Sockets.SocketFlags socketFlags, System.AsyncCallback callback, object state) { return default(System.IAsyncResult); }
        public static System.IAsyncResult BeginReceive(this System.Net.Sockets.Socket thisSocket, System.Collections.Generic.IList<System.ArraySegment<byte>> buffers, System.Net.Sockets.SocketFlags socketFlags, out System.Net.Sockets.SocketError errorCode, System.AsyncCallback callback, object state) { errorCode = default(System.Net.Sockets.SocketError); return default(System.IAsyncResult); }
        public static System.IAsyncResult BeginReceiveFrom(this System.Net.Sockets.Socket thisSocket, byte[] buffer, int offset, int size, System.Net.Sockets.SocketFlags socketFlags, ref System.Net.EndPoint remoteEP, System.AsyncCallback callback, object state) { return default(System.IAsyncResult); }
        public static System.IAsyncResult BeginReceiveMessageFrom(this System.Net.Sockets.Socket thisSocket, byte[] buffer, int offset, int size, System.Net.Sockets.SocketFlags socketFlags, ref System.Net.EndPoint remoteEP, System.AsyncCallback callback, object state) { return default(System.IAsyncResult); }
        public static System.IAsyncResult BeginSend(this System.Net.Sockets.Socket thisSocket, byte[] buffer, int offset, int size, System.Net.Sockets.SocketFlags socketFlags, System.AsyncCallback callback, object state) { return default(System.IAsyncResult); }
        public static System.IAsyncResult BeginSend(this System.Net.Sockets.Socket thisSocket, byte[] buffer, int offset, int size, System.Net.Sockets.SocketFlags socketFlags, out System.Net.Sockets.SocketError errorCode, System.AsyncCallback callback, object state) { errorCode = default(System.Net.Sockets.SocketError); return default(System.IAsyncResult); }
        public static System.IAsyncResult BeginSend(this System.Net.Sockets.Socket thisSocket, System.Collections.Generic.IList<System.ArraySegment<byte>> buffers, System.Net.Sockets.SocketFlags socketFlags, System.AsyncCallback callback, object state) { return default(System.IAsyncResult); }
        public static System.IAsyncResult BeginSend(this System.Net.Sockets.Socket thisSocket, System.Collections.Generic.IList<System.ArraySegment<byte>> buffers, System.Net.Sockets.SocketFlags socketFlags, out System.Net.Sockets.SocketError errorCode, System.AsyncCallback callback, object state) { errorCode = default(System.Net.Sockets.SocketError); return default(System.IAsyncResult); }
        public static System.IAsyncResult BeginSendTo(this System.Net.Sockets.Socket thisSocket, byte[] buffer, int offset, int size, System.Net.Sockets.SocketFlags socketFlags, System.Net.EndPoint remoteEP, System.AsyncCallback callback, object state) { return default(System.IAsyncResult); }
        public static System.Net.Sockets.Socket EndAccept(this System.Net.Sockets.Socket thisSocket, out byte[] buffer, System.IAsyncResult asyncResult) { buffer = default(byte[]); return default(System.Net.Sockets.Socket); }
        public static System.Net.Sockets.Socket EndAccept(this System.Net.Sockets.Socket thisSocket, out byte[] buffer, out int bytesTransferred, System.IAsyncResult asyncResult) { buffer = default(byte[]); bytesTransferred = default(int); return default(System.Net.Sockets.Socket); }
        public static System.Net.Sockets.Socket EndAccept(this System.Net.Sockets.Socket thisSocket, System.IAsyncResult asyncResult) { return default(System.Net.Sockets.Socket); }
        public static void EndConnect(this System.Net.Sockets.Socket thisSocket, System.IAsyncResult asyncResult) { }
        public static void EndDisconnect(this System.Net.Sockets.Socket thisSocket, System.IAsyncResult asyncResult) { }
        public static int EndReceive(this System.Net.Sockets.Socket thisSocket, System.IAsyncResult asyncResult) { return default(int); }
        public static int EndReceive(this System.Net.Sockets.Socket thisSocket, System.IAsyncResult asyncResult, out System.Net.Sockets.SocketError errorCode) { errorCode = default(System.Net.Sockets.SocketError); return default(int); }
        public static int EndReceiveFrom(this System.Net.Sockets.Socket thisSocket, System.IAsyncResult asyncResult, ref System.Net.EndPoint endPoint) { return default(int); }
        public static int EndReceiveMessageFrom(this System.Net.Sockets.Socket thisSocket, System.IAsyncResult asyncResult, ref System.Net.Sockets.SocketFlags socketFlags, ref System.Net.EndPoint endPoint, out System.Net.Sockets.IPPacketInformation ipPacketInformation) { ipPacketInformation = default(System.Net.Sockets.IPPacketInformation); return default(int); }
        public static int EndSend(this System.Net.Sockets.Socket thisSocket, System.IAsyncResult asyncResult) { return default(int); }
        public static int EndSend(this System.Net.Sockets.Socket thisSocket, System.IAsyncResult asyncResult, out System.Net.Sockets.SocketError errorCode) { errorCode = default(System.Net.Sockets.SocketError); return default(int); }
        public static int EndSendTo(this System.Net.Sockets.Socket thisSocket, System.IAsyncResult asyncResult) { return default(int); }
        public static System.IAsyncResult BeginConnect(this System.Net.Sockets.TcpClient thisTcpClient, System.Net.IPAddress address, int port, System.AsyncCallback requestCallback, object state) { return default(System.IAsyncResult); }
        public static System.IAsyncResult BeginConnect(this System.Net.Sockets.TcpClient thisTcpClient, System.Net.IPAddress[] addresses, int port, System.AsyncCallback requestCallback, object state) { return default(System.IAsyncResult); }
        public static System.IAsyncResult BeginConnect(this System.Net.Sockets.TcpClient thisTcpClient, string host, int port, System.AsyncCallback requestCallback, object state) { return default(System.IAsyncResult); }
        public static void EndConnect(this System.Net.Sockets.TcpClient thisTcpClient, System.IAsyncResult asyncResult) { }
        public static System.IAsyncResult BeginAcceptSocket(this System.Net.Sockets.TcpListener thisTcpListener, System.AsyncCallback callback, object state) { return default(System.IAsyncResult); }
        public static System.IAsyncResult BeginAcceptTcpClient(this System.Net.Sockets.TcpListener thisTcpListener, System.AsyncCallback callback, object state) { return default(System.IAsyncResult); }
        public static System.Net.Sockets.Socket EndAcceptSocket(this System.Net.Sockets.TcpListener thisTcpListener, System.IAsyncResult asyncResult) { return default(System.Net.Sockets.Socket); }
        public static System.Net.Sockets.TcpClient EndAcceptTcpClient(this System.Net.Sockets.TcpListener thisTcpListener, System.IAsyncResult asyncResult) { return default(System.Net.Sockets.TcpClient); }
        public static System.IAsyncResult BeginReceive(this System.Net.Sockets.UdpClient thisUdpClient, System.AsyncCallback requestCallback, object state) { return default(System.IAsyncResult); }
        public static System.IAsyncResult BeginSend(this System.Net.Sockets.UdpClient thisUdpClient, byte[] datagram, int bytes, System.AsyncCallback requestCallback, object state) { return default(System.IAsyncResult); }
        public static System.IAsyncResult BeginSend(this System.Net.Sockets.UdpClient thisUdpClient, byte[] datagram, int bytes, System.Net.IPEndPoint endPoint, System.AsyncCallback requestCallback, object state) { return default(System.IAsyncResult); }
        public static System.IAsyncResult BeginSend(this System.Net.Sockets.UdpClient thisUdpClient, byte[] datagram, int bytes, string hostname, int port, System.AsyncCallback requestCallback, object state) { return default(System.IAsyncResult); }
        public static byte[] EndReceive(this System.Net.Sockets.UdpClient thisUdpClient, System.IAsyncResult asyncResult, ref System.Net.IPEndPoint remoteEP) { return default(byte[]); }
        public static int EndSend(this System.Net.Sockets.UdpClient thisUdpClient, System.IAsyncResult asyncResult) { return default(int); }
    }
}
