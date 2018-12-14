using System;

namespace HomeControl.Surveillance.Server.Data
{
    public interface IMediaData
    {
        MediaDataType MediaDataType { get; }
        Byte[] Data { get; }
    }

    public enum MediaDataType
    {
        AudioFrame,
        PredictionFrame,
        InterFrame
    }
}
