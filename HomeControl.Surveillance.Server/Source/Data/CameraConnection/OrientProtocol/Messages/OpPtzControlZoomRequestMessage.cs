using System;
using System.IO;
using System.Text;

namespace HomeControl.Surveillance.Server.Data.OrientProtocol
{
    public class OpPtzControlZoomRequestMessage: Message
    {
        private Int32 Preset;
        private ZoomType Zoom;

        public OpPtzControlZoomRequestMessage(UInt32 sessionId, Int32 preset, ZoomType zoom) : base(sessionId)
        {
            Preset = preset;
            Zoom = zoom;
        }

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
                stream.Write((Byte)((Int32)Operation.OpPtzControl % 256));
                stream.Write((Byte)((Int32)Operation.OpPtzControl / 256));

                var message = $"{{ \"Name\" : \"OPPTZControl\", \"OPPTZControl\" : {{ \"Command\": \"{Zoom}\", \"Parameter\" : {{ \"AUX\" : {{ \"Number\" : 0, \"Status\" : \"On\" }}, \"Channel\" : 0, \"MenuOpts\" : \"Enter\", \"POINT\" : {{ \"bottom\" : 0, \"left\" : 0, \"right\" : 0, \"top\" : 0 }}, \"Pattern\" : \"SetBegin\", \"Preset\" : {Preset}, \"Step\" : 1, \"Tour\" : 0 }} }}, \"SessionID\" : \"0x{SessionId:x}\" }}\n";
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
