using System;

namespace HomeControl.Surveillance.Services.Heroku
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
