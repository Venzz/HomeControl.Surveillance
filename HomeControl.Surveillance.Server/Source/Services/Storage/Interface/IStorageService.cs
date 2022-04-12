using System;
using System.Collections.Generic;
using Windows.Foundation;

namespace HomeControl.Surveillance.Server.Services
{
    public interface IStorageService
    {
        event TypedEventHandler<IStorageService, (String Source, String Message)> Log;
        event TypedEventHandler<IStorageService, (String Source, String Details, Exception Exception)> Exception;

        void Store(IMediaData mediaData);
        IReadOnlyCollection<String> GetStoredRecords();
        IReadOnlyCollection<StoredRecordFile.MediaDataDescriptor> GetStoredRecordMediaDescriptors(String id);
        Byte[] GetStoredRecordMediaData(String id, UInt32 offset);
    }
}
