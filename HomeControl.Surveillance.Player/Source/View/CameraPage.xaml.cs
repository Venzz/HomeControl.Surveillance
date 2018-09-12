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
            VideoPlayer.SetMediaStreamSource(Context.CameraStream.MediaStream);
        }

        private void OnStartZoomingInClicked(Object sender, RoutedEventArgs args) => Context.StartZoomingIn();

        private void OnStartZoomingOutClicked(Object sender, RoutedEventArgs args) => Context.StartZoomingOut();

        private void OnStopZoomingClicked(Object sender, RoutedEventArgs args) => Context.StopZooming();

        private async void OnPinTapped(Object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs args) => await Context.ManageTileAsync();

        private void OnSyncTapped(Object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs args) => Context.CameraStream.Synchronize();

        private async void OnCompactViewTapped(Object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs args)
        {
            ViewMode = (ApplicationView.ViewMode == ApplicationViewMode.CompactOverlay) ? ApplicationViewMode.Default : ApplicationViewMode.CompactOverlay;
            if (ViewMode == ApplicationViewMode.CompactOverlay)
            {
                var compactOptions = ViewModePreferences.CreateDefault(ApplicationViewMode.CompactOverlay);
                compactOptions.CustomSize = new Size(500, 281);
                await ApplicationView.TryEnterViewModeAsync(ViewMode, compactOptions);
                CommandsControl.Opacity = 0;
                CommandsControl.IsHitTestVisible = false;
            }
            else
            {
                await ApplicationView.TryEnterViewModeAsync(ViewMode);
                CommandsControl.Opacity = 1;
                CommandsControl.IsHitTestVisible = true;
            }
        }

        private void OnApplicationViewConsolidated(ApplicationView sender, ApplicationViewConsolidatedEventArgs args)
        {
            Context.Dispose();
            Window.Current.Close();
        }
    }
}
