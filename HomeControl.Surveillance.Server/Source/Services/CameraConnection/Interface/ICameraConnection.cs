using System;
using System.Threading.Tasks;
using Windows.Foundation;

namespace HomeControl.Surveillance.Server.Services
{
    public interface ICameraConnection
    {
        Boolean IsZoomingSupported { get; }

        event TypedEventHandler<ICameraConnection, IMediaData> MediaReceived;
        event TypedEventHandler<ICameraConnection, (String CustomText, Exception Exception)> ExceptionReceived;
        event TypedEventHandler<ICameraConnection, (String CustomText, String Parameter)> LogReceived;

        Task StartZoomingInAsync();
        Task StartZoomingOutAsync();
        Task StopZoomingAsync();
    }
}
