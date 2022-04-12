using System;
using System.Collections.Generic;
using Windows.Foundation;

namespace HomeControl.Surveillance.Server.Services
{
    public class EmptyStorageService: IStorageService
    {
        public event TypedEventHandler<IStorageService, (String, String)> Log = delegate { };
        public event TypedEventHandler<IStorageService, (String, String, Exception)> Exception = delegate { };

        public IReadOnlyCollection<String> GetStoredRecords() => new List<String>();
        public IReadOnlyCollection<StoredRecordFile.MediaDataDescriptor> GetStoredRecordMediaDescriptors(String id) => new List<StoredRecordFile.MediaDataDescriptor>();
        public Byte[] GetStoredRecordMediaData(String id, UInt32 offset) => new Byte[0];
        public void Store(IMediaData mediaData) { }
    }
}
