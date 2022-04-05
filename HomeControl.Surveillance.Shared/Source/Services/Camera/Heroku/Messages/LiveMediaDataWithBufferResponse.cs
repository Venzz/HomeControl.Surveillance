using System.Collections.Generic;

namespace HomeControl.Surveillance.Services.Heroku
{
    public class LiveMediaDataWithBufferResponse: IMessage
    {
        public MessageId Type { get; } = MessageId.LiveMediaDataWithBuffer;
        public LiveMediaDataResponse MediaData { get; }
        public IReadOnlyCollection<LiveMediaDataResponse> MediaDataBuffer { get; }

        public LiveMediaDataWithBufferResponse(LiveMediaDataResponse mediaData, IReadOnlyCollection<LiveMediaDataResponse> mediaDataBuffer)
        {
            MediaData = mediaData;
            MediaDataBuffer = mediaDataBuffer;
        }
    }
}
