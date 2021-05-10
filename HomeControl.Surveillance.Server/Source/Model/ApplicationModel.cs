using HomeControl.Surveillance.Server.Services;
using HomeControl.Surveillance.Services;
using HomeControl.Surveillance.Services.Heroku;
using System;
using System.Threading.Tasks;

namespace HomeControl.Surveillance.Server.Model
{
    public class ApplicationModel
    {
        private ICameraConnection OutdoorCameraConnection;
        private IProviderCameraService ProviderCameraService;
        private Storage Storage;
        private MotionDetection MotionDetection = new MotionDetection();
        private INotificationService NotificationService;

        public Camera IndoorCamera { get; }
        public Camera OutdoorCamera { get; }



        public ApplicationModel()
        {
            ProviderCameraService = new HerokuProviderCameraService("service", (new TimeSpan(23, 0, 0), TimeSpan.FromHours(8)));
            ProviderCameraService.MessageReceived += OnMessageReceived;
            ProviderCameraService.LogReceived += OnLogReceived;
            ProviderCameraService.ExceptionReceived += OnExceptionReceived;

            OutdoorCamera = new Camera(ProviderCameraService);
            OutdoorCamera.ExceptionReceived += OnExceptionReceived;

            #if DEBUG
            Storage = new Storage(new EmptyStorageService());
            #else
            Storage = new Storage(new FileStorageService());
            #endif
            Storage.LogReceived += OnLogReceived;
            Storage.ExceptionReceived += OnExceptionReceived;

            NotificationService = new WindowsNotificationService(ProviderCameraService);
        }

        public async Task InitializeAsync()
        {
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

            await NotificationService.InitializeAsync().ConfigureAwait(false);
        }

        private async void OnMessageReceived(IProviderCameraService sender, (UInt32 ConsumerId, UInt32 Id, IMessage Message) args) => await Task.Run(async () =>
        {
            switch (args.Message.Type)
            {
                case MessageId.StoredRecordsMetadataRequest:
                    var storedRecordsMetadata = Storage.GetStoredRecordsMetadata();
                    await sender.SendStoredRecordsMetadataAsync(args.ConsumerId, args.Id, storedRecordsMetadata).ConfigureAwait(false);
                    break;
                case MessageId.StoredRecordMediaDescriptorsRequest:
                    var storedRecordMediaDescriptorsRequest = (StoredRecordMediaDescriptorsRequest)args.Message;
                    var mediaDescriptors = Storage.GetStoredRecordMediaDescriptors(storedRecordMediaDescriptorsRequest.StoredRecordId);
                    await sender.SendMediaDataDescriptorsAsync(args.ConsumerId, args.Id, mediaDescriptors).ConfigureAwait(false);
                    break;
                case MessageId.StoredRecordMediaDataRequest:
                    var storedRecordMediaDataRequest = (StoredRecordMediaDataRequest)args.Message;
                    var mediaData = Storage.GetStoredRecordMediaData(storedRecordMediaDataRequest.StoredRecordId, storedRecordMediaDataRequest.Offset);
                    await sender.SendMediaDataAsync(args.ConsumerId, args.Id, mediaData).ConfigureAwait(false);
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
