using System;
using System.Collections.Generic;
using Windows.Foundation;

namespace HomeControl.Surveillance.Server.Data.Empty
{
    public class EmptyStorageService: IStorageService
    {
        public event TypedEventHandler<IStorageService, (String CustomText, Exception Exception)> ExceptionReceived = delegate { };

        public IReadOnlyCollection<String> GetStoredRecords() => new List<String>();
        public void Store(IMediaData mediaData) { }
    }
}
