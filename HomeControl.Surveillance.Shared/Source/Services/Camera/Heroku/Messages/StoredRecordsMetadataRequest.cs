namespace HomeControl.Surveillance.Services.Heroku
{
    public class StoredRecordsMetadataRequest: IMessage
    {
        public MessageId Type { get; } = MessageId.StoredRecordsMetadataRequest;
    }
}
