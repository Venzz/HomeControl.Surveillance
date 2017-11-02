using HomeControl.Surveillance.Server.Model;
using System.Threading.Tasks;
using System.Windows;
using Venz.Telemetry;

namespace HomeControl.Surveillance.Server
{
    public partial class App: Application
    {
        public static Diagnostics Diagnostics { get; } = new Diagnostics("App");
        public static ApplicationModel Model { get; } = new ApplicationModel();

        public App()
        {
            Diagnostics.Debug.Add(new DebugTelemetryService());
            Diagnostics.Debug.Add(new FileTelemetryService());
        }

        protected override async void OnStartup(StartupEventArgs args) => await Task.Run(() =>
        {
            Model.Initialize();
        });
    }
}
