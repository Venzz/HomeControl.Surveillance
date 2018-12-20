using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;

namespace HomeControl.Surveillance.Data.Camera
{
    public interface IConsumerCameraService
    {
        event TypedEventHandler<IConsumerCameraService, (MediaDataType MediaType, Byte[] Data, DateTime Timestamp, TimeSpan Duration)> MediaDataReceived;
        event TypedEventHandler<IConsumerCameraService, (String Message, String Parameter)> LogReceived;
        event TypedEventHandler<IConsumerCameraService, (String Message, Exception Exception)> ExceptionReceived;

        Task PerformAsync(Command command);
        Task<IReadOnlyCollection<DateTime>> GetStoredRecordsMetadataAsync();
    }
}
