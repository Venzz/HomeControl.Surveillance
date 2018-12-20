using System;

namespace HomeControl.Surveillance.Data.Camera.Heroku
{
    public class LiveMediaDataResponse: IMessage
    {
        public MessageId Type { get; } = MessageId.LiveMediaData;
        public MediaDataType MediaType { get; }
        public Byte[] Data { get; }
        public DateTime Timestamp { get; }
        public TimeSpan Duration { get; }

        public LiveMediaDataResponse(MediaDataType mediaType, Byte[] data, DateTime timestamp, TimeSpan duration)
        {
            MediaType = mediaType;
            Data = data;
            Timestamp = timestamp;
            Duration = duration;
        }
    }
}
