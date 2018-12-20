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
        private VideoEncodingProperties EncodingProperties;
        private TimeSpan SampleTimestamp;
        private Object Sync = new Object();
        private IList<MediaData> VideoSamples = new List<MediaData>();

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
                VideoSamples.Clear();
        }

        private async void OnMediaStreamSampleRequested(MediaStreamSource sender, MediaStreamSourceSampleRequestedEventArgs args)
        {
            var deferal = args.Request.GetDeferral();
            while (!IsDisposed)
            {
                var mediaData = TryGetVideoSample();
                if (mediaData == null)
                {
                    await Task.Delay(10).ConfigureAwait(false);
                    continue;
                }

                args.Request.Sample = MediaStreamSample.CreateFromBuffer(mediaData.Data.AsBuffer(), SampleTimestamp);
                args.Request.Sample.Duration = mediaData.Duration;
                SampleTimestamp += mediaData.Duration;
                break;
            }
            deferal.Complete();
        }

        private void OnMediaStreamClosed(MediaStreamSource sender, MediaStreamSourceClosedEventArgs args)
        {
            MediaStream = new MediaStreamSource(new VideoStreamDescriptor(EncodingProperties));
        }

        private void OnCameraDataReceived(CameraController sender, MediaData mediaData)
        {
            if (mediaData.Type == MediaDataType.AudioFrame)
                return;

            lock (Sync)
                VideoSamples.Add(mediaData);
        }

        private MediaData TryGetVideoSample()
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
