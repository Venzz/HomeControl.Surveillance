using HomeControl.Surveillance.Data.Storage;
using System;
using System.Collections.Generic;
using Windows.Foundation;

namespace HomeControl.Surveillance.Server.Data.Empty
{
    public class EmptyStorageService: IStorageService
    {
        public event TypedEventHandler<IStorageService, (String CustomText, String Parameter)> LogReceived = delegate { };
        public event TypedEventHandler<IStorageService, (String CustomText, Exception Exception)> ExceptionReceived = delegate { };

        public IReadOnlyCollection<String> GetStoredRecords() => new List<String>();
        public IReadOnlyCollection<StoredRecordFile.MediaDataDescriptor> GetStoredRecordMediaDescriptors(String id) => new List<StoredRecordFile.MediaDataDescriptor>();
        public Byte[] GetStoredRecordMediaData(String id, UInt32 offset) => new Byte[0];
        public void Store(IMediaData mediaData) { }
    }
}
