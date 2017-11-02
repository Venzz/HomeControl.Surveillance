using HomeControl.Surveillance.Player.Model;
using HomeControl.Surveillance.Player.View;
using Venz.Telemetry;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace HomeControl.Surveillance.Player
{
    public sealed partial class App: Application
    {
        public static Diagnostics Diagnostics { get; } = new Diagnostics("App");
        public static ApplicationModel Model { get; } = new ApplicationModel();

        public App()
        {
            InitializeComponent();
            Diagnostics.Debug.Add(new DebugTelemetryService());
        }
        
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
            {
                rootFrame = new Frame();
                Window.Current.Content = rootFrame;
            }

            if (args.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                    rootFrame.Navigate(typeof(HubPage), args.Arguments);
                Window.Current.Activate();
            }
        }
    }
}
