using System;

namespace HomeControl.Surveillance.Data.Camera
{
    public class FileDataRequest: IMessage
    {
        public MessageId Type { get; } = MessageId.FileDataRequest;
        public String FileId { get; }
        public UInt32 Offset { get; }
        public UInt32 Length { get; }

        public FileDataRequest(String fileId, UInt32 offset, UInt32 length)
        {
            FileId = fileId;
            Offset = offset;
            Length = length;
        }
    }
}
