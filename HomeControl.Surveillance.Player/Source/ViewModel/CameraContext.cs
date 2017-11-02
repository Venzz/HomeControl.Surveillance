using HomeControl.Surveillance.Player.Model;
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.MediaProperties;

namespace HomeControl.Surveillance.Player.ViewModel
{
    public class CameraContext: IDisposable
    {
        private TimeSpan NextSampleTimestamp;
        private Boolean IsDisposed;
        private CameraController CameraController;

        public MediaStreamSource MediaSource { get; private set; }



        public CameraContext()
        {
        }

        public void Initialize(CameraController cameraController)
        {
            CameraController = cameraController;
            MediaSource = new MediaStreamSource(new VideoStreamDescriptor(VideoEncodingProperties.CreateH264()));
            MediaSource.SampleRequested += OnMediaSourceSampleRequested;
        }

        private async void OnMediaSourceSampleRequested(MediaStreamSource sender, MediaStreamSourceSampleRequestedEventArgs args)
        {
            var deferal = args.Request.GetDeferral();
            while (!IsDisposed)
            {
                var data = CameraController.TryGetVideoSample();
                if (data == null)
                {
                    await Task.Delay(10).ConfigureAwait(false);
                    continue;
                }
                
                args.Request.Sample = MediaStreamSample.CreateFromBuffer(data.AsBuffer(), NextSampleTimestamp);
                args.Request.Sample.Duration = CameraController.SampleDuration;
                NextSampleTimestamp += CameraController.SampleDuration;
                break;
            }
            deferal.Complete();
        }

        public void Dispose()
        {
            IsDisposed = true;
            MediaSource.SampleRequested -= OnMediaSourceSampleRequested;
        }
    }
}
