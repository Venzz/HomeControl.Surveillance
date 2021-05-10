using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;

namespace HomeControl.Surveillance.Server.Services
{
    public class FileStorageService: IStorageService
    {
        private TimeSpan CachedDurationLength = TimeSpan.FromMinutes(10);
        private DateTime CachedDataDate = DateTime.Now;
        private List<IMediaData> CachedData = new List<IMediaData>();
        private Task DataFlushingSequence = Task.CompletedTask;
        private Stream ActiveFileStream;
        private String ActiveFileName;

        public event TypedEventHandler<IStorageService, (String CustomText, String Parameter)> LogReceived = delegate { };
        public event TypedEventHandler<IStorageService, (String CustomText, Exception Exception)> ExceptionReceived = delegate { };



        public void Store(IMediaData mediaData)
        {
            try
            {
                CachedData.Add(mediaData);
                var now = DateTime.Now;
                if ((now - CachedDataDate > CachedDurationLength) || (now.Hour != CachedDataDate.Hour))
                {
                    var cachedData = CachedData;
                    var cachedDataDate = CachedDataDate;
                    DataFlushingSequence = DataFlushingSequence.ContinueWith(task => FlushAsync(cachedData, cachedDataDate)).Unwrap();
                    CachedData = new List<IMediaData>();
                    CachedDataDate = now;
                }
            }
            catch (Exception exception)
            {
                ExceptionReceived(this, ($"{nameof(FileStorageService)}.{nameof(Store)}", exception));
            }
        }

        private Task FlushAsync(List<IMediaData> mediaData, DateTime mediaDataDate) => Task.Run(() =>
        {
            try
            {
                var mediaStream = new MemoryStream();
                var mediaDataItems = mediaData.Select(a => (new StoredRecordFile.MediaDataDescriptor() { Type = a.MediaDataType, Timestamp = a.Timestamp, Duration = a.Duration }, a.Data)).ToList();
                var storedRecord = new StoredRecordFile(mediaStream);
                storedRecord.WriteSlice(mediaDataItems);

                var fileName = $"{mediaDataDate.ToString("yyyy-MM-dd_HH")}";
                if (ActiveFileName != fileName)
                {
                    if (ActiveFileStream != null)
                    {
                        ActiveFileStream.Flush();
                        ActiveFileStream.Close();
                    }
                    ActiveFileStream = new FileStream($"{fileName}.sr", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                    ActiveFileName = fileName;
                }

                mediaStream.Seek(0, SeekOrigin.Begin);
                mediaStream.CopyTo(ActiveFileStream);
                mediaStream.Dispose();
                ActiveFileStream.Flush();
                LogReceived(this, (nameof(FileStorageService), $"Stored {mediaStream.Length} of data."));
            }
            catch (Exception exception)
            {
                ExceptionReceived(this, ($"{nameof(FileStorageService)}.{nameof(FlushAsync)}", exception));
            }
        });

        public IReadOnlyCollection<String> GetStoredRecords()
        {
            var storedRecords = new List<String>();
            var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
            foreach (var file in directory.GetFiles("*.sr", SearchOption.TopDirectoryOnly))
                storedRecords.Add(file.Name);
            return storedRecords;
        }

        public IReadOnlyCollection<StoredRecordFile.MediaDataDescriptor> GetStoredRecordMediaDescriptors(String id)
        {
            var file = new FileInfo(id);
            using (var fileStream = file.Open(FileMode.Open, FileAccess.Read))
                return new StoredRecordFile(fileStream).ReadMediaDescriptors();
        }

        public Byte[] GetStoredRecordMediaData(String id, UInt32 offset)
        {
            var file = new FileInfo(id);
            using (var fileStream = file.Open(FileMode.Open, FileAccess.Read))
            using (var binaryFileReader = new BinaryReader(fileStream))
            {
                fileStream.Position = offset;
                var size = binaryFileReader.ReadInt32();
                return binaryFileReader.ReadBytes(size);
            }
        }
    }
}