using HomeControl.Surveillance.Player.Model;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.MediaProperties;

namespace HomeControl.Surveillance.Player.ViewModel
{
    public class CameraStream: IDisposable
    {
        private CameraController Camera;
        private Boolean IsDisposed;
        private TimeSpan NextSampleTimestamp;
        private Object Sync = new Object();
        private IList<Byte[]> VideoSamples = new List<Byte[]>();

        public MediaStreamSource MediaStream { get; }



        public CameraStream(CameraController camera)
        {
            Camera = camera;
            Camera.DataReceived += OnCameraDataReceived;
            MediaStream = new MediaStreamSource(new VideoStreamDescriptor(VideoEncodingProperties.CreateH264()));
            MediaStream.SampleRequested += OnMediaStreamSampleRequested;
        }

        public void Synchronize()
        {
            lock (Sync)
                VideoSamples.Clear();
        }

        private async void OnMediaStreamSampleRequested(MediaStreamSource sender, MediaStreamSourceSampleRequestedEventArgs args)
        {
            var deferal = args.Request.GetDeferral();
            while (!IsDisposed)
            {
                var data = TryGetVideoSample();
                if (data == null)
                {
                    await Task.Delay(10).ConfigureAwait(false);
                    continue;
                }

                args.Request.Sample = MediaStreamSample.CreateFromBuffer(data.AsBuffer(), NextSampleTimestamp);
                args.Request.Sample.Duration = Camera.SampleDuration;
                NextSampleTimestamp += Camera.SampleDuration;
                break;
            }
            deferal.Complete();
        }

        private void OnCameraDataReceived(CameraController sender, Byte[] data)
        {
            lock (Sync)
                VideoSamples.Add(data);
        }

        private Byte[] TryGetVideoSample()
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

        public void Dispose()
        {
            IsDisposed = true;
            MediaStream.SampleRequested -= OnMediaStreamSampleRequested;
            Camera.DataReceived -= OnCameraDataReceived;
        }
    }
}
