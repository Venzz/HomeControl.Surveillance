using System;

namespace HomeControl.Surveillance.Server.Data
{
    public class InterFrameMediaData: IMediaData
    {
        public MediaDataType MediaDataType => MediaDataType.InterFrame;
        public Byte[] Data { get; }

        public InterFrameMediaData(Byte[] data)
        {
            Data = data;
        }
    }
}
