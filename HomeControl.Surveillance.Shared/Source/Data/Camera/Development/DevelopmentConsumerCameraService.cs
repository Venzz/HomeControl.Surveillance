using HomeControl.Surveillance.Data.Storage;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;

namespace HomeControl.Surveillance.Data.Camera.Development
{
    public class DevelopmentConsumerCameraService: IConsumerCameraService
    {
        public event TypedEventHandler<IConsumerCameraService, (MediaDataType MediaType, Byte[] Data, DateTime Timestamp, TimeSpan Duration)> MediaDataReceived = delegate { };
        public event TypedEventHandler<IConsumerCameraService, (String Message, String Parameter)> LogReceived = delegate { };
        public event TypedEventHandler<IConsumerCameraService, (String Message, Exception Exception)> ExceptionReceived = delegate { };

        public DevelopmentConsumerCameraService() { }

        public Task EnsureConnectedAsync() => Task.FromResult<Object>(null);

        public Task<Boolean> IsAvailableAsync() => Task.FromResult(true);

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

        public Task<IReadOnlyCollection<String>> GetFileListAsync() => Task.FromResult<IReadOnlyCollection<String>>(new List<String>()
        {
            "2018-12-25_03.sr",
            "2018-12-25_04.sr",
            "2018-12-28.log",
            "2018-12-28_19.sr",
            "2018-12-28_20.sr",
            "2019-01-30.log",
            "2019-01-30_06.sr",
            "2019-01-30_18.sr",
            "2019-01-30_19.sr",
            "2019-01-30_20.sr",
            "2019-01-30_21.sr",
            "2019-01-31.log",
            "2019-01-31_06.sr",
            "2019-01-31_07.sr",
            "2019-01-31_08.sr",
            "2019-02-11.log",
            "2019-06-20_07.sr",
            "2019-06-22_09.sr",
            "2019-06-27_06.sr",
            "2019-06-27_07.sr"
        });

        public async Task<Byte[]> GetFileDataAsync(String id, UInt32 offset, UInt32 length, CancellationToken cancellationToken)
        {
            await Task.Delay(1000).ConfigureAwait(false);
            return (offset >= 50 * 1000 * 1000) ? new Byte[0] : new Byte[length];
        }
    }
}