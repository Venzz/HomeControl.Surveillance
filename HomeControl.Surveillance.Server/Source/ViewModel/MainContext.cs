using System;
using System.Threading.Tasks;
using Venz.Data;

namespace HomeControl.Surveillance.Server.ViewModel
{
    public class MainContext: ObservableObject
    {
        private Boolean _SendOutdoorCameraData = true;
        private Boolean _SendIndoorCameraData = true;

        public Boolean SendOutdoorCameraData { get { return _SendOutdoorCameraData; } set { _SendOutdoorCameraData = value; OnSendOutdoorCameraDataChanged(value); } }
        public Boolean SendIndoorCameraData { get { return _SendIndoorCameraData; } set { _SendIndoorCameraData = value; OnSendIndoorCameraDataChanged(value); } }

        public String OutdoorCameraLastSentDataLength { get; private set; }
        public String OutdoorCameraTransferRate { get; private set; }
        public String IndoorCameraLastSentDataLength { get; private set; }
        public String IndoorCameraTransferRate { get; private set; }



        public MainContext()
        {
            StartTrafficMonitoring();
        }

        private void OnSendOutdoorCameraDataChanged(Boolean send)
        {
            App.Model.OutdoorCamera.IsProviderCommunicationEnabled = send;
        }

        private void OnSendIndoorCameraDataChanged(Boolean send)
        {
            App.Model.IndoorCamera.IsProviderCommunicationEnabled = send;
        }

        private async void StartTrafficMonitoring() => await Task.Run(async () =>
        {
            while (true)
            {
                try
                {
                    var outdoorLastSentDataLength = (Double)App.Model.OutdoorCamera.LastSentDataLength / 1024;
                    var outdoorTransferRate = (Double)App.Model.OutdoorCamera.TotalSentDataLength / 1024 * 8 / (DateTime.Now - App.Model.OutdoorCamera.SendingStartedDate).TotalSeconds;
                    OutdoorCameraLastSentDataLength = $"{outdoorLastSentDataLength:F1} KB";
                    OutdoorCameraTransferRate = $"{outdoorTransferRate:F1} Kb/s";

                    var indoorLastSentDataLength = (Double)App.Model.IndoorCamera.LastSentDataLength / 1024;
                    var indoorTransferRate = (Double)App.Model.IndoorCamera.TotalSentDataLength / 1024 * 8 / (DateTime.Now - App.Model.IndoorCamera.SendingStartedDate).TotalSeconds;
                    IndoorCameraLastSentDataLength = $"{indoorLastSentDataLength:F1} KB";
                    IndoorCameraTransferRate = $"{indoorTransferRate:F1} Kb/s";

                    OnPropertyChanged(nameof(OutdoorCameraLastSentDataLength), nameof(OutdoorCameraTransferRate), nameof(IndoorCameraLastSentDataLength), nameof(IndoorCameraTransferRate));
                    await Task.Delay(1000);
                }
                catch (Exception)
                {
                }
            }
        });
    }
}
