using System;

namespace HomeControl.Surveillance.Server.Data
{
    public interface IMediaData
    {
        MediaDataType MediaDataType { get; }
        Byte[] Data { get; }
        DateTime Timestamp { get; }
        TimeSpan Duration { get; }
    }
}
