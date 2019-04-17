using HomeControl.Surveillance.Data.Camera;
using HomeControl.Surveillance.Data.Camera.Heroku;
using HomeControl.Surveillance.Server.Data;
using HomeControl.Surveillance.Server.Data.DemoClip;
using HomeControl.Surveillance.Server.Data.File;
using HomeControl.Surveillance.Server.Data.OrientProtocol;
using HomeControl.Surveillance.Server.Data.Rtsp;
using System;
using System.Threading.Tasks;

namespace HomeControl.Surveillance.Server.Model
{
    public class ApplicationModel
    {
        private ICameraConnection IndoorCameraConnection;
        private ICameraConnection OutdoorCameraConnection;
        private Storage Storage;
        private MotionDetection MotionDetection = new MotionDetection();

        public Camera IndoorCamera { get; }
        public Camera OutdoorCamera { get; }



        public ApplicationModel()
        {
            var indoorProviderCameraService = new HerokuProviderCameraService("indoor-service", (new TimeSpan(23, 0, 0), TimeSpan.FromHours(8)));
            indoorProviderCameraService.LogReceived += OnLogReceived;
            indoorProviderCameraService.ExceptionReceived += OnExceptionReceived;
            IndoorCamera = new Camera(indoorProviderCameraService);
            IndoorCamera.ExceptionReceived += OnExceptionReceived;

            var outdoorProviderCameraService = new HerokuProviderCameraService("outdoor-service", (new TimeSpan(23, 0, 0), TimeSpan.FromHours(8)));
            outdoorProviderCameraService.MessageReceived += OnMessageReceived;
            outdoorProviderCameraService.LogReceived += OnLogReceived;
            outdoorProviderCameraService.ExceptionReceived += OnExceptionReceived;
            OutdoorCamera = new Camera(outdoorProviderCameraService);
            OutdoorCamera.ExceptionReceived += OnExceptionReceived;

            Storage = new Storage(new FileStorageService());
            Storage.ExceptionReceived += OnExceptionReceived;
        }

        public void Initialize()
        {
            IndoorCameraConnection = new RtspCameraConnection("192.168.1.168", 554, PrivateData.IndoorCameraRtspUrl);
            IndoorCameraConnection.MediaReceived += (sender, media) => IndoorCamera.Send(media);
            IndoorCameraConnection.LogReceived += OnLogReceived;
            IndoorCameraConnection.ExceptionReceived += OnExceptionReceived;
            #if DEBUG
            OutdoorCameraConnection = new DemoClipCameraConnection();
            #else
            OutdoorCameraConnection = new OrientProtocolCameraConnection("192.168.1.10", 34567);
            #endif
            OutdoorCameraConnection.MediaReceived += (sender, media) => OutdoorCamera.Send(media);
            OutdoorCameraConnection.MediaReceived += (sender, media) => Storage.Store(media);
            OutdoorCameraConnection.MediaReceived += (sender, media) => MotionDetection.Process(media);
            OutdoorCamera.CommandReceived += OnOutdoorCameraCommandReceived;
            OutdoorCameraConnection.LogReceived += OnLogReceived;
            OutdoorCameraConnection.ExceptionReceived += OnExceptionReceived;
            MotionDetection.Detected += OnMotionDetected;
            MotionDetection.LogReceived += OnLogReceived;
            MotionDetection.Start();
        }

        private async void OnMessageReceived(IProviderCameraService sender, (UInt32 Id, IMessage Message) args) => await Task.Run(async () =>
        {
            switch (args.Message.Type)
            {
                case MessageId.StoredRecordsMetadataRequest:
                    var storedRecordsMetadata = Storage.GetStoredRecordsMetadata();
                    await sender.SendStoredRecordsMetadataAsync(args.Id, storedRecordsMetadata).ConfigureAwait(false);
                    break;
                case MessageId.StoredRecordMediaDescriptorsRequest:
                    var storedRecordMediaDescriptorsRequest = (StoredRecordMediaDescriptorsRequest)args.Message;
                    var mediaDescriptors = Storage.GetStoredRecordMediaDescriptors(storedRecordMediaDescriptorsRequest.StoredRecordId);
                    await sender.SendMediaDataDescriptorsAsync(args.Id, mediaDescriptors).ConfigureAwait(false);
                    break;
                case MessageId.StoredRecordMediaDataRequest:
                    var storedRecordMediaDataRequest = (StoredRecordMediaDataRequest)args.Message;
                    var mediaData = Storage.GetStoredRecordMediaData(storedRecordMediaDataRequest.StoredRecordId, storedRecordMediaDataRequest.Offset);
                    await sender.SendMediaDataAsync(args.Id, mediaData).ConfigureAwait(false);
                    break;
            }
        });

        private async void OnOutdoorCameraCommandReceived(Camera sender, Command command)
        {
            switch (command)
            {
                case Command.StartZoomingIn:
                    await OutdoorCameraConnection.StartZoomingInAsync().ConfigureAwait(false);
                    break;
                case Command.StartZoomingOut:
                    await OutdoorCameraConnection.StartZoomingOutAsync().ConfigureAwait(false);
                    break;
                case Command.StopZooming:
                    await OutdoorCameraConnection.StopZoomingAsync().ConfigureAwait(false);
                    break;
            }
        }

        private void OnMotionDetected(MotionDetection sender, Object args)
        {
        }

        private void OnLogReceived(Object sender, (String Message, String Parameter) args)
        {
            App.Diagnostics.Debug.Log(args.Message, args.Parameter);
        }

        private void OnExceptionReceived(Object sender, (String Message, Exception Exception) args)
        {
            App.Diagnostics.Debug.Log(args.Message, args.Exception);
        }
    }
}
