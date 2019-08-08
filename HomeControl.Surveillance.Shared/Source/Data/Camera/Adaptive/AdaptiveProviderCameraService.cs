using HomeControl.Surveillance.Data.Camera.Heroku;
using HomeControl.Surveillance.Data.Camera.LocalNetwork;
using HomeControl.Surveillance.Data.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;

namespace HomeControl.Surveillance.Data.Camera.Adaptive
{
    public class AdaptiveProviderCameraService: IProviderCameraService
    {
        private IProviderCameraService OfflineCameraService;
        private IProviderCameraService OnlineCameraService;

        public event TypedEventHandler<IProviderCameraService, Command> CommandReceived = delegate { };
        public event TypedEventHandler<IProviderCameraService, (UInt32 ConsumerId, UInt32 Id, IMessage Message)> MessageReceived = delegate { };
        public event TypedEventHandler<IProviderCameraService, (String Message, String Parameter)> LogReceived = delegate { };
        public event TypedEventHandler<IProviderCameraService, (String Message, Exception Exception)> ExceptionReceived = delegate { };



        public AdaptiveProviderCameraService(ITcpServer tcpServer, (TimeSpan From, TimeSpan Duration)? onlineServiceIdlePeriod)
        {
            OfflineCameraService = new LocalNetworkProviderCameraService(tcpServer);
            OnlineCameraService = new HerokuProviderCameraService("service", onlineServiceIdlePeriod);

            OfflineCameraService.CommandReceived += (sender, args) => CommandReceived(this, args);
            OfflineCameraService.MessageReceived += (sender, args) => MessageReceived(this, args);
            OfflineCameraService.LogReceived += (sender, args) => LogReceived(this, args);
            OfflineCameraService.ExceptionReceived += (sender, args) => ExceptionReceived(this, args);
            OnlineCameraService.CommandReceived += (sender, args) => CommandReceived(this, args);
            OnlineCameraService.MessageReceived += (sender, args) => MessageReceived(this, args);
            OnlineCameraService.LogReceived += (sender, args) => LogReceived(this, args);
            OnlineCameraService.ExceptionReceived += (sender, args) => ExceptionReceived(this, args);
        }

        public void EnsureConnected()
        {
            OfflineCameraService.EnsureConnected();
            OnlineCameraService.EnsureConnected();
        }

        public Task SendFileDataAsync(UInt32 consumerId, UInt32 id, Byte[] data)
        {
            var offlineTask = OfflineCameraService.SendFileDataAsync(consumerId, id, data);
            var onlineTask = OnlineCameraService.SendFileDataAsync(consumerId, id, data);
            return Task.WhenAll(offlineTask, onlineTask);
        }

        public Task SendFileListAsync(UInt32 consumerId, UInt32 id, IReadOnlyCollection<String> fileList)
        {
            var offlineTask = OfflineCameraService.SendFileListAsync(consumerId, id, fileList);
            var onlineTask = OnlineCameraService.SendFileListAsync(consumerId, id, fileList);
            return Task.WhenAll(offlineTask, onlineTask);
        }

        public Task SendLiveMediaDataAsync(MediaDataType type, Byte[] data, DateTime timestamp, TimeSpan duration)
        {
            var offlineTask = OfflineCameraService.SendLiveMediaDataAsync(type, data, timestamp, duration);
            var onlineTask = OnlineCameraService.SendLiveMediaDataAsync(type, data, timestamp, duration);
            return Task.WhenAll(offlineTask, onlineTask);
        }

        public Task SendMediaDataAsync(UInt32 consumerId, UInt32 id, Byte[] data)
        {
            var offlineTask = OfflineCameraService.SendMediaDataAsync(consumerId, id, data);
            var onlineTask = OnlineCameraService.SendMediaDataAsync(consumerId, id, data);
            return Task.WhenAll(offlineTask, onlineTask);
        }

        public Task SendMediaDataDescriptorsAsync(UInt32 consumerId, UInt32 id, IReadOnlyCollection<StoredRecordFile.MediaDataDescriptor> descriptors)
        {
            var offlineTask = OfflineCameraService.SendMediaDataDescriptorsAsync(consumerId, id, descriptors);
            var onlineTask = OnlineCameraService.SendMediaDataDescriptorsAsync(consumerId, id, descriptors);
            return Task.WhenAll(offlineTask, onlineTask);
        }

        public Task SendStoredRecordsMetadataAsync(UInt32 consumerId, UInt32 id, IReadOnlyCollection<(String Id, DateTime Date)> storedRecordsMetadata)
        {
            var offlineTask = OfflineCameraService.SendStoredRecordsMetadataAsync(consumerId, id, storedRecordsMetadata);
            var onlineTask = OnlineCameraService.SendStoredRecordsMetadataAsync(consumerId, id, storedRecordsMetadata);
            return Task.WhenAll(offlineTask, onlineTask);
        }

        public Task SetPushChannelSettingsAsync(String clientId, String clientSecret)
        {
            var offlineTask = OfflineCameraService.SetPushChannelSettingsAsync(clientId, clientSecret);
            var onlineTask = OnlineCameraService.SetPushChannelSettingsAsync(clientId, clientSecret);
            return Task.WhenAll(offlineTask, onlineTask);
        }

        public Task SetPushMessageAsync(String content)
        {
            var offlineTask = OfflineCameraService.SetPushMessageAsync(content);
            var onlineTask = OnlineCameraService.SetPushMessageAsync(content);
            return Task.WhenAll(offlineTask, onlineTask);
        }
    }
}