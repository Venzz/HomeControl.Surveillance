using HomeControl.Surveillance.Data.Camera;
using HomeControl.Surveillance.Data.Camera.Heroku;
using HomeControl.Surveillance.Server.Data;
using HomeControl.Surveillance.Server.Data.Empty;
using HomeControl.Surveillance.Server.Data.OrientProtocol;
using HomeControl.Surveillance.Server.Data.Rtsp;
using System;

namespace HomeControl.Surveillance.Server.Model
{
    public class ApplicationModel
    {
        private ICameraConnection IndoorCameraConnection;
        private ICameraConnection OutdoorCameraConnection;
        private Storage Storage;

        public Camera IndoorCamera { get; }
        public Camera OutdoorCamera { get; }



        public ApplicationModel()
        {
            var indoorProviderCameraService = new HerokuProviderCameraService("indoor-service");
            indoorProviderCameraService.LogReceived += OnProviderCameraServiceLogReceived;
            indoorProviderCameraService.ExceptionReceived += OnProviderCameraServiceExceptionReceived;
            IndoorCamera = new Camera(indoorProviderCameraService);

            var outdoorProviderCameraService = new HerokuProviderCameraService("outdoor-service");
            outdoorProviderCameraService.LogReceived += OnProviderCameraServiceLogReceived;
            outdoorProviderCameraService.ExceptionReceived += OnProviderCameraServiceExceptionReceived;
            OutdoorCamera = new Camera(outdoorProviderCameraService);

            Storage = new Storage(new EmptyStorageService());
        }

        public void Initialize()
        {
            IndoorCameraConnection = new RtspCameraConnection("192.168.1.168", 554, PrivateData.IndoorCameraRtspUrl);
            IndoorCameraConnection.DataReceived += (sender, data) => IndoorCamera.Send(data);
            IndoorCameraConnection.DataReceived += (sender, data) => Storage.Store(data);
            OutdoorCameraConnection = new OrientProtocolCameraConnection("192.168.1.10", 34567);
            OutdoorCameraConnection.DataReceived += (sender, data) => OutdoorCamera.Send(data);
        }

        private void OnProviderCameraServiceLogReceived(IProviderCameraService sender, (String Message, String Parameter) args)
        {
            App.Diagnostics.Debug.Log(args.Message, args.Parameter);
        }

        private void OnProviderCameraServiceExceptionReceived(IProviderCameraService sender, (String Message, Exception Exception) args)
        {
            App.Diagnostics.Debug.Log(args.Message, args.Exception);
        }
    }
}
