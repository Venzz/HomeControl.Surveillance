using System;

namespace HomeControl.Surveillance.Server.Data.Empty
{
    public class EmptyStorageService: IStorageService
    {
        public void Store(Byte[] data)
        {
        }
    }
}
