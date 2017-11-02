using System;
using Windows.Foundation;

namespace HomeControl.Surveillance.Data.Camera.Empty
{
    public class EmptyConsumerCameraService: IConsumerCameraService
    {
        public event TypedEventHandler<IConsumerCameraService, Byte[]> DataReceived = delegate { };
        public event TypedEventHandler<IConsumerCameraService, (String Message, String Parameter)> LogReceived = delegate { };
        public event TypedEventHandler<IConsumerCameraService, (String Message, Exception Exception)> ExceptionReceived = delegate { };

        public EmptyConsumerCameraService()
        {
        }
    }
}