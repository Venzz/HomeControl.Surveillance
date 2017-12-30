﻿using HomeControl.Surveillance.Player.Model;
using HomeControl.Surveillance.Player.View;
using System;
using System.Threading.Tasks;
using Venz.Telemetry;
using Venz.UI.Xaml;
using Windows.ApplicationModel.Activation;
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

        protected override Task OnManuallyActivatedAsync(Frame frame, Boolean newInstance, String args)
        {
            if (frame.Content == null)
                frame.Navigate(typeof(HubPage), args);

            return Task.CompletedTask;
        }

        protected override Task OnFileActivatedAsync(Frame frame, Boolean newInstance, FileActivatedEventArgs args)
        {
            if (!(frame.Content is PlayerPage))
                frame.Navigate(typeof(PlayerPage), null);

            ((PlayerPage)frame.Content).Activate(args.Files[0]);
            return Task.CompletedTask;
        }
    }
}
