namespace HomeControl.Surveillance.Data.Camera
{
    public class FileListRequest: IMessage
    {
        public MessageId Type { get; } = MessageId.FileListRequest;
    }
}
