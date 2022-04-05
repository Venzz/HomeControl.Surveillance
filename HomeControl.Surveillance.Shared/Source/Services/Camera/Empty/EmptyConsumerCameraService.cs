using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;

namespace HomeControl.Surveillance.Services
{
    public class EmptyConsumerCameraService: IConsumerCameraService
    {
        public event TypedEventHandler<IConsumerCameraService, (MediaDataType MediaType, Byte[] Data, DateTime Timestamp, TimeSpan Duration)> MediaDataReceived = delegate { };
        public event TypedEventHandler<IConsumerCameraService, IReadOnlyCollection<(MediaDataType MediaType, Byte[] Data, DateTime Timestamp, TimeSpan Duration)>> MediaDataBufferReceived = delegate { };
        public event TypedEventHandler<IConsumerCameraService, (String Message, String Parameter)> LogReceived = delegate { };
        public event TypedEventHandler<IConsumerCameraService, (String Message, Exception Exception)> ExceptionReceived = delegate { };

        public EmptyConsumerCameraService() { }

        public Task PerformAsync(Command command) => Task.FromResult<Object>(null);

        public Task<IReadOnlyCollection<(String Id, DateTime Date)>> GetStoredRecordsMetadataAsync()
        {
            return Task.FromResult<IReadOnlyCollection<(String Id, DateTime Date)>>(new List<(String Id, DateTime Date)>());
        }

        public Task<IReadOnlyCollection<StoredRecordFile.MediaDataDescriptor>> GetMediaDataDescriptorsAsync(String id)
        {
            return Task.FromResult<IReadOnlyCollection<StoredRecordFile.MediaDataDescriptor>>(new List<StoredRecordFile.MediaDataDescriptor>());
        }

        public Task<Byte[]> GetMediaDataAsync(String id, UInt32 offset)
        {
            return Task.FromResult(new Byte[0]);
        }

        public Task SetPushChannelUriAsync(String previousChannelUri, String channelUri)
        {
            return Task.CompletedTask;
        }
    }
}