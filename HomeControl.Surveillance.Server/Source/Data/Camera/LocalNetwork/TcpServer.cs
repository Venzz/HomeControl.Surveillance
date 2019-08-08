using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Windows.Foundation;

namespace HomeControl.Surveillance.Data.Camera.LocalNetwork
{
    public class TcpServer: ITcpServer
    {
        private const Int32 BufferSize = 1 * 1024 * 1024;

        private Object Sync;
        private UInt32 LastUsedSocketId;
        private TcpListener Listener;
        private Dictionary<UInt32, Socket> AcceptedSockets = new Dictionary<UInt32, Socket>();

        public event TypedEventHandler<ITcpServer, (Socket Socket, UInt32 SocketId)> ClientConnected = delegate { };
        public event TypedEventHandler<ITcpServer, (UInt32 SocketId, Byte[])> DataReceived = delegate { };
        public event TypedEventHandler<ITcpServer, (String Message, String Parameter)> LogReceived = delegate { };
        public event TypedEventHandler<ITcpServer, UInt32> ClientDisconnected = delegate { };



        public TcpServer(String ipAddress, Int16 port)
        {
            Sync = new Object();
            Listener = new TcpListener(IPAddress.Parse(ipAddress), port);
        }

        public void Start()
        {
            Listener.Start(10);
            Listener.BeginAcceptSocket(OnSocketAccepted, null);
        }

        public void Send(UInt32 socketId, Byte[] data)
        {
            lock (Sync)
            {
                if (AcceptedSockets.ContainsKey(socketId))
                {
                    var socket = AcceptedSockets[socketId];
                    socket.BeginSend(data, 0, data.Length, SocketFlags.None, OnSocketDataSent, (socket, socketId));
                }
            }
        }

        private void OnSocketAccepted(IAsyncResult asyncResult)
        {
            var socketId = GetSocketId();
            var acceptedSocket = Listener.EndAcceptSocket(asyncResult);
            lock (Sync)
                AcceptedSockets.Add(socketId, acceptedSocket);
            ClientConnected(this, (acceptedSocket, socketId));

            var data = new Byte[BufferSize];
            acceptedSocket.BeginReceive(data, 0, data.Length, SocketFlags.None, OnSocketDataReceived, (data, acceptedSocket, socketId));
            Listener.BeginAcceptSocket(OnSocketAccepted, null);
        }

        private void OnSocketDataReceived(IAsyncResult asyncResult)
        {
            (Byte[] Data, Socket Socket, UInt32 SocketId) state = ((ValueTuple<Byte[], Socket, UInt32>)asyncResult.AsyncState);
            try
            {
                var readBytes = state.Socket.EndReceive(asyncResult);
                if (readBytes > 0)
                {
                    var readData = new Byte[readBytes];
                    Array.Copy(state.Data, readData, readBytes);
                    DataReceived(this, (state.SocketId, readData));
                }
                state.Socket.BeginReceive(state.Data, 0, state.Data.Length, SocketFlags.None, OnSocketDataReceived, state);
            }
            catch (SocketException exception)
            {
                lock (Sync)
                    AcceptedSockets.Remove(state.SocketId);
                ClientDisconnected(this, state.SocketId);
                LogReceived(this, ($"{nameof(TcpServer)}.{nameof(OnSocketDataReceived)}", exception.Message));
            }
        }

        private void OnSocketDataSent(IAsyncResult asyncResult)
        {
            (Socket Socket, UInt32 SocketId) state = ((ValueTuple<Socket, UInt32>)asyncResult.AsyncState);
            try
            {
                state.Socket.EndSend(asyncResult);
            }
            catch (SocketException exception)
            {
                lock (Sync)
                    AcceptedSockets.Remove(state.SocketId);
                ClientDisconnected(this, state.SocketId);
                LogReceived(this, ($"{nameof(TcpServer)}.{nameof(OnSocketDataSent)}", exception.Message));
            }
        }

        private UInt32 GetSocketId()
        {
            var baseDateTime = new DateTime(2019, 1, 1);
            var socketId = (UInt32)((DateTime.Now - baseDateTime).Ticks >> 22);
            if (LastUsedSocketId == socketId)
                socketId++;
            LastUsedSocketId = socketId;
            return LastUsedSocketId;
        }
    }
}
