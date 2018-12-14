using System;

namespace HomeControl.Surveillance.Server.Data.OrientProtocol
{
    public class MediaDataResponseMessage: Message
    {
        public Byte[] Data { get; }

        public MediaDataResponseMessage(UInt32 sessionId, Byte[] data): base(sessionId)
        {
            Data = data;
        }
    }
}
