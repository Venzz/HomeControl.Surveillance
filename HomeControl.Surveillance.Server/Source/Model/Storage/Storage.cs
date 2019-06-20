using HomeControl.Surveillance.Data.Storage;
using HomeControl.Surveillance.Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;

namespace HomeControl.Surveillance.Server.Model
{
    public class Storage
    {
        private IStorageService Service;

        public event TypedEventHandler<Storage, (String CustomText, String Parameter)> LogReceived = delegate { };
        public event TypedEventHandler<Storage, (String CustomText, Exception Exception)> ExceptionReceived = delegate { };



        public Storage(IStorageService storageService)
        {
            Service = storageService;
            Service.LogReceived += (sender, args) => LogReceived(this, (nameof(Storage), args.Parameter));
            Service.ExceptionReceived += (sender, args) => ExceptionReceived(this, args);
        }

        public void Store(IMediaData data)
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

        public IReadOnlyCollection<(String Id, DateTime Date)> GetStoredRecordsMetadata()
        {
            var storedRecordsMetadata = new List<(String Id, DateTime Date)>();
            var storedRecords = Service.GetStoredRecords();
            foreach (var storedRecord in storedRecords)
            {
                var date = DateTime.Parse(storedRecord.Substring(0, 10)).AddHours(Int32.Parse(storedRecord.Substring(11, 2)));
                storedRecordsMetadata.Add((storedRecord, date));
            }
            return storedRecordsMetadata.OrderByDescending(a => a.Date).ToList();
        }

        public IReadOnlyCollection<StoredRecordFile.MediaDataDescriptor> GetStoredRecordMediaDescriptors(String id)
        {
            return Service.GetStoredRecordMediaDescriptors(id);
        }

        public Byte[] GetStoredRecordMediaData(String id, UInt32 offset)
        {
            return Service.GetStoredRecordMediaData(id, offset);
        }

        public IReadOnlyCollection<String> GetFileList()
        {
            return Service.GetFileList();
        }

        public Byte[] GetFileData(String id, UInt32 offset, UInt32 length)
        {
            return Service.GetFileData(id, offset, length);
        }
    }
}
