using HomeControl.Surveillance.Data.Storage;
using System;
using System.Collections.Generic;
using Windows.Foundation;

namespace HomeControl.Surveillance.Server.Data
{
    public interface IStorageService
    {
        event TypedEventHandler<IStorageService, (String CustomText, Exception Exception)> ExceptionReceived;

        void Store(IMediaData mediaData);
        IReadOnlyCollection<String> GetStoredRecords();
        IReadOnlyCollection<StoredRecordFile.MediaDataDescriptor> GetStoredRecordMediaDescriptors(String id);
        Byte[] GetStoredRecordMediaData(String id, UInt32 offset);
    }
}
