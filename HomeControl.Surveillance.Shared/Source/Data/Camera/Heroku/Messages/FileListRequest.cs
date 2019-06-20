namespace HomeControl.Surveillance.Data.Camera.Heroku
{
    public class FileListRequest: IMessage
    {
        public MessageId Type { get; } = MessageId.FileListRequest;
    }
}
