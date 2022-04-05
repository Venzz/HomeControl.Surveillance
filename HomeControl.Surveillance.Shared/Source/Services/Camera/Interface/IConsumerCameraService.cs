using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;

namespace HomeControl.Surveillance.Services
{
    public interface IConsumerCameraService
    {
        event TypedEventHandler<IConsumerCameraService, (MediaDataType MediaType, Byte[] Data, DateTime Timestamp, TimeSpan Duration)> MediaDataReceived;
        event TypedEventHandler<IConsumerCameraService, IReadOnlyCollection<(MediaDataType MediaType, Byte[] Data, DateTime Timestamp, TimeSpan Duration)>> MediaDataBufferReceived;
        event TypedEventHandler<IConsumerCameraService, (String Message, String Parameter)> LogReceived;
        event TypedEventHandler<IConsumerCameraService, (String Message, Exception Exception)> ExceptionReceived;

        Task PerformAsync(Command command);
        Task<IReadOnlyCollection<(String Id, DateTime Date)>> GetStoredRecordsMetadataAsync();
        Task<IReadOnlyCollection<StoredRecordFile.MediaDataDescriptor>> GetMediaDataDescriptorsAsync(String id);
        Task<Byte[]> GetMediaDataAsync(String id, UInt32 offset);

        Task SetPushChannelUriAsync(String previousChannelUri, String channelUri);
    }
}
