using HomeControl.Surveillance.Data.Camera;
using HomeControl.Surveillance.Data.Camera.Heroku;
using System;

namespace HomeControl.Surveillance.Player.Model
{
    public class ApplicationModel
    {
        public CameraController OutdoorCameraController { get; }

        public ApplicationModel()
        {
            var outdoorConsumerCameraService = new HerokuConsumerCameraService("outdoor-client");
            outdoorConsumerCameraService.LogReceived += OnCameraServiceLogReceived;
            outdoorConsumerCameraService.ExceptionReceived += OnCameraServiceExceptionReceived;
            OutdoorCameraController = new CameraController(outdoorConsumerCameraService, supportsCommands: true, sampleDuration: TimeSpan.FromMilliseconds(13));
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
