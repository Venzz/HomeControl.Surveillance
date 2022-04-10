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
        private TimeSpan DataCacheDuration = TimeSpan.FromMinutes(10);
        private List<(DateTime DateAdded, IMediaData Data)> DataCache = new List<(DateTime, IMediaData)>();
        private Task DataStoringSequence = Task.CompletedTask;
        private DriveInfo Drive;
        private DirectoryInfo CurrentDirectory;

        public event TypedEventHandler<IStorageService, (String CustomText, String Parameter)> LogReceived = delegate { };
        public event TypedEventHandler<IStorageService, (String CustomText, Exception Exception)> ExceptionReceived = delegate { };



        public FileStorageService()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            CurrentDirectory = new DirectoryInfo(currentDirectory);
            foreach (var drive in DriveInfo.GetDrives())
                if (currentDirectory.StartsWith(drive.Name))
                    Drive = drive;

            if (Drive == null)
                throw new InvalidOperationException("Drive not found.");
        }

        public void Store(IMediaData mediaData)
        {
            try
            {
                lock (this)
                {
                    var now = DateTime.Now;
                    DataCache.Add((now, mediaData));
                    if (now - DataCache[0].DateAdded > DataCacheDuration)
                    {
                        var dataCache = DataCache;
                        DataStoringSequence = DataStoringSequence.ContinueWith(task => StoreCache(dataCache));
                        DataCache = new List<(DateTime, IMediaData)>();
                    }
                }
            }
            catch (Exception exception)
            {
                ExceptionReceived(this, ($"{nameof(FileStorageService)}.{nameof(Store)}", exception));
            }
        }

        private void StoreCache(List<(DateTime DateAdded, IMediaData Data)> dataCache)
        {
            try
            {
                if (Drive.AvailableFreeSpace / 1024 / 1024 / 1024 < 1)
                {
                    var files = new List<FileInfo>();
                    foreach (var file in CurrentDirectory.GetFiles("*.*", SearchOption.TopDirectoryOnly).OrderBy(a => a.CreationTime).Where(a => a.Extension == ".sr" || a.Extension == ".log"))
                        files.Add(file);

                    while (Drive.AvailableFreeSpace / 1024 / 1024 / 1024 < 2 && files.Count > 0)
                    {
                        LogReceived(this, (nameof(FileStorageService), $"Deleted {files[0].Name}."));
                        files[0].Delete();
                        files.RemoveAt(0);
                    }
                }

                var dataCacheByHour = dataCache.GroupBy(a => a.DateAdded.Hour);
                foreach (var dataCacheSlice in dataCacheByHour)
                {
                    var mediaStream = new MemoryStream();
                    var mediaDataItems = dataCacheSlice.Select(a => (new StoredRecordFile.MediaDataDescriptor() { Type = a.Data.MediaDataType, Timestamp = a.Data.Timestamp, Duration = a.Data.Duration }, a.Data.Data)).ToList();
                    var storedRecord = new StoredRecordFile(mediaStream);
                    storedRecord.WriteSlice(mediaDataItems);
                    var mediaStreamLength = mediaStream.Length;

                    var fileName = $"{dataCacheSlice.First().DateAdded.ToString("yyyy-MM-dd_HH")}.sr";
                    using (var fileStream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        mediaStream.Seek(0, SeekOrigin.Begin);
                        mediaStream.CopyTo(fileStream);
                        mediaStream.Dispose();
                        LogReceived(this, (nameof(FileStorageService), $"Stored {mediaStreamLength.ToDataLength()} to {fileName}."));
                    }
                }
            }
            catch (Exception exception)
            {
                ExceptionReceived(this, ($"{nameof(FileStorageService)}.{nameof(StoreCache)}", exception));
            }
        }

        public IReadOnlyCollection<String> GetStoredRecords()
        {
            var storedRecords = new List<String>();
            foreach (var file in CurrentDirectory.GetFiles("*.sr", SearchOption.TopDirectoryOnly))
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