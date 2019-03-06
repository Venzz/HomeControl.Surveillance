using HomeControl.Surveillance.Data.Storage;
using HomeControl.Surveillance.Player.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Media.Core;
using Windows.Media.MediaProperties;

namespace HomeControl.Surveillance.Player.ViewModel
{
    public class StoredRecordStream: IDisposable
    {
        private Boolean IsDisposed;
        private CameraController Camera;
        private IList<StoredRecordFile.MediaDataDescriptor> MediaDescriptors;
        private String StoredRecordId;
        private UInt32 CurrentSampleIndex;
        private TimeSpan CurrentSampleTimestamp;

        public MediaStreamSource MediaStream { get; private set; }



        public StoredRecordStream(CameraController cameraController, String storedRecordId, IReadOnlyCollection<StoredRecordFile.MediaDataDescriptor> mediaDescriptors)
        {
            Camera = cameraController;
            StoredRecordId = storedRecordId;
            MediaDescriptors = mediaDescriptors.Where(a => (a.Type == MediaDataType.InterFrame || a.Type == MediaDataType.PredictionFrame)).SkipWhile(a => a.Type == MediaDataType.PredictionFrame).ToList();
            MediaStream = new MediaStreamSource(new VideoStreamDescriptor(VideoEncodingProperties.CreateH264()));
            MediaStream.CanSeek = true;
            MediaStream.Duration = TimeSpan.FromSeconds(MediaDescriptors.Sum(a => a.Duration.TotalSeconds));
            MediaStream.Starting += OnMediaStreamStarting;
            MediaStream.SampleRequested += OnMediaStreamSampleRequested;
            MediaStream.Closed += OnMediaStreamClosed;
        }

        private void OnMediaStreamStarting(MediaStreamSource sender, MediaStreamSourceStartingEventArgs args)
        {
            if (!args.Request.StartPosition.HasValue)
                return;

            var index = 0;
            CurrentSampleTimestamp = new TimeSpan();
            while (CurrentSampleTimestamp < args.Request.StartPosition.Value)
            {
                CurrentSampleTimestamp += MediaDescriptors[index].Duration;
                index++;
            }
            while ((index >= 0) && (MediaDescriptors[index].Type != MediaDataType.InterFrame))
            {
                index--;
                if (index >= 0)
                    CurrentSampleTimestamp -= MediaDescriptors[index].Duration;
            }
            while ((index < 0) || (MediaDescriptors[index].Type != MediaDataType.InterFrame))
            {
                if (index >= 0)
                    CurrentSampleTimestamp += MediaDescriptors[index].Duration;
                index++;
            }
            CurrentSampleIndex = (UInt32)index;
        }

        private async void OnMediaStreamSampleRequested(MediaStreamSource sender, MediaStreamSourceSampleRequestedEventArgs args)
        {
            if (IsDisposed || (CurrentSampleIndex >= MediaDescriptors.Count))
                return;

            var deferral = args.Request.GetDeferral();
            var mediaDescriptor = MediaDescriptors[(Int32)CurrentSampleIndex++];
            var sampleData = await Camera.GetMediaDataAsync(StoredRecordId, mediaDescriptor.Offset).ConfigureAwait(false);

            args.Request.Sample = MediaStreamSample.CreateFromBuffer(sampleData.AsBuffer(), CurrentSampleTimestamp);
            args.Request.Sample.Duration = mediaDescriptor.Duration;
            CurrentSampleTimestamp += mediaDescriptor.Duration;
            deferral.Complete();
        }

        private void OnMediaStreamClosed(MediaStreamSource sender, MediaStreamSourceClosedEventArgs args)
        {
            MediaStream.SampleRequested -= OnMediaStreamSampleRequested;
            MediaStream.Closed -= OnMediaStreamClosed;
        }

        public void Dispose()
        {
            IsDisposed = true;
            MediaStream.SampleRequested -= OnMediaStreamSampleRequested;
            MediaStream.Closed -= OnMediaStreamClosed;
        }
    }
}
