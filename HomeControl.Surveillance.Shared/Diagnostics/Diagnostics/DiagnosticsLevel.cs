using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Venz.Telemetry
{
    public sealed class DiagnosticsLevel
    {
        private String ProcessName;
        private Object Sync = new Object();
        private IList<ITelemetryService> TelemetryServices = new List<ITelemetryService>();

        public DiagnosticsLevel(String processName)
        {
            ProcessName = processName;
        }

        public void Log(String customText)
        {
            lock (Sync)
                foreach (var service in TelemetryServices)
                    service.LogEvent($"{ProcessName} || {customText}");
        }
        
        public void Log(String customText, String parameter)
        {
            lock (Sync)
                foreach (var service in TelemetryServices)
                    service.LogEvent($"{ProcessName} || {customText} || {parameter}");
        }

        public void Log(String customText, Exception exception)
        {
            lock (Sync)
                foreach (var service in TelemetryServices)
                    service.LogException($"{ProcessName} || {customText}", exception);
        }

        public IPerformanceDiagnostics LogIf(String customText, String parameter) => new PerformanceDiagnostics(this, customText, parameter);

        public void Add(ITelemetryService telemetryService)
        {
            lock (Sync)
                TelemetryServices.Add(telemetryService);
        }



        private class PerformanceDiagnostics: IPerformanceDiagnostics
        {
            private DiagnosticsLevel DiagnosticsLevel;
            private Stopwatch Watch;
            private String Parameter;
            private TimeSpan Threshold;
            private String CustomText;

            public PerformanceDiagnostics(DiagnosticsLevel diagnosticsLevel, String customText, String parameter)
            {
                DiagnosticsLevel = diagnosticsLevel;
                Parameter = parameter;
                CustomText = customText;
            }

            public IPerformanceDiagnostics TakesMoreThan(TimeSpan threshold)
            {
                Threshold = threshold;
                Watch = new Stopwatch();
                Watch.Start();
                return this;
            }

            public void Dispose()
            {
                Watch.Stop();
                if ((Threshold.TotalMilliseconds == 0) || (Watch.Elapsed > Threshold))
                    DiagnosticsLevel.Log(CustomText, $"Elapsed {Watch.Elapsed.Minutes:00}:{Watch.Elapsed.Seconds:00}.{Watch.Elapsed.Milliseconds:000} || {Parameter}");
            }
        }
    }
}