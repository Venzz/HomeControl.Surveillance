using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Foundation;

namespace HomeControl.Surveillance.Server.Data.DemoClip
{
    public class DemoClipCameraConnection: ICameraConnection
    {
        private List<Byte[]> Data = new List<Byte[]>();

        public Boolean IsZoomingSupported => false;

        public event TypedEventHandler<ICameraConnection, Byte[]> DataReceived = delegate { };
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
            using (var fileStream = new FileStream($"Source\\Data\\CameraConnection\\DemoClip\\DemoClip.h264", FileMode.Open))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                while (fileStream.Position < fileStream.Length)
                {
                    var size = binaryReader.ReadInt32();
                    var data = binaryReader.ReadBytes(size);
                    Data.Add(data);
                }
            }

            var index = 0;
            while (true)
            {
                
                DataReceived(this, Data[index]);
                index = (index + 1) % Data.Count;
                await Task.Delay(15);
            }
        });
    }
}
