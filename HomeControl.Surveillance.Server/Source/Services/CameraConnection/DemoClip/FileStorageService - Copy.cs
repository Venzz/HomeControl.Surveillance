using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;

namespace HomeControl.Surveillance.Server.Services
{
    public class FileStorageService1
    {
        private TimeSpan DataCacheDuration = TimeSpan.FromSeconds(10);
        private List<(DateTime DateAdded, Byte[] Data)> DataCache = new List<(DateTime, Byte[])>();
        private Task DataStoringSequence = Task.CompletedTask;
        private DriveInfo Drive;
        private DirectoryInfo CurrentDirectory;

        public event TypedEventHandler<IStorageService, (String, String)> Log = delegate { };
        public event TypedEventHandler<IStorageService, (String, String, Exception)> Exception = delegate { };



        public FileStorageService1()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            CurrentDirectory = new DirectoryInfo(currentDirectory);
            foreach (var drive in DriveInfo.GetDrives())
                if (currentDirectory.StartsWith(drive.Name))
                    Drive = drive;

            if (Drive == null)
                throw new InvalidOperationException("Drive not found.");

            foreach (var file in CurrentDirectory.GetFiles("*.*", SearchOption.TopDirectoryOnly).OrderBy(a => a.CreationTime).Where(a => a.Extension == ".test" || a.Extension == ".log"))
                file.Delete();
        }

        public void Store(Byte[] mediaData)
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
                        DataCache = new List<(DateTime, Byte[])>();
                    }
                }
            }
            catch (Exception exception)
            {
                Exception(null, ($"{nameof(FileStorageService)}.{nameof(Store)}", null, exception));
            }
        }

        private void StoreCache(List<(DateTime DateAdded, Byte[] Data)> dataCache)
        {
            Console.WriteLine("StoreCache");
            try
            {
                foreach (var dataCacheSlice in dataCache)
                {
                    var fileName = $"{dataCacheSlice.DateAdded.ToString("yyyy-MM-dd_HH")}.test";
                    using (var fileStream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                    using (var binaryWriter = new BinaryWriter(fileStream))
                    {
                        fileStream.Seek(0, SeekOrigin.End);
                        binaryWriter.Write(dataCacheSlice.Data);
                        binaryWriter.Write(0xBBBBBBBBBBBBBBBB);
                    }
                }
            }
            catch (Exception exception)
            {
                Exception(null, ($"{nameof(FileStorageService)}.{nameof(StoreCache)}", null, exception));
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