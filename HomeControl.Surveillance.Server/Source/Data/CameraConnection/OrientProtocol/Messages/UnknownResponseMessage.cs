using System;
using Venz;

namespace HomeControl.Surveillance.Server.Data.OrientProtocol
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
