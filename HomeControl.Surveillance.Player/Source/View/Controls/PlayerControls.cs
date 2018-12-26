using System;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;

namespace HomeControl.Surveillance.Player.View.Controls
{
    public class PlayerControls: MediaTransportControls
    {
        private AppBarToggleButton NormalRateButton;
        private AppBarToggleButton MaxRateButton;
        private AppBarToggleButton FastForwardButton;

        public event TypedEventHandler<PlayerControls, Object> NormalRateEnabled = delegate { };
        public event TypedEventHandler<PlayerControls, Object> MaxRateEnabled = delegate { };
        public event TypedEventHandler<PlayerControls, Object> FastForwardEnabled = delegate { };
        public event TypedEventHandler<PlayerControls, Object> FastForwardDisabled = delegate { };



        public PlayerControls()
        {
            DefaultStyleKey = typeof(PlayerControls);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            NormalRateButton = (AppBarToggleButton)GetTemplateChild("SetNormalRateButton");
            NormalRateButton.Click += OnNormalRateButtonClicked;
            NormalRateButton.IsChecked = true;

            MaxRateButton = (AppBarToggleButton)GetTemplateChild("SetMaxRateButton");
            MaxRateButton.Click += OnMaxRateButtonClicked;

            FastForwardButton = (AppBarToggleButton)GetTemplateChild("FastForwardButton");
            FastForwardButton.Click += OnFastForwardButtonClicked;
        }

        private void OnNormalRateButtonClicked(Object sender, Windows.UI.Xaml.RoutedEventArgs args)
        {
            if (NormalRateButton.IsChecked == true)
            {
                MaxRateButton.IsChecked = false;
                NormalRateEnabled(this, null);
            }
            else
            {
                MaxRateButton.IsChecked = true;
                MaxRateEnabled(this, null);
            }
        }

        private void OnMaxRateButtonClicked(Object sender, Windows.UI.Xaml.RoutedEventArgs args)
        {
            if (MaxRateButton.IsChecked == true)
            {
                NormalRateButton.IsChecked = false;
                MaxRateEnabled(this, null);
            }
            else
            {
                NormalRateButton.IsChecked = true;
                NormalRateEnabled(this, null);
            }
        }

        private void OnFastForwardButtonClicked(Object sender, Windows.UI.Xaml.RoutedEventArgs args)
        {
            if (FastForwardButton.IsChecked == true)
                FastForwardEnabled(this, null);
            else
                FastForwardDisabled(this, null);
        }
    }
}
