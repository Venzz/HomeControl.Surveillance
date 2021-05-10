using System;

namespace HomeControl.Surveillance.Services.Heroku
{
    public class StoredRecordMediaDataRequest: IMessage
    {
        public MessageId Type { get; } = MessageId.StoredRecordMediaDataRequest;
        public String StoredRecordId { get; }
        public UInt32 Offset { get; }

        public StoredRecordMediaDataRequest(String storedRecordId, UInt32 offset)
        {
            StoredRecordId = storedRecordId;
            Offset = offset;
        }
    }
}
