namespace HomeControl.Surveillance.Data.Camera.Heroku
{
    public class StoredRecordsMetadataRequest: IMessage
    {
        public MessageId Type { get; } = MessageId.StoredRecordsMetadataRequest;
    }
}
