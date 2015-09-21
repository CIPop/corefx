// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.Net.Sockets
{
    public static class SocketLegacyExtensions
    {
        #region Socket
        public static IAsyncResult BeginAccept(this Socket thisSocket, AsyncCallback callback, object state)
        {
            return thisSocket.InternalBeginAccept(callback, state);
        }
        public static IAsyncResult BeginAccept(this Socket thisSocket, int receiveSize, AsyncCallback callback, object state)
        {
            return thisSocket.InternalBeginAccept(receiveSize, callback, state);
        }
        public static IAsyncResult BeginAccept(this Socket thisSocket, Socket acceptSocket, int receiveSize, AsyncCallback callback, object state)
        {
            return thisSocket.InternalBeginAccept(acceptSocket, receiveSize, callback, state);
        }
        public static IAsyncResult BeginConnect(this Socket thisSocket, EndPoint remoteEP, AsyncCallback callback, object state)
        {
            return thisSocket.InternalBeginConnect(remoteEP, callback, state);
        }
        public static IAsyncResult BeginConnect(this Socket thisSocket, IPAddress address, int port, AsyncCallback requestCallback, object state)
        {
            return thisSocket.InternalBeginConnect(address, port, requestCallback, state);
        }
        public static IAsyncResult BeginConnect(this Socket thisSocket, IPAddress[] addresses, int port, AsyncCallback requestCallback, object state)
        {
            return thisSocket.InternalBeginConnect(addresses, port, requestCallback, state);
        }
        public static IAsyncResult BeginConnect(this Socket thisSocket, string host, int port, AsyncCallback requestCallback, object state)
        {
            return thisSocket.InternalBeginConnect(host, port, requestCallback, state);
        }
        public static IAsyncResult BeginDisconnect(this Socket thisSocket, bool reuseSocket, AsyncCallback callback, object state)
        {
            return thisSocket.InternalBeginDisconnect(reuseSocket, callback, state);
        }
        public static IAsyncResult BeginReceive(this Socket thisSocket, byte[] buffer, int offset, int size, SocketFlags socketFlags, AsyncCallback callback, object state)
        {
            return thisSocket.InternalBeginReceive(buffer, offset, size, socketFlags, callback, state);
        }
        public static IAsyncResult BeginReceive(this Socket thisSocket, byte[] buffer, int offset, int size, SocketFlags socketFlags, out SocketError errorCode, AsyncCallback callback, object state)
        {
            return thisSocket.InternalBeginReceive(buffer, offset, size, socketFlags, out errorCode, callback, state);
        }
        public static IAsyncResult BeginReceive(this Socket thisSocket, Collections.Generic.IList<ArraySegment<byte>> buffers, SocketFlags socketFlags, AsyncCallback callback, object state)
        {
            return thisSocket.InternalBeginReceive(buffers, socketFlags, callback, state);
        }
        public static IAsyncResult BeginReceive(this Socket thisSocket, Collections.Generic.IList<ArraySegment<byte>> buffers, SocketFlags socketFlags, out SocketError errorCode, AsyncCallback callback, object state)
        {
            return thisSocket.InternalBeginReceive(buffers, socketFlags, out errorCode, callback, state);
        }
        public static IAsyncResult BeginReceiveFrom(this Socket thisSocket, byte[] buffer, int offset, int size, SocketFlags socketFlags, ref EndPoint remoteEP, AsyncCallback callback, object state)
        {
            return thisSocket.InternalBeginReceiveFrom(buffer, offset, size, socketFlags, ref remoteEP, callback, state);
        }
        public static IAsyncResult BeginReceiveMessageFrom(this Socket thisSocket, byte[] buffer, int offset, int size, SocketFlags socketFlags, ref EndPoint remoteEP, AsyncCallback callback, object state)
        {
            return thisSocket.InternalBeginReceiveMessageFrom(buffer, offset, size, socketFlags, ref remoteEP, callback, state);
        }
        public static IAsyncResult BeginSend(this Socket thisSocket, byte[] buffer, int offset, int size, SocketFlags socketFlags, AsyncCallback callback, object state)
        {
            return thisSocket.InternalBeginSend(buffer, offset, size, socketFlags, callback, state);
        }
        public static IAsyncResult BeginSend(this Socket thisSocket, byte[] buffer, int offset, int size, SocketFlags socketFlags, out SocketError errorCode, AsyncCallback callback, object state)
        {
            return thisSocket.InternalBeginSend(buffer, offset, size, socketFlags, out errorCode, callback, state);
        }
        public static IAsyncResult BeginSend(this Socket thisSocket, Collections.Generic.IList<ArraySegment<byte>> buffers, SocketFlags socketFlags, AsyncCallback callback, object state)
        {
            return thisSocket.InternalBeginSend(buffers, socketFlags, callback, state);
        }
        public static IAsyncResult BeginSend(this Socket thisSocket, Collections.Generic.IList<ArraySegment<byte>> buffers, SocketFlags socketFlags, out SocketError errorCode, AsyncCallback callback, object state)
        {
            return thisSocket.InternalBeginSend(buffers, socketFlags, out errorCode, callback, state);
        }
        public static IAsyncResult BeginSendTo(this Socket thisSocket, byte[] buffer, int offset, int size, SocketFlags socketFlags, EndPoint remoteEP, AsyncCallback callback, object state)
        {
            return thisSocket.InternalBeginSendTo(buffer,offset, size, socketFlags, remoteEP, callback, state);
        }
        public static Socket EndAccept(this Socket thisSocket, out byte[] buffer, IAsyncResult asyncResult)
        {
            return thisSocket.InternalEndAccept(out buffer, asyncResult);
        }
        public static Socket EndAccept(this Socket thisSocket, out byte[] buffer, out int bytesTransferred, IAsyncResult asyncResult)
        {
            return thisSocket.InternalEndAccept(out buffer, out bytesTransferred, asyncResult);
        }
        public static Socket EndAccept(this Socket thisSocket, IAsyncResult asyncResult)
        {
            return thisSocket.InternalEndAccept(asyncResult);
        }
        public static void EndConnect(this Socket thisSocket, IAsyncResult asyncResult)
        {
            thisSocket.InternalEndConnect(asyncResult);
        }
        public static void EndDisconnect(this Socket thisSocket, IAsyncResult asyncResult)
        {
            thisSocket.InternalEndDisconnect(asyncResult);
        }
        public static int EndReceive(this Socket thisSocket, IAsyncResult asyncResult)
        {
            return thisSocket.InternalEndReceive(asyncResult);
        }
        public static int EndReceive(this Socket thisSocket, IAsyncResult asyncResult, out SocketError errorCode)
        {
            return thisSocket.InternalEndReceive(asyncResult, out errorCode);
        }
        public static int EndReceiveFrom(this Socket thisSocket, IAsyncResult asyncResult, ref EndPoint endPoint)
        {
            return thisSocket.InternalEndReceiveFrom(asyncResult, ref endPoint);
        }
        public static int EndReceiveMessageFrom(this Socket thisSocket, IAsyncResult asyncResult, ref SocketFlags socketFlags, ref EndPoint endPoint, out IPPacketInformation ipPacketInformation)
        {
            return thisSocket.InternalEndReceiveMessageFrom(asyncResult, ref socketFlags, ref endPoint, out ipPacketInformation);
        }
        public static int EndSend(this Socket thisSocket, IAsyncResult asyncResult)
        {
            return thisSocket.InternalEndSend(asyncResult);
        }
        public static int EndSend(this Socket thisSocket,IAsyncResult asyncResult, out SocketError errorCode)
        {
            return thisSocket.InternalEndSend(asyncResult, out errorCode);
        }
        public static int EndSendTo(this Socket thisSocket, IAsyncResult asyncResult)
        {
            return thisSocket.InternalEndSendTo(asyncResult);
        }
        #endregion
        #region TcpClient
        public static IAsyncResult BeginConnect(this TcpClient thisTcpClient, IPAddress address, int port, AsyncCallback requestCallback, object state)
        {
            return thisTcpClient.InternalBeginConnect(address, port, requestCallback, state);
        }
        public static IAsyncResult BeginConnect(this TcpClient thisTcpClient, IPAddress[] addresses, int port, AsyncCallback requestCallback, object state)
        {
            return thisTcpClient.InternalBeginConnect(addresses, port, requestCallback, state);
        }
        public static IAsyncResult BeginConnect(this TcpClient thisTcpClient, string host, int port, AsyncCallback requestCallback, object state)
        {
            return thisTcpClient.InternalBeginConnect(host, port, requestCallback, state);
        }
        public static void EndConnect(this TcpClient thisTcpClient, IAsyncResult asyncResult)
        {
            thisTcpClient.InternalEndConnect(asyncResult);
        }
        #endregion
        #region TcpListener
        public static IAsyncResult BeginAcceptSocket(this TcpListener thisTcpListener, AsyncCallback callback, object state)
        {
            return thisTcpListener.InternalBeginAcceptSocket(callback, state);
        }
        public static IAsyncResult BeginAcceptTcpClient(this TcpListener thisTcpListener, AsyncCallback callback, object state)
        {
            return thisTcpListener.InternalBeginAcceptTcpClient(callback, state);
        }
        public static Socket EndAcceptSocket(this TcpListener thisTcpListener, IAsyncResult asyncResult)
        {
            return thisTcpListener.InternalEndAcceptSocket(asyncResult);
        }
        public static TcpClient EndAcceptTcpClient(this TcpListener thisTcpListener, IAsyncResult asyncResult)
        {
            return thisTcpListener.InternalEndAcceptTcpClient(asyncResult);
        }
        #endregion
        #region UdpClient
        public static IAsyncResult BeginReceive(this UdpClient thisUdpClient, AsyncCallback requestCallback, object state)
        {
            return thisUdpClient.InternalBeginReceive(requestCallback, state);
        }
        public static IAsyncResult BeginSend(this UdpClient thisUdpClient, byte[] datagram, int bytes, AsyncCallback requestCallback, object state)
        {
            return thisUdpClient.InternalBeginSend(datagram, bytes, requestCallback, state);
        }
        public static IAsyncResult BeginSend(this UdpClient thisUdpClient, byte[] datagram, int bytes, IPEndPoint endPoint, AsyncCallback requestCallback, object state)
        {
            return thisUdpClient.InternalBeginSend(datagram, bytes, endPoint, requestCallback, state);
        }
        public static IAsyncResult BeginSend(this UdpClient thisUdpClient, byte[] datagram, int bytes, string hostname, int port, AsyncCallback requestCallback, object state)
        {
            return thisUdpClient.InternalBeginSend(datagram, bytes, hostname, port, requestCallback, state);
        }
        public static byte[] EndReceive(this UdpClient thisUdpClient, IAsyncResult asyncResult, ref IPEndPoint remoteEP)
        {
            return thisUdpClient.InternalEndReceive(asyncResult, ref remoteEP);
        }
        public static int EndSend(this UdpClient thisUdpClient, IAsyncResult asyncResult)
        {
            return thisUdpClient.InternalEndSend(asyncResult);
        }
        #endregion
    }
}

