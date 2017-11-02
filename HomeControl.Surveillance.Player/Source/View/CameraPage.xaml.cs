using HomeControl.Surveillance.Player.Model;
using HomeControl.Surveillance.Player.ViewModel;
using System;
using System.Threading.Tasks;
using Windows.UI.Core;
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
        private Boolean IsDisposed;
        private ApplicationViewMode ViewMode;

        public CameraPage()
        {
            InitializeComponent();
            DataContext = Context;
        }

        protected override void OnNavigatedTo(NavigationEventArgs args)
        {
            Window.Current.Activated += OnWindowActivated;
            Window.Current.VisibilityChanged += OnWindowVisibilityChanged;
            Context.Initialize((CameraController)args.Parameter);
            VideoPlayer.SetMediaStreamSource(Context.MediaSource);
        }

        private async void OnWindowActivated(Object sender, WindowActivatedEventArgs args)
        {
            if (args.WindowActivationState != CoreWindowActivationState.Deactivated)
                return;

            await Task.Delay(1000);
            if (IsDisposed)
                return;

            if ((args.WindowActivationState == CoreWindowActivationState.Deactivated) && (ViewMode != ApplicationViewMode.CompactOverlay))
            {
                ViewMode = ApplicationViewMode.CompactOverlay;
                await ApplicationView.TryEnterViewModeAsync(ViewMode);
            }
        }

        private void OnWindowVisibilityChanged(Object sender, VisibilityChangedEventArgs args)
        {
            if (!args.Visible)
                Window.Current.Close();
        }

        private async void OnCompactViewTapped(Object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs args)
        {
            ViewMode = (ApplicationView.ViewMode == ApplicationViewMode.CompactOverlay) ? ApplicationViewMode.Default : ApplicationViewMode.CompactOverlay;
            await ApplicationView.TryEnterViewModeAsync(ViewMode);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs args)
        {
            Window.Current.Activated -= OnWindowActivated;
            Window.Current.VisibilityChanged -= OnWindowVisibilityChanged;
            IsDisposed = true;
            Context.Dispose();
        }
    }
}
