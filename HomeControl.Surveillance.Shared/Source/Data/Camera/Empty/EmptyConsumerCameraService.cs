using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;

namespace HomeControl.Surveillance.Data.Camera.Empty
{
    public class EmptyConsumerCameraService: IConsumerCameraService
    {
        public event TypedEventHandler<IConsumerCameraService, (MediaDataType MediaType, Byte[] Data, DateTime Timestamp, TimeSpan Duration)> MediaDataReceived = delegate { };
        public event TypedEventHandler<IConsumerCameraService, (String Message, String Parameter)> LogReceived = delegate { };
        public event TypedEventHandler<IConsumerCameraService, (String Message, Exception Exception)> ExceptionReceived = delegate { };

        public EmptyConsumerCameraService() { }

        public Task PerformAsync(Command command) => Task.FromResult<Object>(null);

        public Task<IReadOnlyCollection<(String Id, DateTime Date)>> GetStoredRecordsMetadataAsync() => Task.FromResult<IReadOnlyCollection<(String Id, DateTime Date)>>(new List<(String Id, DateTime Date)>());        
    }
}