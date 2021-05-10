using System;

namespace HomeControl.Surveillance.Services.Heroku
{
    public class StoredRecordMediaDataResponse: IMessage
    {
        public MessageId Type { get; } = MessageId.StoredRecordMediaDataResponse;
        public Byte[] Data { get; }

        public StoredRecordMediaDataResponse(Byte[] data)
        {
            Data = data;
        }
    }
}
