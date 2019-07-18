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

        private TcpListener Listener;
        private Dictionary<UInt32, Socket> AcceptedSockets = new Dictionary<UInt32, Socket>();

        public event TypedEventHandler<Object, UInt32> ClientConnected = delegate { };
        public event TypedEventHandler<Object, (UInt32 SocketId, Byte[])> DataReceived = delegate { };
        public event TypedEventHandler<Object, (String Message, String Parameter)> LogReceived = delegate { };
        public event TypedEventHandler<Object, UInt32> ClientDisconnected = delegate { };



        public TcpServer(Int16 port)
        {
            Listener = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
        }

        public void Start()
        {
            Listener.Start(10);
            Listener.BeginAcceptSocket(OnSocketAccepted, null);
        }

        public void Send(UInt32 socketId, Byte[] data)
        {
            if (socketId == 0)
            {
                foreach (var acceptedSocket in AcceptedSockets.Values)
                    acceptedSocket.Send(data);

            }
            else if (AcceptedSockets.ContainsKey(socketId))
            {
                var socket = AcceptedSockets[socketId];
                socket.Send(data);
            }
        }

        private void OnSocketAccepted(IAsyncResult asyncResult)
        {
            var socketId = GetSocketId();
            var acceptedSocket = Listener.EndAcceptSocket(asyncResult);
            AcceptedSockets.Add(socketId, acceptedSocket);
            ClientConnected(this, socketId);
            LogReceived(this, ($"{nameof(TcpServer)}.{nameof(OnSocketAccepted)}", acceptedSocket.LocalEndPoint.ToString()));

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
                AcceptedSockets.Remove(state.SocketId);
                LogReceived(this, ($"{nameof(TcpServer)}.{nameof(OnSocketDataReceived)}", exception.Message));
            }
        }

        private UInt32 GetSocketId()
        {
            var baseDateTime = new DateTime(2019, 1, 1);
            return (UInt32)((DateTime.Now - baseDateTime).Ticks >> 22);
        }
    }
}
