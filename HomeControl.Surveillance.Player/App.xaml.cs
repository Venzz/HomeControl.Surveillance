using HomeControl.Surveillance.Player.Model;
using HomeControl.Surveillance.Player.UI.View;
using System;
using System.Threading.Tasks;
using Venz.Telemetry;
using Venz.UI.Xaml;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.UI.ViewManagement;

namespace HomeControl.Surveillance.Player
{
    public sealed partial class App: Application
    {
        public static Diagnostics Diagnostics { get; } = new Diagnostics("App");
        public static Settings Settings { get; } = new Settings();
        public static ApplicationModel Model { get; } = new ApplicationModel();

        public App()
        {
            InitializeComponent();
            Diagnostics.Debug.Add(new DebugTelemetryService());
        }

        protected override Task OnManuallyActivatedAsync(Frame frame, Boolean newInstance, PrelaunchStage prelaunchStage, String args)
        {
            if (!String.IsNullOrWhiteSpace(args) && (frame.Content == null))
            {
                ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;
                frame.Navigate(typeof(CameraPage), (args == App.Model.OutdoorCameraController.Id) ? App.Model.OutdoorCameraController : App.Model.IndoorCameraController);
                return Model.InitializeAsync();
            }
            else if (frame.Content == null)
            {
                ApplicationView.PreferredLaunchViewSize = new Size(800, 800);
                ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
                frame.Navigate(typeof(HubPage));
                return Model.InitializeAsync();
            }
            return Task.CompletedTask;
        }

        protected override Task OnFileActivatedAsync(Frame frame, Boolean newInstance, PrelaunchStage prelaunchStage, FileActivatedEventArgs args)
        {
            if (!(frame.Content is PlayerPage))
                frame.Navigate(typeof(PlayerPage), null);

            ((PlayerPage)frame.Content).Activate(args.Files[0]);
            return Task.CompletedTask;
        }
    }
}
