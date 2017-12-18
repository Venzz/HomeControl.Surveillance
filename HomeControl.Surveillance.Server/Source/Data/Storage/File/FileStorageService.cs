using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Foundation;

namespace HomeControl.Surveillance.Server.Data.File
{
    public class FileStorageService: IStorageService
    {
        private Stream ActiveFileStream;
        private String ActiveFileName;
        private Task DataSendingSequence = Task.CompletedTask;

        public event TypedEventHandler<IStorageService, (String CustomText, Exception Exception)> ExceptionReceived = delegate { };



        public void Store(Byte[] data) => DataSendingSequence = DataSendingSequence.ContinueWith(task => StoreInternal(data));

        private void StoreInternal(Byte[] data)
        {
            try
            {
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
                ExceptionReceived(this, ($"{nameof(FileStorageService)}.{nameof(StoreInternal)}", exception));
            }
        }
    }
}