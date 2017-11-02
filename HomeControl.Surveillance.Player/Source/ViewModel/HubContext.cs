using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.MediaProperties;

namespace HomeControl.Surveillance.Player.ViewModel
{
    public class HubContext: IDisposable
    {
        private TimeSpan NextSampleTimestamp;
        private Boolean IsDisposed;

        public MediaStreamSource OutdoorCameraMediaSource { get; private set; }
        public MediaStreamSource IndoorCameraMediaSource { get; private set; }



        public HubContext()
        {
        }

        public void Initialize()
        {
            OutdoorCameraMediaSource = new MediaStreamSource(new VideoStreamDescriptor(VideoEncodingProperties.CreateH264()));
            OutdoorCameraMediaSource.SampleRequested += OnOutdoorCameraSampleRequested;
            IndoorCameraMediaSource = new MediaStreamSource(new VideoStreamDescriptor(VideoEncodingProperties.CreateH264()));
            IndoorCameraMediaSource.SampleRequested += OnIndoorCameraSampleRequested;
        }

        private async void OnOutdoorCameraSampleRequested(MediaStreamSource sender, MediaStreamSourceSampleRequestedEventArgs args)
        {
            var deferal = args.Request.GetDeferral();
            while (!IsDisposed)
            {
                var data = App.Model.OutdoorCameraController.TryGetVideoSample();
                if (data == null)
                {
                    await Task.Delay(10).ConfigureAwait(false);
                    continue;
                }
                
                args.Request.Sample = MediaStreamSample.CreateFromBuffer(data.AsBuffer(), NextSampleTimestamp);
                args.Request.Sample.Duration = App.Model.OutdoorCameraController.SampleDuration;
                NextSampleTimestamp += App.Model.OutdoorCameraController.SampleDuration;
                break;
            }
            deferal.Complete();
        }

        private async void OnIndoorCameraSampleRequested(MediaStreamSource sender, MediaStreamSourceSampleRequestedEventArgs args)
        {
            var deferal = args.Request.GetDeferral();
            while (!IsDisposed)
            {
                var data = App.Model.IndoorCameraController.TryGetVideoSample();
                if (data == null)
                {
                    await Task.Delay(10).ConfigureAwait(false);
                    continue;
                }
                
                args.Request.Sample = MediaStreamSample.CreateFromBuffer(data.AsBuffer(), NextSampleTimestamp);
                args.Request.Sample.Duration = App.Model.IndoorCameraController.SampleDuration;
                NextSampleTimestamp += App.Model.IndoorCameraController.SampleDuration;
                break;
            }
            deferal.Complete();
        }

        public void Dispose()
        {
            IsDisposed = true;
            OutdoorCameraMediaSource.SampleRequested -= OnOutdoorCameraSampleRequested;
            IndoorCameraMediaSource.SampleRequested -= OnIndoorCameraSampleRequested;
        }
    }
}
