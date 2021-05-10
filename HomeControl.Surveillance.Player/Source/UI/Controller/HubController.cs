using System;

namespace HomeControl.Surveillance.Player.UI.Controller
{
    public class HubController: IDisposable
    {
        public CameraStream OutdoorCameraStream { get; private set; }
        public CameraStream IndoorCameraStream { get; private set; }



        public HubController()
        {
            OutdoorCameraStream = new CameraStream(App.Model.OutdoorCameraController);
            IndoorCameraStream = new CameraStream(App.Model.IndoorCameraController);
        }

        public void Dispose()
        {
            OutdoorCameraStream.Dispose();
            OutdoorCameraStream.Dispose();
        }
    }
}
