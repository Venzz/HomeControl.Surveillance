using HomeControl.Surveillance.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;

namespace HomeControl.Surveillance.Player.Model
{
    public class Camera
    {
        private IConsumerCameraService ConsumerService;
        private Boolean MediaDataBufferReceived;
        private List<MediaData> MediaDataBuffer = new List<MediaData>();

        public TimeSpan SampleDuration { get; }
        public Boolean SupportsCommands { get; }
        public String Id { get; }
        public String Title { get; }

        public event TypedEventHandler<Camera, MediaData> DataReceived = delegate { };



        public Camera(IConsumerCameraService consumerService, Boolean supportsCommands, TimeSpan sampleDuration, String title)
        {
            SampleDuration = sampleDuration;
            SupportsCommands = supportsCommands;
            Id = title.ToLower();
            Title = title;
            ConsumerService = consumerService;
            ConsumerService.MediaDataBufferReceived += OnMediaDataBufferReceived;
            ConsumerService.MediaDataReceived += OnMediaDataReceived;
        }

        public Task StartZoomingInAsync() => !SupportsCommands ? Task.CompletedTask : ConsumerService.PerformAsync(Command.StartZoomingIn);

        public Task StartZoomingOutAsync() => !SupportsCommands ? Task.CompletedTask : ConsumerService.PerformAsync(Command.StartZoomingOut);

        public Task StopZoomingAsync() => !SupportsCommands ? Task.CompletedTask : ConsumerService.PerformAsync(Command.StopZooming);

        public async Task<IReadOnlyCollection<StoredRecord>> GetStoredRecordsMetadataAsync()
        {
            var storedRecordsMetadata = await ConsumerService.GetStoredRecordsMetadataAsync().ConfigureAwait(false);
            return storedRecordsMetadata.Select(a => new StoredRecord() { Id = a.Id, Date = a.Date }).ToList();
        }

        public async Task<IReadOnlyCollection<StoredRecordFile.MediaDataDescriptor>> GetMediaDataDescriptorsAsync(String id)
        {
            var mediaDataDescriptors = await ConsumerService.GetMediaDataDescriptorsAsync(id).ConfigureAwait(false);
            return mediaDataDescriptors;
        }

        public async Task<Byte[]> GetMediaDataAsync(String id, UInt32 offset)
        {
            var mediaData = await ConsumerService.GetMediaDataAsync(id, offset).ConfigureAwait(false);
            return mediaData;
        }

        public IReadOnlyCollection<MediaData> GetMediaDataBuffer()
        {
            lock (this)
                return MediaDataBuffer.ToList();
        }

        private void OnMediaDataBufferReceived(IConsumerCameraService sender, IReadOnlyCollection<(MediaDataType MediaType, Byte[] Data, DateTime Timestamp, TimeSpan Duration)> args)
        {
            lock (this)
            {
                foreach (var bufferItem in args)
                {
                    var mediaData = new MediaData(bufferItem.MediaType, bufferItem.Data, bufferItem.Timestamp, bufferItem.Duration);
                    if (mediaData.Type == MediaDataType.InterFrame || mediaData.Type == MediaDataType.PredictionFrame)
                    {
                        MediaDataBufferReceived = true;
                        if (mediaData.Type == MediaDataType.InterFrame)
                            MediaDataBuffer.Clear();
                        MediaDataBuffer.Add(mediaData);
                    }
                }
                foreach (var mediaData in MediaDataBuffer)
                {
                    DataReceived(this, mediaData);
                }
            }
        }

        private void OnMediaDataReceived(IConsumerCameraService sender, (MediaDataType MediaType, Byte[] Data, DateTime Timestamp, TimeSpan Duration) args)
        {
            var mediaData = new MediaData(args.MediaType, args.Data, args.Timestamp, args.Duration);
            lock (this)
            {
                if ((mediaData.Type == MediaDataType.InterFrame || mediaData.Type == MediaDataType.PredictionFrame) && MediaDataBufferReceived)
                {
                    if (mediaData.Type == MediaDataType.InterFrame)
                        MediaDataBuffer.Clear();
                    MediaDataBuffer.Add(mediaData);
                }
                DataReceived(this, mediaData);
            }
        }
    }
}
