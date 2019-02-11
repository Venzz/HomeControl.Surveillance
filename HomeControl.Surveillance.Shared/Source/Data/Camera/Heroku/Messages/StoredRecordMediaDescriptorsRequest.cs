using System;

namespace HomeControl.Surveillance.Data.Camera.Heroku
{
    public class StoredRecordMediaDescriptorsRequest: IMessage
    {
        public MessageId Type { get; } = MessageId.StoredRecordMediaDescriptorsRequest;
        public String StoredRecordId { get; }

        public StoredRecordMediaDescriptorsRequest(String storedRecordId)
        {
            StoredRecordId = storedRecordId;
        }
    }
}
