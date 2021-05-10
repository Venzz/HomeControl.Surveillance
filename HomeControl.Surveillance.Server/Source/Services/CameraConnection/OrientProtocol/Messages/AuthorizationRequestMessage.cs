using System;
using System.IO;
using System.Text;

namespace HomeControl.Surveillance.Server.Services.OrientProtocol
{
    public class AuthorizationRequestMessage: Message
    {
        public AuthorizationRequestMessage(): base(sessionId: 0) { }

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
                stream.Write((Byte)((Int32)Operation.AuthorizationRequest % 256));
                stream.Write((Byte)((Int32)Operation.AuthorizationRequest / 256));

                var message = $"{{ \"EncryptType\" : \"MD5\", \"LoginType\" : \"DVRIP-Web\", \"PassWord\" : \"6QNMIQGe\", \"UserName\" : \"admin\" }}\n";
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
