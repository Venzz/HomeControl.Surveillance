using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;

namespace HomeControl.Surveillance.Services
{
    public interface IProviderCameraService
    {
        event TypedEventHandler<IProviderCameraService, Command> CommandReceived;
        event TypedEventHandler<IProviderCameraService, (UInt32 ConsumerId, UInt32 Id, IMessage Message)> MessageReceived;
        event TypedEventHandler<IProviderCameraService, (String Message, String Parameter)> LogReceived;
        event TypedEventHandler<IProviderCameraService, (String Message, Exception Exception)> ExceptionReceived;

        void EnsureConnected();

        Task SendStoredRecordsMetadataAsync(UInt32 consumerId, UInt32 id, IReadOnlyCollection<(String Id, DateTime Date)> storedRecordsMetadata);
        Task SendLiveMediaDataAsync(MediaDataType type, Byte[] data, DateTime timestamp, TimeSpan duration);
        Task SendMediaDataDescriptorsAsync(UInt32 consumerId, UInt32 id, IReadOnlyCollection<StoredRecordFile.MediaDataDescriptor> descriptors);
        Task SendMediaDataAsync(UInt32 consumerId, UInt32 id, Byte[] data);

        Task SetPushChannelSettingsAsync(String clientId, String clientSecret);
        Task SetPushMessageAsync(String content);
    }
}
