using System;

namespace HomeControl.Surveillance.Server.Data
{
    public class AudioMediaData: IMediaData
    {
        public MediaDataType MediaDataType => MediaDataType.AudioFrame;
        public Byte[] Data { get; }
        public DateTime Timestamp { get; }
        public TimeSpan Duration { get; }

        public AudioMediaData(Byte[] data, DateTime timestamp, TimeSpan duration)
        {
            Data = data;
            Timestamp = timestamp;
            Duration = duration;
        }
    }
}
