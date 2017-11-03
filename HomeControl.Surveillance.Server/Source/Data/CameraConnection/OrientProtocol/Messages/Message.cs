using System;
using System.IO;

namespace HomeControl.Surveillance.Server.Data.OrientProtocol
{
    public class Message
    {
        public UInt32 SessionId { get; private set; }



        protected Message() { }

        protected Message(UInt32 sessionId) { SessionId = sessionId; }

        public static Message Create(Byte[] data)
        {
            var message = new Message();
            using (var reader = new BinaryReader(new MemoryStream(data)))
            {
                var marker = reader.ReadInt32();
                var sessionId = reader.ReadUInt32();
                reader.ReadBytes(6);
                var operationCode = reader.ReadUInt16();
                var messageDataSize = reader.ReadInt32();
                var messageData = reader.ReadBytes(messageDataSize);

                switch (operationCode)
                {
                    case 0x03E9:
                        return new AuthorizationResponseMessage(sessionId, messageData);
                    case 0x0584:
                        return new VideoDataResponseMessage(sessionId, messageData);
                    case 0x0586:
                        return new OpMonitorClaimResponseMessage(sessionId, messageData);
                    default:
                        return new UnknownResponseMessage(sessionId, data);
                }
            }
        }

        public virtual Byte[] Serialize() => new Byte[0];

        public enum Operation
        {
            AuthorizationRequest = 0x03E8,
            AuthorizationResponse = 0x03E9,
            OpMonitorStartRequest = 0x0582,
            VideoDataResponse = 0x0584,
            OpMonitorClaimRequest = 0x0585,
            OpMonitorClaimResponse = 0x0586,
        }
    }
}
