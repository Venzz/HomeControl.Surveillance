using HomeControl.Surveillance.Server.Data;
using System;

namespace HomeControl.Surveillance.Server.Model
{
    public class Storage
    {
        private IStorageService Service;

        public Storage(IStorageService storageService) { Service = storageService; }

        public void Store(Byte[] data)
        {
            try
            {
                Service.Store(data);
            }
            catch (Exception exception)
            {
                App.Diagnostics.Debug.Log($"{nameof(Storage)}.{nameof(Store)}", exception);
            }
        }
    }
}
