using HomeControl.Surveillance.Data.Camera;
using System;
using System.Threading.Tasks;
using Windows.Foundation;

namespace HomeControl.Surveillance.Player.Model
{
    public class CameraController
    {
        private IConsumerCameraService ConsumerService;

        public TimeSpan SampleDuration { get; }
        public Boolean SupportsCommands { get; }

        public event TypedEventHandler<CameraController, Byte[]> DataReceived = delegate { };



        public CameraController(IConsumerCameraService consumerService, Boolean supportsCommands, TimeSpan sampleDuration)
        {
            SampleDuration = sampleDuration;
            SupportsCommands = supportsCommands;
            ConsumerService = consumerService;
            ConsumerService.DataReceived += (sender, data) => DataReceived(this, data);
        }

        public Task StartZoomingInAsync() => !SupportsCommands ? Task.CompletedTask : ConsumerService.PerformAsync(Command.StartZoomingIn);

        public Task StartZoomingOutAsync() => !SupportsCommands ? Task.CompletedTask : ConsumerService.PerformAsync(Command.StartZoomingOut);

        public Task StopZoomingAsync() => !SupportsCommands ? Task.CompletedTask : ConsumerService.PerformAsync(Command.StopZooming);
    }
}
