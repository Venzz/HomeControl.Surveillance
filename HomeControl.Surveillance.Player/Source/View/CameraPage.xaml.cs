using HomeControl.Surveillance.Player.Model;
using HomeControl.Surveillance.Player.ViewModel;
using System;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace HomeControl.Surveillance.Player.View
{
    public sealed partial class CameraPage: Page
    {
        private CameraContext Context = new CameraContext();
        private ApplicationView ApplicationView = ApplicationView.GetForCurrentView();
        private ApplicationViewMode ViewMode;

        public CameraPage()
        {
            InitializeComponent();
            DataContext = Context;
        }

        protected override void OnNavigatedTo(NavigationEventArgs args)
        {
            ApplicationView.Consolidated += OnApplicationViewConsolidated;
            Context.Initialize((CameraController)args.Parameter);
            VideoPlayer.SetMediaStreamSource(Context.MediaSource);
        }

        private async void OnCompactViewTapped(Object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs args)
        {
            ViewMode = (ApplicationView.ViewMode == ApplicationViewMode.CompactOverlay) ? ApplicationViewMode.Default : ApplicationViewMode.CompactOverlay;
            if (ViewMode == ApplicationViewMode.CompactOverlay)
            {
                var compactOptions = ViewModePreferences.CreateDefault(ApplicationViewMode.CompactOverlay);
                compactOptions.CustomSize = new Size(500, 281);
                await ApplicationView.TryEnterViewModeAsync(ViewMode, compactOptions);
            }
            else
            {
                await ApplicationView.TryEnterViewModeAsync(ViewMode);
            }
        }

        private void OnApplicationViewConsolidated(ApplicationView sender, ApplicationViewConsolidatedEventArgs args)
        {
            Context.Dispose();
            Window.Current.Close();
        }
    }
}
