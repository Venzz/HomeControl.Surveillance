using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Foundation;

namespace HomeControl.Surveillance.Server.Data.File
{
    public class FileStorageService: IStorageService
    {
        private const Int32 CachedFileStreamLength = 128 * 1024 * 1024;

        private Stream ActiveFileStream;
        private String ActiveFileName;
        private MemoryStream CachedFileStream = new MemoryStream(CachedFileStreamLength);
        private Task DataFlushingSequence = Task.CompletedTask;

        public event TypedEventHandler<IStorageService, (String CustomText, Exception Exception)> ExceptionReceived = delegate { };

        public void Store(Byte[] data)
        {
            try
            {
                CachedFileStream.Write(data, 0, data.Length);
                if (CachedFileStream.Length > CachedFileStreamLength)
                {
                    var cachedFileStream = CachedFileStream;
                    DataFlushingSequence = DataFlushingSequence.ContinueWith(task => FlushAsync(cachedFileStream)).Unwrap();
                    CachedFileStream = new MemoryStream(CachedFileStreamLength);
                }
            }
            catch (Exception exception)
            {
                ExceptionReceived(this, ($"{nameof(FileStorageService)}.{nameof(Store)}", exception));
            }
        }

        private Task FlushAsync(MemoryStream stream) => Task.Run(() =>
        {
            try
            {
                var data = new Byte[stream.Length];
                stream.Position = 0;
                stream.Read(data, 0, data.Length);
                stream.Dispose();

                var now = DateTime.Now;
                var fileName = $"{now.ToString("yyyy-MM-dd_HH")}";
                if (ActiveFileName != fileName)
                {
                    if (ActiveFileStream != null)
                    {
                        ActiveFileStream.Flush();
                        ActiveFileStream.Close();
                    }
                    ActiveFileStream = new FileStream($"{fileName}.h264", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                    ActiveFileName = fileName;
                }
                ActiveFileStream.Write(data, 0, data.Length);
                ActiveFileStream.Flush();
            }
            catch (Exception exception)
            {
                ExceptionReceived(this, ($"{nameof(FileStorageService)}.{nameof(FlushAsync)}", exception));
            }
        });
    }
}