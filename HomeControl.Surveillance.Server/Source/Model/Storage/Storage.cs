using HomeControl.Surveillance.Server.Data;
using System;
using Windows.Foundation;

namespace HomeControl.Surveillance.Server.Model
{
    public class Storage
    {
        private IStorageService Service;

        public event TypedEventHandler<Storage, (String CustomText, Exception Exception)> ExceptionReceived = delegate { };



        public Storage(IStorageService storageService)
        {
            Service = storageService;
            Service.ExceptionReceived += (sender, args) => ExceptionReceived(this, args);
        }

        public void Store(Byte[] data)
        {
            try
            {
                Service.Store(data);
            }
            catch (Exception exception)
            {
                ExceptionReceived(this, ($"{nameof(Storage)}.{nameof(Store)}", exception));
            }
        }
    }
}
