using HomeControl.Surveillance.Player.Model;
using System;

namespace HomeControl.Surveillance.Player.ViewModel
{
    public class CameraContext: IDisposable
    {
        public CameraStream CameraStream { get; private set; }

        public CameraContext() { }

        public void Initialize(CameraController cameraController)
        {
            CameraStream = new CameraStream(cameraController);
        }

        public void Dispose()
        {
            CameraStream.Dispose();
        }
    }
}
