using System;

namespace Venz.Telemetry
{
    public sealed class Diagnostics
    {
        public DiagnosticsLevel Info { get; }
        public DiagnosticsLevel Debug { get; }
        public DiagnosticsLevel Error { get; }

        public Diagnostics(String processName)
        {
            Info = new DiagnosticsLevel(processName);
            Debug = new DiagnosticsLevel(processName);
            Error = new DiagnosticsLevel(processName);
        }
    }
}