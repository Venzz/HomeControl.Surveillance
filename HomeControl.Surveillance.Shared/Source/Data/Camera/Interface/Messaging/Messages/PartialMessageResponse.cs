using System;

namespace HomeControl.Surveillance.Data.Camera
{
    public class PartialMessageResponse: IMessage
    {
        public MessageId Type { get; } = MessageId.PartialMessageResponse;
        public Byte[] Data { get; }
        public Boolean Final { get; }

        public PartialMessageResponse(Boolean final, Byte[] data)
        {
            Final = final;
            Data = data;
        }
    }
}
