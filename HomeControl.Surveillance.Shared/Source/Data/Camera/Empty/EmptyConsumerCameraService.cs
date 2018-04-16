using System;
using System.Threading.Tasks;
using Windows.Foundation;

namespace HomeControl.Surveillance.Data.Camera.Empty
{
    public class EmptyConsumerCameraService: IConsumerCameraService
    {
        public event TypedEventHandler<IConsumerCameraService, Byte[]> DataReceived = delegate { };
        public event TypedEventHandler<IConsumerCameraService, (String Message, String Parameter)> LogReceived = delegate { };
        public event TypedEventHandler<IConsumerCameraService, (String Message, Exception Exception)> ExceptionReceived = delegate { };

        public EmptyConsumerCameraService() { }

        public Task PerformAsync(Command command) => Task.FromResult<Object>(null);
    }
}