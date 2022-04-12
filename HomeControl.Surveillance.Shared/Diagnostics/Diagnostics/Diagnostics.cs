using System;

namespace Venz.Telemetry
{
    public sealed class Diagnostics
    {
        public DiagnosticsLevel Info { get; }
        public DiagnosticsLevel Debug { get; }
        public DiagnosticsLevel Error { get; }
        public DiagnosticsLevel Console { get; }
        public DiagnosticsLevel File { get; }

        public Diagnostics(String processName)
        {
            Info = new DiagnosticsLevel(processName);
            Debug = new DiagnosticsLevel(processName);
            Error = new DiagnosticsLevel(processName);
            Console = new DiagnosticsLevel(processName);
            File = new DiagnosticsLevel(processName);
        }
    }
}