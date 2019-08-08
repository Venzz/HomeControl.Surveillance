using System;
using System.Net.Sockets;
using Windows.Foundation;

namespace HomeControl.Surveillance.Data.Camera.LocalNetwork
{
    public interface ITcpServer
    {
        event TypedEventHandler<ITcpServer, (Socket Socket, UInt32 SocketId)> ClientConnected;
        event TypedEventHandler<ITcpServer, (UInt32 SocketId, Byte[])> DataReceived;
        event TypedEventHandler<ITcpServer, (String Message, String Parameter)> LogReceived;
        event TypedEventHandler<ITcpServer, UInt32> ClientDisconnected;

        void Start();
        void Send(UInt32 socketId, Byte[] data);
    }
}
