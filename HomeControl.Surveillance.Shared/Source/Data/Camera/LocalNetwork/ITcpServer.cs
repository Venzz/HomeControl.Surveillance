using System;
using Windows.Foundation;

namespace HomeControl.Surveillance.Data.Camera.LocalNetwork
{
    public interface ITcpServer
    {
        event TypedEventHandler<Object, UInt32> ClientConnected;
        event TypedEventHandler<Object, (UInt32 SocketId, Byte[])> DataReceived;
        event TypedEventHandler<Object, (String Message, String Parameter)> LogReceived;
        event TypedEventHandler<Object, UInt32> ClientDisconnected;

        void Start();
        void Send(UInt32 socketId, Byte[] data);
    }
}
