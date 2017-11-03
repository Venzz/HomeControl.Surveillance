using System;

namespace HomeControl.Surveillance.Player.ViewModel
{
    public class HubContext: IDisposable
    {
        public CameraStream OutdoorCameraStream { get; private set; }
        public CameraStream IndoorCameraStream { get; private set; }



        public HubContext()
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
