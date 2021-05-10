using System;

namespace HomeControl.Surveillance.Server.Services.OrientProtocol
{
    public class AuthorizationResponseMessage: Message
    {
        public AuthorizationResponseMessage(UInt32 sessionId, Byte[] data): base(sessionId) { }
    }
}
