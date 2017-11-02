using System;

namespace HomeControl.Surveillance.Server.Data.OrientProtocol
{
    public class OpMonitorClaimResponseMessage: Message
    {
        public OpMonitorClaimResponseMessage(UInt32 sessionId, Byte[] data): base(sessionId)
        {
        }
    }
}
