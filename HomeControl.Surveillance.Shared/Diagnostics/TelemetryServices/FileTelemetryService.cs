using System;
using System.IO;

namespace Venz.Telemetry
{
    public sealed class FileTelemetryService: ITelemetryService
    {
        public FileTelemetryService() { }

        public void Start() => Write($"{GetTimestamp()} >> Application Launched");

        public void Finish() => Write($"{GetTimestamp()} >> Application Exit");

        public void LogEvent(String title) => Write($"{GetTimestamp()} >> {title}");

        public void LogEvent(String title, String parameter, String value) => Write($"{GetTimestamp()} >> {title} || {parameter}: {value}");

        public void LogException(String comment, Exception exception) => Write($"{GetTimestamp()} >> {comment} || {exception.GetType().FullName}: {exception.Message}");

        private String GetTimestamp() => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        private void Write(String value)
        {
            try
            {
                var now = DateTime.Now;
                var fileName = $"{now.ToString("yyyy-MM-dd")}.log";
                using (var fileStream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                using (var writer = new StreamWriter(fileStream))
                {
                    fileStream.Seek(0, SeekOrigin.End);
                    writer.WriteLine(value);
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
