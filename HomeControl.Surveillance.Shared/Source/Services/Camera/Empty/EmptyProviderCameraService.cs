using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;

namespace HomeControl.Surveillance.Services
{
    public class EmptyProviderCameraService: IProviderCameraService
    {
        public event TypedEventHandler<IProviderCameraService, Command> CommandReceived = delegate { };
        public event TypedEventHandler<IProviderCameraService, (UInt32 ConsumerId, UInt32 Id, IMessage Message)> MessageReceived = delegate { };
        public event TypedEventHandler<IProviderCameraService, (String Source, String Message)> Log = delegate { };
        public event TypedEventHandler<IProviderCameraService, (String Source, String Details, Exception Exception)> Exception = delegate { };

        public EmptyProviderCameraService() { }

        public void EnsureConnected()
        {
        }

        public Task SendStoredRecordsMetadataAsync(UInt32 consumerId, UInt32 id, IReadOnlyCollection<(String Id, DateTime Date)> storedRecordsMetadata)
        {
            return Task.CompletedTask;
        }

        public Task SendLiveMediaDataAsync(MediaDataType type, Byte[] data, DateTime timestamp, TimeSpan duration)
        {
            return Task.CompletedTask;
        }

        public Task SendMediaDataDescriptorsAsync(UInt32 consumerId, UInt32 id, IReadOnlyCollection<StoredRecordFile.MediaDataDescriptor> descriptors)
        {
            return Task.CompletedTask;
        }

        public Task SendMediaDataAsync(UInt32 consumerId, UInt32 id, Byte[] data)
        {
            return Task.CompletedTask;
        }

        public Task SetPushChannelSettingsAsync(String clientId, String clientSecret)
        {
            return Task.CompletedTask;
        }

        public Task SetPushMessageAsync(String content)
        {
            return Task.CompletedTask;
        }
    }
}