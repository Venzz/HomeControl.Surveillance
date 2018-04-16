using HomeControl.Surveillance.Player.Model;
using HomeControl.Surveillance.Player.ViewModel;
using System;
using Venz.Async;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace HomeControl.Surveillance.Player.View
{
    public sealed partial class CameraPage: Page
    {
        private CameraContext Context = new CameraContext();
        private DateTime LastScreenTappedDate = DateTime.Now;
        private PeriodicOperation AppBarHiding;

        public CameraPage()
        {
            InitializeComponent();
            DataContext = Context;
            AppBarHiding = PeriodicOperation.CreateUiBased(TimeSpan.FromSeconds(1));
            AppBarHiding.Triggered += OnAppBarHidingTriggered;
            ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
        }

        protected override void OnNavigatedTo(NavigationEventArgs args)
        {
            AppBarHiding.Start();
            Context.Initialize((CameraController)args.Parameter);
            VideoPlayer.SetMediaStreamSource(Context.CameraStream.MediaStream);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs args) => AppBarHiding.Stop();

        private void OnStartZoomingInTapped(Object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs args) => Context.StartZoomingIn();

        private void OnStartZoomingOutTapped(Object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs args) => Context.StartZoomingOut();

        private void OnStopZoomingTapped(Object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs args) => Context.StopZooming();
        
        private void OnSyncTapped(Object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs args) => Context.CameraStream.Synchronize();

        private void OnScreenTapped(Object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs args)
        {
            BottomAppBar.Visibility = Visibility.Visible;
            LastScreenTappedDate = DateTime.Now;
        }

        private void OnAppBarHidingTriggered(Object sender, EventArgs args)
        {
            if ((DateTime.Now - LastScreenTappedDate) > TimeSpan.FromSeconds(5))
                BottomAppBar.Visibility = Visibility.Collapsed;
        }
    }
}
