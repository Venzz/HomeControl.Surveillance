using HomeControl.Surveillance.Server.Model;
using System.Threading.Tasks;
using Venz.Telemetry;

namespace HomeControl.Surveillance.Server
{
    public class App
    {
        public static Diagnostics Diagnostics { get; } = new Diagnostics("App");
        public static ApplicationModel Model { get; } = new ApplicationModel();

        public App()
        {
            Diagnostics.Debug.Add(new DebugTelemetryService());
            Diagnostics.Debug.Add(new FileTelemetryService());
        }

        public async void Start() => await Task.Run(() =>
        {
            Model.Initialize();
        });
    }
}
