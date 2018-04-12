﻿using HomeControl.Surveillance.Player.ViewModel;
using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace HomeControl.Surveillance.Player.View
{
    public sealed partial class HubPage: Page
    {
        private HubContext Context = new HubContext();

        public HubPage()
        {
            InitializeComponent();
            DataContext = Context;
            SizeChanged += OnSizeChanged;
        }

        protected override void OnNavigatedTo(NavigationEventArgs args)
        {
            OutdoorCameraPreview.SetMediaStreamSource(Context.OutdoorCameraStream.MediaStream);
            IndoorCameraPreview.SetMediaStreamSource(Context.IndoorCameraStream.MediaStream);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs args)
        {
            Context.Dispose();
        }

        private async void OnCameraTapped(Object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs args)
        {
            var applicationViewId = 0;
            var coreApplicationView = CoreApplication.CreateNewView();
            await coreApplicationView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var frame = new Frame();
                frame.Navigate(typeof(CameraPage), (sender == OutdoorCamera) ? App.Model.OutdoorCameraController : App.Model.IndoorCameraController);
                Window.Current.Content = frame;
                Window.Current.Activate();
                applicationViewId = ApplicationView.GetForCurrentView().Id;
            });
            await ApplicationViewSwitcher.TryShowAsStandaloneAsync(applicationViewId);
        }

        private void OnSizeChanged(Object sender, SizeChangedEventArgs args)
        {
            var availableSize = args.NewSize.Width - 40;
            OutdoorCamera.Width = availableSize;
            OutdoorCamera.Height = availableSize * 9 / 16;
            IndoorCamera.Width = availableSize;
            IndoorCamera.Height = availableSize * 9 / 16;
        }
    }
}
