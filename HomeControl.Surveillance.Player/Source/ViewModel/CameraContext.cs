using HomeControl.Surveillance.Player.Model;
using System;
using System.Threading.Tasks;
using Venz.Data;

namespace HomeControl.Surveillance.Player.ViewModel
{
    public class CameraContext: ObservableObject, IDisposable
    {
        public CameraController CameraController { get; private set; }
        public CameraStream CameraStream { get; private set; }

        public CameraContext() { }

        public void Initialize(CameraController cameraController)
        {
            CameraController = cameraController;
            CameraStream = new CameraStream(cameraController);
            OnPropertyChanged(nameof(CameraController));
        }

        public async void StartZoomingIn() => await Task.Run(() => CameraController.StartZoomingInAsync());

        public async void StartZoomingOut() => await Task.Run(() => CameraController.StartZoomingOutAsync());

        public async void StopZooming() => await Task.Run(() => CameraController.StopZoomingAsync());

        public void Dispose()
        {
            CameraStream.Dispose();
        }
    }
}
