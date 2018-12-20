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
        private DateTime? StartDate;
        private VideoEncodingProperties EncodingProperties;
        private Object Sync = new Object();
        private IList<Byte[]> VideoSamples = new List<Byte[]>();

        public MediaStreamSource MediaStream { get; private set; }



        public CameraStream(CameraController camera)
        {
            Camera = camera;
            Camera.DataReceived += OnCameraDataReceived;
            EncodingProperties = VideoEncodingProperties.CreateH264();
            #if WP81
            EncodingProperties.ProfileId = H264ProfileIds.High;
            EncodingProperties.Width = 1920;
            EncodingProperties.Height = 1080;
            #endif
            MediaStream = new MediaStreamSource(new VideoStreamDescriptor(EncodingProperties));
            MediaStream.SampleRequested += OnMediaStreamSampleRequested;
            MediaStream.Closed += OnMediaStreamClosed;
        }

        public void Synchronize()
        {
            lock (Sync)
            {
                VideoSamples.Clear();
                StartDate = null;
            }
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

                args.Request.Sample = MediaStreamSample.CreateFromBuffer(data.AsBuffer(), DateTime.Now - StartDate.Value);
                args.Request.Sample.Duration = Camera.SampleDuration;
                break;
            }
            deferal.Complete();
        }

        private void OnMediaStreamClosed(MediaStreamSource sender, MediaStreamSourceClosedEventArgs args)
        {
            MediaStream = new MediaStreamSource(new VideoStreamDescriptor(EncodingProperties));
        }

        private void OnCameraDataReceived(CameraController sender, (MediaDataType MediaType, Byte[] Data) args)
        {
            if (args.MediaType == MediaDataType.AudioFrame)
                return;

            lock (Sync)
            {
                if (!StartDate.HasValue)
                    StartDate = DateTime.Now;
                VideoSamples.Add(args.Data);
            }
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
            MediaStream.Closed -= OnMediaStreamClosed;
            Camera.DataReceived -= OnCameraDataReceived;
        }
    }
}
