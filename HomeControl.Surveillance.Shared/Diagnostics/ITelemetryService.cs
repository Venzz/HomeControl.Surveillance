using System;

namespace Venz.Telemetry
{
    public interface ITelemetryService
    {
        void Start();
        void Finish();
        void LogEvent(String title);
        void LogEvent(String title, String parameter, String value);
        void LogException(String comment, Exception exception);
    }
}
