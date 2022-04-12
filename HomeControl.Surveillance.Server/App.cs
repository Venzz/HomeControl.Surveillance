using HomeControl.Surveillance.Server.Model;
using System;
using System.Threading.Tasks;
using Venz.Telemetry;

namespace HomeControl.Surveillance.Server
{
    public class App
    {
        public static Diagnostics Diagnostics { get; } = new Diagnostics("Server");
        public static ApplicationModel Model { get; } = new ApplicationModel();

        public App()
        {
            Diagnostics.Console.Add(new ConsoleTelemetryService());
            #if !DEBUG
            Diagnostics.File.Add(new FileTelemetryService());
            #endif
        }

        public async void Start() => await Task.Run(async () =>
        {
            await Model.InitializeAsync().ConfigureAwait(false);
        });

        public sealed class ConsoleTelemetryService: ITelemetryService
        {
            public void Start() => Console.WriteLine($"{GetTimestamp()} >> Application Launched");
            public void Finish() => Console.WriteLine($"{GetTimestamp()} >> Application Exit");
            public void LogEvent(String title) => Console.WriteLine($"{GetTimestamp()} >> {title}");
            public void LogEvent(String title, String parameter, String value) => Console.WriteLine($"{GetTimestamp()} >> {title} || {parameter}: {value}");
            public void LogException(String comment, Exception exception) => Console.WriteLine($"{GetTimestamp()} >> {comment} || {exception.GetType().FullName}: {exception.Message}");

            private String GetTimestamp() => DateTime.Now.ToString("yy-MM-dd HH:mm:ss");
        }
    }
}
