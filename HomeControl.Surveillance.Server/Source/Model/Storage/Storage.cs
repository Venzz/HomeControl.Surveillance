using HomeControl.Surveillance.Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;

namespace HomeControl.Surveillance.Server.Model
{
    public class Storage
    {
        private IStorageService Service;

        public event TypedEventHandler<Storage, (String Source, String Message)> Log = delegate { };
        public event TypedEventHandler<Storage, (String Source, String Details, Exception Exception)> Exception = delegate { };



        public Storage(IStorageService storageService)
        {
            Service = storageService;
            Service.Log += (sender, args) => Log(this, args);
            Service.Exception += (sender, args) => Exception(this, args);
        }

        public void Store(IMediaData data)
        {
            try
            {
                Service.Store(data);
            }
            catch (Exception exception)
            {
                Exception(this, ($"{nameof(Storage)}.{nameof(Store)}", null, exception));
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
    }
}
