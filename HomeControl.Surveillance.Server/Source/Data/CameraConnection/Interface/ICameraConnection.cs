using System;
using Windows.Foundation;

namespace HomeControl.Surveillance.Server.Data
{
    public interface ICameraConnection
    {
        event TypedEventHandler<ICameraConnection, Byte[]> DataReceived;
        event TypedEventHandler<ICameraConnection, (String CustomText, Exception Exception)> ExceptionReceived;
        event TypedEventHandler<ICameraConnection, (String CustomText, String Parameter)> LogReceived;
    }
}
