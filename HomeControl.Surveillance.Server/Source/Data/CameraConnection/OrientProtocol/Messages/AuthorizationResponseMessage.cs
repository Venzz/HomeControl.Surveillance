using System;

namespace HomeControl.Surveillance.Server.Data.OrientProtocol
{
    public class AuthorizationResponseMessage: Message
    {
        public AuthorizationResponseMessage(UInt32 sessionId, Byte[] data): base(sessionId)
        {

        }
    }
}
