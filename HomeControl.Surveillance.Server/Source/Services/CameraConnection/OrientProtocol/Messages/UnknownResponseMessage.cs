using System;

namespace HomeControl.Surveillance.Server.Services.OrientProtocol
{
    public class UnknownResponseMessage: Message
    {
        public String Data { get; }

        public UnknownResponseMessage(UInt32 sessionId, Byte[] data): base(sessionId)
        {
            Data = data.ToHexView();
        }
    }
}
