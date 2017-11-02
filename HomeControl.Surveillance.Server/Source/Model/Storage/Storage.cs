using HomeControl.Surveillance.Server.Data;
using System;

namespace HomeControl.Surveillance.Server.Model
{
    public class Storage
    {
        private IStorageService Service;

        public Storage(IStorageService storageService) { Service = storageService; }

        public void Store(Byte[] data) => Service.Store(data);
    }
}
