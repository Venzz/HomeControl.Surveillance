using HomeControl.Surveillance.Data.Camera;
using System;
using System.Collections.Generic;

namespace HomeControl.Surveillance.Player.Model
{
    public class CameraController
    {
        private IConsumerCameraService ConsumerService;
        private Object Sync = new Object();
        private List<Byte[]> VideoSamples = new List<Byte[]>();

        public TimeSpan SampleDuration { get; }



        public CameraController(IConsumerCameraService consumerService, TimeSpan sampleDuration)
        {
            SampleDuration = sampleDuration;
            ConsumerService = consumerService;
            ConsumerService.DataReceived += OnConsumerServiceDataReceived;
        }

        public Byte[] TryGetVideoSample()
        {
            lock (Sync)
            {
                if (VideoSamples.Count == 0)
                    return null;
                var sample = VideoSamples[0];
                VideoSamples.RemoveAt(0);
                return sample;
            }
        }

        private void OnConsumerServiceDataReceived(IConsumerCameraService sender, Byte[] data)
        {
            lock (Sync)
                VideoSamples.Add(data);
        }
    }
}
