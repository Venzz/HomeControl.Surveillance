using System;

namespace HomeControl.Surveillance.Server.ViewModel
{
    public class MainContext
    {
        private Boolean _SendOutdoorCameraData = true;
        private Boolean _SendIndoorCameraData = true;

        public Boolean SendOutdoorCameraData { get { return _SendOutdoorCameraData; } set { _SendOutdoorCameraData = value; OnSendOutdoorCameraDataChanged(value); } }
        public Boolean SendIndoorCameraData { get { return _SendIndoorCameraData; } set { _SendIndoorCameraData = value; OnSendIndoorCameraDataChanged(value); } }

        private void OnSendOutdoorCameraDataChanged(Boolean send)
        {
            App.Model.OutdoorCamera.IsProviderCommunicationEnabled = send;
        }

        private void OnSendIndoorCameraDataChanged(Boolean send)
        {
            App.Model.IndoorCamera.IsProviderCommunicationEnabled = send;
        }
    }
}
