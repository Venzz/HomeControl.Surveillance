using System;

namespace HomeControl.Surveillance.Server.Data.OrientProtocol
{
    public class VideoDataResponseMessage: Message
    {
        public Byte[] Data { get; }

        public VideoDataResponseMessage(UInt32 sessionId, Byte[] data): base(sessionId)
        {
            Data = data;
        }
    }
}
