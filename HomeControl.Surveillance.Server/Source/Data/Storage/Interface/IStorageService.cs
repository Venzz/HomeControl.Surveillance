using System;

namespace HomeControl.Surveillance.Server.Data
{
    public interface IStorageService
    {
        void Store(Byte[] data);
    }
}
