using System;

namespace HomeControl.Surveillance.Data.Camera.Heroku
{
    public class LiveMediaDataResponse: IMessage
    {
        public MessageId Type { get; } = MessageId.LiveMediaData;
        public MediaDataType MediaType { get; }
        public Byte[] Data { get; }

        public LiveMediaDataResponse(MediaDataType mediaType, Byte[] data)
        {
            MediaType = mediaType;
            Data = data;
        }
    }
}
