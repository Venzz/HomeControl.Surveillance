using System;

namespace HomeControl.Surveillance.Player.Model
{
    public class MediaData
    {
        public MediaDataType Type { get; }
        public Byte[] Data { get; }
        public DateTime Timestamp { get; }
        public TimeSpan Duration { get; }

        public MediaData(MediaDataType mediaType, Byte[] data, DateTime timestamp, TimeSpan duration)
        {
            Type = mediaType;
            Data = data;
            Timestamp = timestamp;
            Duration = duration;
        }

        public override String ToString()
        {
            return $"{Timestamp:mm:ss.fff} - {Duration}, {Type}, {Data.Length}b";
        }
    }
}
