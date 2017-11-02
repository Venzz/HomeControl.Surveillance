using System;
using System.IO;
using System.Threading.Tasks;

namespace HomeControl.Surveillance.Server.Data.File
{
    public class FileStorageService: IStorageService
    {
        private Stream ActiveFileStream;
        private String ActiveFileName;
        private Task DataSendingSequence = Task.CompletedTask;

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
                App.Diagnostics.Debug.Log($"{nameof(FileStorageService)}.{nameof(StoreInternal)}", exception);
            }
        }
    }
}