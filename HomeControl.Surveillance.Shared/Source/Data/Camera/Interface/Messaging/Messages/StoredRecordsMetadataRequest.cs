namespace HomeControl.Surveillance.Data.Camera
{
    public class StoredRecordsMetadataRequest: IMessage
    {
        public MessageId Type { get; } = MessageId.StoredRecordsMetadataRequest;
    }
}
