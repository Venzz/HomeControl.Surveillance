using System;
using System.IO;

namespace Venz.Telemetry
{
    public sealed class FileTelemetryService: ITelemetryService
    {
        private FileStream File;

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
                if (Path.GetFileName(File?.Name) != fileName)
                {
                    File?.Flush();
                    File?.Dispose();
                    File = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                }

                var writer = new StreamWriter(File);
                writer.WriteLine(value);
                writer.Flush();
            }
            catch (Exception)
            {
            }
        }
    }
}
