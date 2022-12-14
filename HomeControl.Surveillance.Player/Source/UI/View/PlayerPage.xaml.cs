using HomeControl.Surveillance.Player.UI.Controller;
using System;
using Windows.Media.Core;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace HomeControl.Surveillance.Player.UI.View
{
    public sealed partial class PlayerPage: Page, IDisposable
    {
        private PlayerController Context = new PlayerController();

        public PlayerPage()
        {
            InitializeComponent();
            DataContext = Context;
        }

        public async void Activate(IStorageItem item)
        {
            if (!(item is StorageFile file))
                return;

            await Context.OpenAsync(file);

            Context.MediaPlayer.Source = MediaSource.CreateFromMediaStreamSource(Context.MediaStream);
            Context.MediaPlayer.AutoPlay = true;
            VideoPlayer.SetMediaPlayer(Context.MediaPlayer);
        }

        public void Dispose()
        {
            Context.Dispose();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs args)
        {
            base.OnNavigatedFrom(args);
            Context.Dispose();
        }
    }
}
