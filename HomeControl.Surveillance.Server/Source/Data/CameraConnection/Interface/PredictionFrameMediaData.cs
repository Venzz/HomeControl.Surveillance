using System;

namespace HomeControl.Surveillance.Server.Data
{
    public class PredictionFrameMediaData: IMediaData
    {
        public MediaDataType MediaDataType => MediaDataType.PredictionFrame;
        public Byte[] Data { get; }

        public PredictionFrameMediaData(Byte[] data)
        {
            Data = data;
        }
    }
}
