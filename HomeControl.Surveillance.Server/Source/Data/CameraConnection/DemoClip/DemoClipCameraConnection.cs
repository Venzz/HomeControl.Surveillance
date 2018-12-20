﻿using System;
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
                if (Data[index].Length < 200)
                    MediaReceived(this, new AudioMediaData(Data[index], DateTime.UtcNow, TimeSpan.FromSeconds(1.0 / 50)));
                else if (Data[index].Length < 6000)
                    MediaReceived(this, new PredictionFrameMediaData(Data[index], DateTime.UtcNow, TimeSpan.FromSeconds(1.0 / 12.5)));
                else
                    MediaReceived(this, new InterFrameMediaData(Data[index], DateTime.UtcNow, TimeSpan.FromSeconds(1.0 / 12.5)));
                index = (index + 1) % Data.Count;
                await Task.Delay(15);
            }
        });
    }
}
