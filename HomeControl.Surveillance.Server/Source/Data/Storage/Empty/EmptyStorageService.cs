using System;
using Windows.Foundation;

namespace HomeControl.Surveillance.Server.Data.Empty
{
    public class EmptyStorageService: IStorageService
    {
        public event TypedEventHandler<IStorageService, (String CustomText, Exception Exception)> ExceptionReceived = delegate { };

        public void Store(Byte[] data)
        {
        }
    }
}
