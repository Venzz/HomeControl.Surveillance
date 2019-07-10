using System;

namespace HomeControl.Surveillance.Data.Camera
{
    public class FileDataResponse: IMessage
    {
        public MessageId Type { get; } = MessageId.FileDataResponse;
        public Byte[] Data { get; }

        public FileDataResponse(Byte[] data)
        {
            Data = data;
        }
    }
}
