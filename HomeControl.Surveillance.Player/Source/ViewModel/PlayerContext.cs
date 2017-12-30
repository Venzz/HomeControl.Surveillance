using System;
using System.Collections.Generic;
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
        private TimeSpan SampleDuration = TimeSpan.FromMilliseconds(39);
        private IList<CombinedSample> Samples = new List<CombinedSample>();
        private UInt32 CurrentSampleIndex;

        public MediaPlayer MediaPlayer { get; } = new MediaPlayer();
        public MediaStreamSource MediaStream { get; private set; }



        public PlayerContext() { }

        public async Task OpenAsync(StorageFile file)
        {
            Stream = await file.OpenReadAsync().AsTask().ConfigureAwait(false);
            var samples = new List<Sample>();
            using (var reader = new DataReader(Stream) { ByteOrder = ByteOrder.LittleEndian })
            {
                while (Stream.Position < Stream.Size)
                {
                    await reader.LoadAsync(4).AsTask().ConfigureAwait(false);
                    var size = reader.ReadUInt32();
                    var position = (UInt32)Stream.Position;
                    if (Stream.Position + size <= Stream.Size)
                        samples.Add(new Sample(position, size));
                    Stream.Seek(Stream.Position + size);
                }
                reader.DetachStream();
            }

            var index = 0;
            while (index < samples.Count)
            {
                var combinedSample = new CombinedSample();
                combinedSample.Add(samples[index++]);
                while ((index < samples.Count) && (samples[index].Size < 1000))
                    combinedSample.Add(samples[index++]);
                Samples.Add(combinedSample);
            }

            var duration = TimeSpan.FromSeconds(Samples.Count * SampleDuration.TotalSeconds);
            MediaStream = new MediaStreamSource(new VideoStreamDescriptor(VideoEncodingProperties.CreateH264()));
            MediaStream.SampleRequested += OnMediaStreamSampleRequested;
            MediaStream.Starting += OnMediaStreamStarting;
            MediaStream.CanSeek = true;
            MediaStream.Duration = duration;
            MediaPlayer.PlaybackSession.Position = new TimeSpan();
        }

        private void OnMediaStreamStarting(MediaStreamSource sender, MediaStreamSourceStartingEventArgs args)
        {
            CurrentSampleIndex = 0;
            if (!args.Request.StartPosition.HasValue)
                return;
            CurrentSampleIndex = (UInt32)(args.Request.StartPosition.Value.TotalMilliseconds / SampleDuration.TotalMilliseconds);
        }

        private async void OnMediaStreamSampleRequested(MediaStreamSource sender, MediaStreamSourceSampleRequestedEventArgs args)
        {
            if (CurrentSampleIndex >= Samples.Count)
                return;

            var deferal = args.Request.GetDeferral();
            var sample = Samples[(Int32)CurrentSampleIndex];
            var data = await sample.GetDataAsync(Stream).ConfigureAwait(false);

            args.Request.Sample = MediaStreamSample.CreateFromBuffer(data, TimeSpan.FromMilliseconds(CurrentSampleIndex++ * SampleDuration.TotalMilliseconds));
            args.Request.Sample.Duration = SampleDuration;
            deferal.Complete();
        }

        public void Dispose() => Stream.Dispose();



        private class Sample
        {
            public UInt32 Position { get; }
            public UInt32 Size { get; }

            public Sample(UInt32 position, UInt32 size)
            {
                Position = position;
                Size = size;
            }

            public override String ToString() => Size.ToString();
        }

        private class CombinedSample
        {
            private IList<Sample> Samples = new List<Sample>();

            public UInt32 Size { get; private set; }



            public CombinedSample() { }

            public void Add(Sample sample)
            {
                Samples.Add(sample);
                Size += sample.Size;
            }

            public async Task<IBuffer> GetDataAsync(IRandomAccessStream stream)
            {
                var data = new InMemoryRandomAccessStream();
                var buffer = new Windows.Storage.Streams.Buffer((UInt32)Samples.Sum(a => a.Size));
                foreach (var sample in Samples)
                {
                    stream.Seek(sample.Position);
                    var sampleData = await stream.ReadAsync(buffer, sample.Size, InputStreamOptions.None).AsTask().ConfigureAwait(false);
                    await data.WriteAsync(sampleData).AsTask().ConfigureAwait(false);
                }
                data.Seek(0);
                return await data.ReadAsync(buffer, (UInt32)data.Size, InputStreamOptions.None).AsTask().ConfigureAwait(false);
            }

            public override String ToString() => $"{Samples.Count} samples. {Size} bytes.";
        }
    }
}
