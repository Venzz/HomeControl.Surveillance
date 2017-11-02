using System;
using Windows.Foundation;

namespace HomeControl.Surveillance.Server.Data
{
    public interface ICameraConnection
    {
        event TypedEventHandler<ICameraConnection, Byte[]> DataReceived;
    }
}
