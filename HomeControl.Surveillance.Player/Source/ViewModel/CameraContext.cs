using HomeControl.Surveillance.Player.Model;
using System;
using System.Threading.Tasks;
using Venz.Data;
using Windows.UI.Core;
using Windows.UI.StartScreen;
using Windows.UI.Xaml.Controls;

namespace HomeControl.Surveillance.Player.ViewModel
{
    public class CameraContext: ObservableObject, IDisposable
    {
        public CameraController CameraController { get; private set; }
        public CameraStream CameraStream { get; private set; }
        public Boolean IsTilePinned { get; private set; }
        public IconElement TileIcon => new SymbolIcon(IsTilePinned ? Symbol.UnPin : Symbol.Pin);

        public CameraContext() { }

        public void Initialize(CameraController cameraController, CoreDispatcher dispatcher)
        {
            OverrideDispatcher(dispatcher);
            CameraController = cameraController;
            CameraStream = new CameraStream(cameraController);
            IsTilePinned = SecondaryTile.Exists(CameraController.Id);
            OnPropertyChanged(nameof(CameraController), nameof(IsTilePinned));
        }

        public async void StartZoomingIn() => await Task.Run(() => CameraController.StartZoomingInAsync());

        public async void StartZoomingOut() => await Task.Run(() => CameraController.StartZoomingOutAsync());

        public async void StopZooming() => await Task.Run(() => CameraController.StopZoomingAsync());

        public async Task ManageTileAsync()
        {
            if (IsTilePinned)
            {
                var cameraTile = new SecondaryTile(CameraController.Id);
                IsTilePinned = !await cameraTile.RequestDeleteAsync();
            }
            else
            {
                var cameraTile = new SecondaryTile(CameraController.Id, CameraController.Title, CameraController.Id, new Uri("ms-appx:///Resources/Package/Square_150x150.png"), TileSize.Default);
                IsTilePinned = await cameraTile.RequestCreateAsync();
            }
            OnPropertyChanged(nameof(TileIcon));
        }

        public void Dispose()
        {
            CameraStream.Dispose();
        }
    }
}
