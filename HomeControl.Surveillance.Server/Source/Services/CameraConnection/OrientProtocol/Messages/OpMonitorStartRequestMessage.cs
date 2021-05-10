using System;
using System.IO;
using System.Text;

namespace HomeControl.Surveillance.Server.Services.OrientProtocol
{
    public class OpMonitorStartRequestMessage: Message
    {
        public OpMonitorStartRequestMessage(UInt32 sessionId): base(sessionId) { }

        public override Byte[] Serialize()
        {
            using (var stream = new BinaryWriter(new MemoryStream()))
            {
                stream.Write((Byte)0xFF);
                stream.Write((Byte)0x00);
                stream.Write((Byte)0x00);
                stream.Write((Byte)0x00);
                stream.Write(SessionId);
                stream.Write((Byte)0x00);
                stream.Write((Byte)0x00);
                stream.Write((Byte)0x00);
                stream.Write((Byte)0x00);
                stream.Write((Byte)0x00);
                stream.Write((Byte)0x00);
                stream.Write((Byte)((Int32)Operation.OpMonitorStartRequest % 256));
                stream.Write((Byte)((Int32)Operation.OpMonitorStartRequest / 256));

                var message = $"{{ \"Name\" : \"OPMonitor\", \"OPMonitor\" : {{ \"Action\" : \"Start\", \"Parameter\" : {{ \"Channel\" : 0, \"CombinMode\" : \"CONNECT_ALL\", \"StreamType\" : \"Main\", \"TransMode\" : \"TCP\" }} }}, \"SessionID\" : \"0x{SessionId:x}\" }}\n";
                var messageData = Encoding.ASCII.GetBytes(message);

                stream.Write((Byte)(messageData.Length % 256));
                stream.Write((Byte)(messageData.Length / 256));
                stream.Write((Byte)0x00);
                stream.Write((Byte)0x00);
                stream.Write(messageData);

                var data = new Byte[stream.BaseStream.Length];
                stream.BaseStream.Position = 0;
                stream.BaseStream.Read(data, 0, data.Length);
                return data;
            }
        }
    }
}
