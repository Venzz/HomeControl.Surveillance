using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;

namespace HomeControl.Surveillance.Data.Camera.Empty
{
    public class EmptyConsumerCameraService: IConsumerCameraService
    {
        public event TypedEventHandler<IConsumerCameraService, (MediaDataType MediaType, Byte[] Data)> MediaDataReceived = delegate { };
        public event TypedEventHandler<IConsumerCameraService, (String Message, String Parameter)> LogReceived = delegate { };
        public event TypedEventHandler<IConsumerCameraService, (String Message, Exception Exception)> ExceptionReceived = delegate { };

        public EmptyConsumerCameraService() { }

        public Task PerformAsync(Command command) => Task.FromResult<Object>(null);

        public Task<IReadOnlyCollection<DateTime>> GetStoredRecordsMetadataAsync() => Task.FromResult<IReadOnlyCollection<DateTime>>(new List<DateTime>());        
    }
}