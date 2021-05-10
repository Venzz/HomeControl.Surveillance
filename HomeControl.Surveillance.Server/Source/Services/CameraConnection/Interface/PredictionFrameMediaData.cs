using System;

namespace HomeControl.Surveillance.Server.Services
{
    public class PredictionFrameMediaData: IMediaData
    {
        public MediaDataType MediaDataType => MediaDataType.PredictionFrame;
        public Byte[] Data { get; }
        public DateTime Timestamp { get; }
        public TimeSpan Duration { get; }

        public PredictionFrameMediaData(Byte[] data, DateTime timestamp, TimeSpan duration)
        {
            Data = data;
            Timestamp = timestamp;
            Duration = duration;
        }
    }
}
