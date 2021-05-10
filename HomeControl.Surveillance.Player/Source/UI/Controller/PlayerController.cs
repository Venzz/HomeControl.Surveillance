using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Streams;

namespace HomeControl.Surveillance.Player.UI.Controller
{
    public class PlayerController: IDisposable
    {
        private IRandomAccessStream Stream;
        private IList<StoredRecordFile.MediaDataDescriptor> MediaDescriptors;
        private UInt32 CurrentSampleIndex;
        private TimeSpan CurrentSampleTimestamp;

        public MediaPlayer MediaPlayer { get; } = new MediaPlayer();
        public MediaStreamSource MediaStream { get; private set; }



        public PlayerController() { }

        public async Task OpenAsync(StorageFile file)
        {
            Stream = await file.OpenReadAsync().AsTask().ConfigureAwait(false);
            var mediaDescriptors = new StoredRecordFile(Stream.AsStream()).ReadMediaDescriptors();
            MediaDescriptors = mediaDescriptors.Where(a => (a.Type == MediaDataType.InterFrame || a.Type == MediaDataType.PredictionFrame)).SkipWhile(a => a.Type == MediaDataType.PredictionFrame).ToList();

            var duration = TimeSpan.FromSeconds(MediaDescriptors.Sum(a => a.Duration.TotalSeconds));
            MediaStream = new MediaStreamSource(new VideoStreamDescriptor(VideoEncodingProperties.CreateH264()));
            MediaStream.SampleRequested += OnMediaStreamSampleRequested;
            MediaStream.Starting += OnMediaStreamStarting;
            MediaStream.CanSeek = true;
            MediaStream.Duration = duration;
            MediaPlayer.PlaybackSession.Position = new TimeSpan();
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
            if (CurrentSampleIndex >= MediaDescriptors.Count)
                return;

            var deferral = args.Request.GetDeferral();
            var mediaDescriptor = MediaDescriptors[(Int32)CurrentSampleIndex++];
            var sampleData = (IBuffer)null;
            using (var reader = new DataReader(Stream) { ByteOrder = ByteOrder.LittleEndian })
            {
                Stream.Seek(mediaDescriptor.Offset);
                await reader.LoadAsync(4).AsTask().ConfigureAwait(false);
                var size = reader.ReadUInt32();
                await reader.LoadAsync(size).AsTask().ConfigureAwait(false);
                sampleData = reader.ReadBuffer(size);
                reader.DetachStream();
            }
            
            args.Request.Sample = MediaStreamSample.CreateFromBuffer(sampleData, CurrentSampleTimestamp);
            args.Request.Sample.Duration = mediaDescriptor.Duration;
            CurrentSampleTimestamp += mediaDescriptor.Duration;
            deferral.Complete();
        }

        public void Dispose() => Stream.Dispose();
    }
}
