using System;

namespace HomeControl.Surveillance.Server.Services.OrientProtocol
{
    public class OpMonitorClaimResponseMessage: Message
    {
        public OpMonitorClaimResponseMessage(UInt32 sessionId, Byte[] data): base(sessionId) { }
    }
}
