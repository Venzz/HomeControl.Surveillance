using System;

namespace Venz.Telemetry
{
    public interface IPerformanceDiagnostics: IDisposable
    {
        IPerformanceDiagnostics TakesMoreThan(TimeSpan threshold);
    }
}
