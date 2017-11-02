using System;

namespace Venz.Telemetry
{
    public sealed class EmptyTelemetryService: ITelemetryService
    {
        public void Start()
        {
        }

        public void Finish()
        {
        }

        public void LogEvent(String title)
        {
        }

        public void LogEvent(String title, String parameter, String value)
        {
        }

        public void LogException(String comment, Exception exception)
        {
        }
    }
}
