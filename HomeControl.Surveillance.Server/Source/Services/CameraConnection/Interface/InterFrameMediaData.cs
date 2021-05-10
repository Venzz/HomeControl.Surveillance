using System;

namespace HomeControl.Surveillance.Server.Services
{
    public class InterFrameMediaData: IMediaData
    {
        public MediaDataType MediaDataType => MediaDataType.InterFrame;
        public Byte[] Data { get; }
        public DateTime Timestamp { get; }
        public TimeSpan Duration { get; }

        public InterFrameMediaData(Byte[] data, DateTime timestamp, TimeSpan duration)
        {
            Data = data;
            Timestamp = timestamp;
            Duration = duration;
        }
    }
}
