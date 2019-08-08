using System;
using System.IO;

namespace HomeControl.Surveillance.Server.Data.OrientProtocol
{
    public class Message
    {
        public UInt32 SessionId { get; private set; }
        public UInt32 SequenceNumber { get; private set; }
        public String Header { get; private set; }



        protected Message() { }

        protected Message(UInt32 sessionId) { SessionId = sessionId; }

        public static Message Create(Byte[] data)
        {
            var message = new Message();
            using (var reader = new BinaryReader(new MemoryStream(data)))
            {
                var marker = reader.ReadInt32();
                var sessionId = reader.ReadUInt32();
                var sequenceNumber = reader.ReadUInt32();
                reader.ReadBytes(2);
                var operationCode = reader.ReadUInt16();
                var messageDataSize = reader.ReadInt32();
                var messageData = reader.ReadBytes(messageDataSize);

                switch (operationCode)
                {
                    case (UInt16)Operation.AuthorizationResponse:
                        return new AuthorizationResponseMessage(sessionId, messageData) { Header = data.ToTestView(0, 20), SequenceNumber = sequenceNumber };
                    case (UInt16)Operation.MediaDataResponse:
                        return new MediaDataResponseMessage(sessionId, messageData) { Header = data.ToTestView(0, 20), SequenceNumber = sequenceNumber };
                    case (UInt16)Operation.OpMonitorClaimResponse:
                        return new OpMonitorClaimResponseMessage(sessionId, messageData) { Header = data.ToTestView(0, 20), SequenceNumber = sequenceNumber };
                    default:
                        return new UnknownResponseMessage(sessionId, data) { Header = data.ToTestView(0, 20), SequenceNumber = sequenceNumber };
                }
            }
        }

        public virtual Byte[] Serialize() => new Byte[0];

        public enum Operation
        {
            AudioFrame = 0x01FA,
            InterFrame = 0x01FC,
            PredictionFrame = 0x01FD,
            AuthorizationRequest = 0x03E8,
            AuthorizationResponse = 0x03E9,
            OpMonitorStartRequest = 0x0582,
            MediaDataResponse = 0x0584,
            OpMonitorClaimRequest = 0x0585,
            OpMonitorClaimResponse = 0x0586,
            OpPtzControl = 0x0578,
        }

        public enum ZoomType
        {
            ZoomTile,
            ZoomWide
        }
    }
}
