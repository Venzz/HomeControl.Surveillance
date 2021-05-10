using HomeControl.Surveillance.Services;
using System;
using System.Threading.Tasks;
using Windows.Networking.PushNotifications;

namespace HomeControl.Surveillance.Player
{
    public class PushNotification
    {
        private IConsumerCameraService ConsumerService;

        public PushNotification(IConsumerCameraService consumerService)
        {
            ConsumerService = consumerService;
        }

        public async Task UpdateUriAsync()
        {
            var channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync().AsTask().ConfigureAwait(false);
            await ConsumerService.SetPushChannelUriAsync(App.Settings.PushChannelUri, channel.Uri).ConfigureAwait(false);
            App.Settings.PushChannelUri = channel.Uri;
        }
    }
}
