
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Foundation;

namespace HomeControl.Surveillance.Server.Services
{
    public class DemoClipCameraConnection: ICameraConnection
    {
        private List<IMediaData> Data = new List<IMediaData>();

        public Boolean IsZoomingSupported => false;

        public event TypedEventHandler<ICameraConnection, IMediaData> MediaReceived = delegate { };
        public event TypedEventHandler<ICameraConnection, (String CustomText, Exception Exception)> ExceptionReceived = delegate { };
        public event TypedEventHandler<ICameraConnection, (String CustomText, String Parameter)> LogReceived = delegate { };



        public DemoClipCameraConnection()
        {
            StartDataGeneration();
        }

        public Task StartZoomingInAsync() => Task.CompletedTask;

        public Task StartZoomingOutAsync() => Task.CompletedTask;

        public Task StopZoomingAsync() => Task.CompletedTask;

        private async void StartDataGeneration() => await Task.Run(async () =>
        {
            using (var fileStream = new FileStream($"Source\\Data\\CameraConnection\\DemoClip\\DemoClip.sr", FileMode.Open))
            using (var fileStreamReader = new BinaryReader(fileStream))
            {
                var storedRecord = new StoredRecordFile(fileStream);
                foreach (var mediaDescriptor in storedRecord.ReadMediaDescriptors())
                {
                    fileStream.Seek(mediaDescriptor.Offset, SeekOrigin.Begin);
                    var dataLength = fileStreamReader.ReadInt32();
                    var data = fileStreamReader.ReadBytes(dataLength);
                    switch (mediaDescriptor.Type)
                    {
                        case MediaDataType.AudioFrame:
                            Data.Add(new AudioMediaData(data, mediaDescriptor.Timestamp, mediaDescriptor.Duration));
                            break;
                        case MediaDataType.InterFrame:
                            Data.Add(new InterFrameMediaData(data, mediaDescriptor.Timestamp, mediaDescriptor.Duration));
                            break;
                        case MediaDataType.PredictionFrame:
                            Data.Add(new PredictionFrameMediaData(data, mediaDescriptor.Timestamp, mediaDescriptor.Duration));
                            break;
                    }
                }
            }

            var currentIndex = 0;
            while (true)
            {
                var nextIndex = (currentIndex + 1) % Data.Count;
                MediaReceived(this, Data[currentIndex]);
                currentIndex = nextIndex;
                await Task.Delay(10);
            }
        });
    }
}
