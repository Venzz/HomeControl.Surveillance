using HomeControl.Surveillance.Data.Camera;
using HomeControl.Surveillance.Data.Camera.Heroku;
using System;
using System.Threading.Tasks;

namespace HomeControl.Surveillance.Player.Model
{
    public class ApplicationModel
    {
        private IConsumerCameraService ConsumerCameraService;
        private PushNotification PushNotification;

        public CameraController OutdoorCameraController { get; }
        public CameraController IndoorCameraController { get; }
        public ProviderController ProviderController { get; }

        public ApplicationModel()
        {
            ConsumerCameraService = new HerokuConsumerCameraService("client");
            ConsumerCameraService.LogReceived += OnCameraServiceLogReceived;
            ConsumerCameraService.ExceptionReceived += OnCameraServiceExceptionReceived;
            PushNotification = new PushNotification(ConsumerCameraService);

            IndoorCameraController = new CameraController(ConsumerCameraService, supportsCommands: false, sampleDuration: TimeSpan.FromMilliseconds(42), title: "Indoor");
            OutdoorCameraController = new CameraController(ConsumerCameraService, supportsCommands: true, sampleDuration: TimeSpan.FromMilliseconds(13), title: "Outdoor");
            ProviderController = new ProviderController(ConsumerCameraService);
        }

        public async Task InitializeAsync()
        {
            await PushNotification.UpdateUriAsync().ConfigureAwait(false);
        }

        private void OnCameraServiceLogReceived(IConsumerCameraService sender, (String Message, String Parameter) args)
        {
            App.Diagnostics.Debug.Log(args.Message, args.Parameter);
        }

        private void OnCameraServiceExceptionReceived(IConsumerCameraService sender, (String Message, Exception Exception) args)
        {
            App.Diagnostics.Debug.Log(args.Message, args.Exception);
        }
    }
}
