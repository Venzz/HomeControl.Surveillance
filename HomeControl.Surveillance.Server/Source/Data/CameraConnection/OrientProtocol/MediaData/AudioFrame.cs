using System;

namespace HomeControl.Surveillance.Server.Data.OrientProtocol
{
    public class AudioFrame
    {
        public Byte[] Data { get; }

        public AudioFrame(Byte[] data)
        {
            Data = data;
        }
    }
}
