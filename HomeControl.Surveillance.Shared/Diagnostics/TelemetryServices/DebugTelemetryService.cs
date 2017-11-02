using System;
using System.Diagnostics;

namespace Venz.Telemetry
{
    public sealed class DebugTelemetryService: ITelemetryService
    {
        public void Start()
        {
            Debug.WriteLine($"{GetTimestamp()} >> Application Launched");
        }

        public void Finish()
        {
            Debug.WriteLine($"{GetTimestamp()} >> Application Exit");
        }

        public void LogEvent(String title)
        {
            Debug.WriteLine($"{GetTimestamp()} >> {title}");
        }

        public void LogEvent(String title, String parameter, String value)
        {
            Debug.WriteLine($"{GetTimestamp()} >> {title} || {parameter}: {value}");
        }

        public void LogException(String comment, Exception exception)
        {
            Debug.WriteLine($"{GetTimestamp()} >> {comment} || {exception.GetType().FullName}: {exception.Message}");
        }

        private String GetTimestamp() => DateTime.Now.ToString("yy-MM-dd HH:mm:ss");
    }
}
