using HomeControl.Surveillance.Data.Storage;
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

namespace HomeControl.Surveillance.Player.ViewModel
{
    public class PlayerContext: IDisposable
    {
        private IRandomAccessStream Stream;
        private IList<StoredRecordFile.MediaDataDescriptor> MediaDescriptors;
        private UInt32 CurrentSampleIndex;
        private TimeSpan CurrentSampleTimestamp;

        public MediaPlayer MediaPlayer { get; } = new MediaPlayer();
        public MediaStreamSource MediaStream { get; private set; }



        public PlayerContext() { }

        public async Task OpenAsync(StorageFile file)
        {
            Stream = await file.OpenReadAsync().AsTask().ConfigureAwait(false);
            var mediaDescriptors = StoredRecordFile.ReadMediaDescriptors(Stream.AsStream());
            MediaDescriptors = mediaDescriptors.Where(a => (a.Type == MediaDataType.InterFrame || a.Type == MediaDataType.PredictionFrame)).SkipWhile(a => a.Type == MediaDataType.PredictionFrame).ToList();

            MediaStream = new MediaStreamSource(new VideoStreamDescriptor(VideoEncodingProperties.CreateH264()));
            MediaStream.SampleRequested += OnMediaStreamSampleRequested;
            MediaStream.Starting += OnMediaStreamStarting;
            MediaStream.CanSeek = true;
            MediaStream.Duration = TimeSpan.FromSeconds(MediaDescriptors.Sum(a => a.Duration.TotalSeconds));
            MediaStream.MaxSupportedPlaybackRate = 10;

            MediaPlayer.PlaybackSession.Position = new TimeSpan();
            MediaPlayer.AutoPlay = true;
            MediaPlayer.Source = MediaSource.CreateFromMediaStreamSource(MediaStream);
        }

        public void TryCloseOpenedFile()
        {
            if (MediaPlayer != null)
                MediaPlayer.Source = null;
            Stream?.Dispose();
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
            if (index >= MediaDescriptors.Count)
            {
                index = MediaDescriptors.Count - 1;
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
            var mediaDescriptor = MediaDescriptors[(Int32)CurrentSampleIndex];
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
            CurrentSampleIndex++;

            deferral.Complete();
        }

        public void EnabledNormalRate() => MediaPlayer.PlaybackSession.PlaybackRate = 1.0;

        public void EnabledMaxRate() => MediaPlayer.PlaybackSession.PlaybackRate = 3.0;

        public void EnabledFastForward()
        {
        }

        public void DisableFastForward()
        {
        }

        public void Dispose() => Stream.Dispose();
    }
}
