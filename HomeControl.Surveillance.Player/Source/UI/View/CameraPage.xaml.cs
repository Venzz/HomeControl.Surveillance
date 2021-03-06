﻿using HomeControl.Surveillance.Player.Model;
using HomeControl.Surveillance.Player.UI.Controller;
using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace HomeControl.Surveillance.Player.UI.View
{
    public sealed partial class CameraPage: Page
    {
        private CameraController Context = new CameraController();
        private ApplicationView ApplicationView = ApplicationView.GetForCurrentView();
        private ApplicationViewMode ViewMode;

        public CameraPage()
        {
            InitializeComponent();
            DataContext = Context;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs args)
        {
            var parameters = (List<Object>)args.Parameter;
            ApplicationView.Consolidated += OnApplicationViewConsolidated;
            Context.Initialize((Camera)parameters[0], (CoreDispatcher)parameters[1]);
            await Context.InitializeAsync();
            StoredRecordsView.SelectedIndex = 0;
        }

        private void OnStartZoomingInClicked(Object sender, RoutedEventArgs args) => Context.StartZoomingIn();

        private void OnStartZoomingOutClicked(Object sender, RoutedEventArgs args) => Context.StartZoomingOut();

        private void OnStopZoomingClicked(Object sender, RoutedEventArgs args) => Context.StopZooming();

        private void OnMenuButtonTapped(Object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs args) => Overlay.IsPaneOpen = !Overlay.IsPaneOpen;

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
                Overlay.Opacity = 0;
                Overlay.IsHitTestVisible = false;
            }
            else
            {
                await ApplicationView.TryEnterViewModeAsync(ViewMode);
                Overlay.Opacity = 1;
                Overlay.IsHitTestVisible = true;
            }
        }

        private void OnApplicationViewConsolidated(ApplicationView sender, ApplicationViewConsolidatedEventArgs args)
        {
            Context.Dispose();
            Window.Current.Close();
        }

        private async void OnSelectionChanged(Object sender, SelectionChangedEventArgs args)
        {
            var selectedItem = (CameraController.StoredRecord)args.AddedItems[0];
            if (selectedItem.Model == null)
            {
                VideoPlayer.AreTransportControlsEnabled = false;
                VideoPlayer.SetMediaStreamSource(Context.CameraStream.MediaStream);
                Context.CameraStream.Synchronize();
            }
            else
            {
                await Context.InitializeStoredRecordStreamAsync(selectedItem);
                VideoPlayer.SetMediaStreamSource(Context.StoredRecordStream.MediaStream);
                VideoPlayer.AreTransportControlsEnabled = true;
            }
        }
    }
}
