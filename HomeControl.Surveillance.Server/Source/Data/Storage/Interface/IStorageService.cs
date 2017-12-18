using System;
using Windows.Foundation;

namespace HomeControl.Surveillance.Server.Data
{
    public interface IStorageService
    {
        event TypedEventHandler<IStorageService, (String CustomText, Exception Exception)> ExceptionReceived;

        void Store(Byte[] data);
    }
}
