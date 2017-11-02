using System;
using Windows.Foundation;

namespace HomeControl.Surveillance.Data.Camera
{
    public interface IConsumerCameraService
    {
        event TypedEventHandler<IConsumerCameraService, Byte[]> DataReceived;
        event TypedEventHandler<IConsumerCameraService, (String Message, String Parameter)> LogReceived;
        event TypedEventHandler<IConsumerCameraService, (String Message, Exception Exception)> ExceptionReceived;
    }
}
