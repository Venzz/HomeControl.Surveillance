using HomeControl.Surveillance.Data.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;

namespace HomeControl.Surveillance.Server.Data.File
{
    public class FileStorageService: IStorageService
    {
        private TimeSpan CachedDurationLength = TimeSpan.FromMinutes(10);

        private Stream ActiveFileStream;
        private String ActiveFileName;
        private List<IMediaData> CachedData = new List<IMediaData>();
        private TimeSpan CachedDuration = new TimeSpan();
        private Task DataFlushingSequence = Task.CompletedTask;

        public event TypedEventHandler<IStorageService, (String CustomText, Exception Exception)> ExceptionReceived = delegate { };



        public void Store(IMediaData mediaData)
        {
            try
            {
                CachedData.Add(mediaData);
                if ((mediaData.MediaDataType == MediaDataType.InterFrame) || (mediaData.MediaDataType == MediaDataType.PredictionFrame))
                    CachedDuration += mediaData.Duration;

                if (CachedDuration > CachedDurationLength)
                {
                    var cachedData = CachedData;
                    DataFlushingSequence = DataFlushingSequence.ContinueWith(task => FlushAsync(cachedData)).Unwrap();
                    CachedData = new List<IMediaData>();
                    CachedDuration = new TimeSpan();
                }
            }
            catch (Exception exception)
            {
                ExceptionReceived(this, ($"{nameof(FileStorageService)}.{nameof(Store)}", exception));
            }
        }

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
                return StoredRecordFile.ReadMediaDescriptors(fileStream);
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

        private Task FlushAsync(List<IMediaData> mediaData) => Task.Run(() =>
        {
            try
            {
                var mediaStream = new MemoryStream();
                var mediaDataItems = mediaData.Select(a => (new StoredRecordFile.MediaDataDescriptor() { Type = a.MediaDataType, Timestamp = a.Timestamp, Duration = a.Duration }, a.Data)).ToList();
                StoredRecordFile.WriteSlice(mediaStream, mediaDataItems);

                var now = DateTime.Now;
                var fileName = $"{now.ToString("yyyy-MM-dd_HH")}";
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
            }
            catch (Exception exception)
            {
                ExceptionReceived(this, ($"{nameof(FileStorageService)}.{nameof(FlushAsync)}", exception));
            }
        });
    }
}