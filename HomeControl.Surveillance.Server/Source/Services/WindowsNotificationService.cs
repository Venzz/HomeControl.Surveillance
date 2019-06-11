using HomeControl.Surveillance.Data.Camera;
using System.Threading.Tasks;

namespace HomeControl.Surveillance.Server
{
    public class WindowsNotificationService: INotificationService
    {
        private IProviderCameraService ProviderService;

        public WindowsNotificationService(IProviderCameraService providerService)
        {
            ProviderService = providerService;
        }

        public Task InitializeAsync()
        {
            ProviderService.EnsureConnected();
            return ProviderService.SetPushChannelSettingsAsync(PrivateData.PushChannelClientId, PrivateData.PushChannelClientSecret);
        }
    }
}
