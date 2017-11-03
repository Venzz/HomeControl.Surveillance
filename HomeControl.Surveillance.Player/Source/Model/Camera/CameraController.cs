using HomeControl.Surveillance.Data.Camera;
using System;
using Windows.Foundation;

namespace HomeControl.Surveillance.Player.Model
{
    public class CameraController
    {
        private IConsumerCameraService ConsumerService;

        public TimeSpan SampleDuration { get; }

        public event TypedEventHandler<CameraController, Byte[]> DataReceived = delegate { };



        public CameraController(IConsumerCameraService consumerService, TimeSpan sampleDuration)
        {
            SampleDuration = sampleDuration;
            ConsumerService = consumerService;
            ConsumerService.DataReceived += (sender, data) => DataReceived(this, data);
        }
    }
}
