using HomeControl.Surveillance.Data.Storage;
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
        Task<IReadOnlyCollection<(String Id, DateTime Date)>> GetStoredRecordsMetadataAsync();
        Task<IReadOnlyCollection<StoredRecordFile.MediaDataDescriptor>> GetMediaDataDescriptorsAsync(String id);
        Task<Byte[]> GetMediaDataAsync(String id, UInt32 offset);

        Task SetPushChannelUriAsync(String previousChannelUri, String channelUri);
    }
}
