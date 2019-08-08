using HomeControl.Surveillance.Data.Camera.Heroku;
using HomeControl.Surveillance.Data.Camera.LocalNetwork;
using HomeControl.Surveillance.Data.Storage;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;

namespace HomeControl.Surveillance.Data.Camera.Adaptive
{
    public class AdaptiveConsumerCameraService: IConsumerCameraService
    {
        private IConsumerCameraService OfflineCameraService;
        private Lazy<IConsumerCameraService> OnlineCameraService;

        public event TypedEventHandler<IConsumerCameraService, (MediaDataType MediaType, Byte[] Data, DateTime Timestamp, TimeSpan Duration)> MediaDataReceived = delegate { };
        public event TypedEventHandler<IConsumerCameraService, (String Message, String Parameter)> LogReceived = delegate { };
        public event TypedEventHandler<IConsumerCameraService, (String Message, Exception Exception)> ExceptionReceived = delegate { };



        public AdaptiveConsumerCameraService(String endpoint, Int16 offlineServicePort)
        {
            OfflineCameraService = new LocalNetworkConsumerCameraService(endpoint, offlineServicePort);
            OnlineCameraService = new Lazy<IConsumerCameraService>(CreateOnlineCameraService, isThreadSafe: true);

            OfflineCameraService.MediaDataReceived += (sender, args) => MediaDataReceived(this, args);
            OfflineCameraService.LogReceived += (sender, args) => LogReceived(this, args);
            OfflineCameraService.ExceptionReceived += (sender, args) => ExceptionReceived(this, args);
        }

        private IConsumerCameraService CreateOnlineCameraService()
        {
            var onlineCameraService = new HerokuConsumerCameraService("client");
            onlineCameraService.MediaDataReceived += (sender, args) => MediaDataReceived(this, args);
            onlineCameraService.LogReceived += (sender, args) => LogReceived(this, args);
            onlineCameraService.ExceptionReceived += (sender, args) => ExceptionReceived(this, args);
            return onlineCameraService;
        }

        public async Task EnsureConnectedAsync()
        {
            var isOfflineAvailable = await OfflineCameraService.IsAvailableAsync().ConfigureAwait(false);
            await (isOfflineAvailable ? OfflineCameraService : OnlineCameraService.Value).EnsureConnectedAsync().ConfigureAwait(false);
        }

        public Task<Boolean> IsAvailableAsync()
        {
            return Task.FromResult(true);
        }

        public async Task PerformAsync(Command command)
        {
            var isOfflineAvailable = await OfflineCameraService.IsAvailableAsync().ConfigureAwait(false);
            await (isOfflineAvailable ? OfflineCameraService : OnlineCameraService.Value).PerformAsync(command).ConfigureAwait(false);
        }

        public async Task<IReadOnlyCollection<(String Id, DateTime Date)>> GetStoredRecordsMetadataAsync()
        {
            var isOfflineAvailable = await OfflineCameraService.IsAvailableAsync().ConfigureAwait(false);
            return await (isOfflineAvailable ? OfflineCameraService : OnlineCameraService.Value).GetStoredRecordsMetadataAsync().ConfigureAwait(false);
        }

        public async Task<IReadOnlyCollection<StoredRecordFile.MediaDataDescriptor>> GetMediaDataDescriptorsAsync(String id)
        {
            var isOfflineAvailable = await OfflineCameraService.IsAvailableAsync().ConfigureAwait(false);
            return await (isOfflineAvailable ? OfflineCameraService : OnlineCameraService.Value).GetMediaDataDescriptorsAsync(id).ConfigureAwait(false);
        }

        public async Task<Byte[]> GetMediaDataAsync(String id, UInt32 offset)
        {
            var isOfflineAvailable = await OfflineCameraService.IsAvailableAsync().ConfigureAwait(false);
            return await (isOfflineAvailable ? OfflineCameraService : OnlineCameraService.Value).GetMediaDataAsync(id, offset).ConfigureAwait(false);
        }

        public async Task<IReadOnlyCollection<String>> GetFileListAsync()
        {
            var isOfflineAvailable = await OfflineCameraService.IsAvailableAsync().ConfigureAwait(false);
            return await (isOfflineAvailable ? OfflineCameraService : OnlineCameraService.Value).GetFileListAsync().ConfigureAwait(false);
        }

        public async Task<Byte[]> GetFileDataAsync(String id, UInt32 offset, UInt32 length, CancellationToken cancellationToken)
        {
            var isOfflineAvailable = await OfflineCameraService.IsAvailableAsync().ConfigureAwait(false);
            return await (isOfflineAvailable ? OfflineCameraService : OnlineCameraService.Value).GetFileDataAsync(id, offset, length, cancellationToken).ConfigureAwait(false);
        }

        public async Task SetPushChannelUriAsync(String previousChannelUri, String channelUri)
        {
            var isOfflineAvailable = await OfflineCameraService.IsAvailableAsync().ConfigureAwait(false);
            await (isOfflineAvailable ? OfflineCameraService : OnlineCameraService.Value).SetPushChannelUriAsync(previousChannelUri, channelUri).ConfigureAwait(false);
        }
    }
}