using HomeControl.Surveillance.Services;
using System;
using System.Threading.Tasks;

namespace HomeControl.Surveillance.Player.Model
{
    public class ApplicationModel
    {
        private IConsumerCameraService ConsumerCameraService;
        private PushNotification PushNotification;

        public Camera OutdoorCameraController { get; }
        public Camera IndoorCameraController { get; }

        public ApplicationModel()
        {
            ConsumerCameraService = new HerokuConsumerCameraService("client");
            ConsumerCameraService.LogReceived += OnCameraServiceLogReceived;
            ConsumerCameraService.ExceptionReceived += OnCameraServiceExceptionReceived;
            PushNotification = new PushNotification(ConsumerCameraService);

            IndoorCameraController = new Camera(ConsumerCameraService, supportsCommands: false, sampleDuration: TimeSpan.FromMilliseconds(42), title: "Indoor");
            OutdoorCameraController = new Camera(ConsumerCameraService, supportsCommands: true, sampleDuration: TimeSpan.FromMilliseconds(13), title: "Outdoor");
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
