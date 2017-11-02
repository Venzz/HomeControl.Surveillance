using System;
using System.IO;

namespace Venz.Telemetry
{
    public sealed class FileTelemetryService: ITelemetryService
    {
        private StreamWriter Writer;

        public FileTelemetryService()
        {
            Writer = new StreamWriter(new FileStream($"{DateTime.Now.ToString("yyyy-MM-dd_HH-mm")}.log", FileMode.Create));
        }

        public void Start()
        {
            Writer.WriteLine($"{GetTimestamp()} >> Application Launched");
        }

        public void Finish()
        {
            Writer.WriteLine($"{GetTimestamp()} >> Application Exit");
        }

        public void LogEvent(String title)
        {
            Writer.WriteLine($"{GetTimestamp()} >> {title}");
            Writer.Flush();
        }

        public void LogEvent(String title, String parameter, String value)
        {
            Writer.WriteLine($"{GetTimestamp()} >> {title} || {parameter}: {value}");
            Writer.Flush();
        }

        public void LogException(String comment, Exception exception)
        {
            Writer.WriteLine($"{GetTimestamp()} >> {comment} || {exception.GetType().FullName}: {exception.Message}");
            Writer.Flush();
        }

        private String GetTimestamp() => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
