using System;
using System.Threading.Tasks;
using Windows.Foundation;

namespace HomeControl.Surveillance.Server.Services
{
    public interface ICameraConnection
    {
        Boolean IsZoomingSupported { get; }

        event TypedEventHandler<ICameraConnection, IMediaData> MediaReceived;
        event TypedEventHandler<ICameraConnection, (String Source, String Message)> Log;
        event TypedEventHandler<ICameraConnection, (String Source, String Message, String DetailedMessage)> DetailedLog;
        event TypedEventHandler<ICameraConnection, (String Source, String Details, Exception Exception)> Exception;

        Task StartZoomingInAsync();
        Task StartZoomingOutAsync();
        Task StopZoomingAsync();
    }
}
