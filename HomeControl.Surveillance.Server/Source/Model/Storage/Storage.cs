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

        public event TypedEventHandler<Storage, (String CustomText, Exception Exception)> ExceptionReceived = delegate { };



        public Storage(IStorageService storageService)
        {
            Service = storageService;
            Service.ExceptionReceived += (sender, args) => ExceptionReceived(this, args);
        }

        public void Store(Byte[] data)
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

        public IReadOnlyCollection<DateTime> GetStoredRecordsMetadata()
        {
            var storedRecordsMetadata = new HashSet<DateTime>();
            var storedRecords = Service.GetStoredRecords();
            foreach (var storedRecord in storedRecords)
            {
                var date = DateTime.Parse(storedRecord.Substring(0, 10));
                storedRecordsMetadata.Add(date);
            }
            return storedRecordsMetadata.OrderByDescending(a => a).ToList();
        }
    }
}
