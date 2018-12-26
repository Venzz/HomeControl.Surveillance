using HomeControl.Surveillance.Player.ViewModel;
using System;
using Windows.Media.Core;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace HomeControl.Surveillance.Player.View
{
    public sealed partial class PlayerPage: Page
    {
        private PlayerContext Context = new PlayerContext();

        public PlayerPage()
        {
            InitializeComponent();
            DataContext = Context;
            VideoPlayer.SetMediaPlayer(Context.MediaPlayer);
        }

        public async void Activate(IStorageItem item)
        {
            if (!(item is StorageFile file))
                return;

            Context.TryCloseOpenedFile();
            await Context.OpenAsync(file);
        }

        private void OnNormalRateEnabled(Controls.PlayerControls sender, Object args) => Context.EnabledNormalRate();

        private void OnMaxRateEnabled(Controls.PlayerControls sender, Object args) => Context.EnabledMaxRate();

        private void OnFastForwardEnabled(Controls.PlayerControls sender, Object args) => Context.EnabledFastForward();

        private void OnFastForwardDisabled(Controls.PlayerControls sender, Object args) => Context.DisableFastForward();

        protected override void OnNavigatedFrom(NavigationEventArgs args)
        {
            base.OnNavigatedFrom(args);
            Context.Dispose();
        }
    }
}
