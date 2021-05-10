namespace HomeControl.Surveillance.Services
{
    public interface IMessage
    {
        MessageId Type { get; }
    }
}
