using System;

namespace HomeControl.Surveillance.Server.Data
{
    public class AudioMediaData: IMediaData
    {
        public MediaDataType MediaDataType => MediaDataType.AudioFrame;
        public Byte[] Data { get; }

        public AudioMediaData(Byte[] data)
        {
            Data = data;
        }
    }
}
