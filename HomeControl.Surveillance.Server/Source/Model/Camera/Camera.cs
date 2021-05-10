using HomeControl.Surveillance.Server.Services;
using HomeControl.Surveillance.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;

namespace HomeControl.Surveillance.Server.Model
{
    public class Camera
    {
        private const Int32 PendingDataSize = 50;

        private IProviderCameraService ProviderService;
        private Queue<IMediaData> Data = new Queue<IMediaData>(PendingDataSize);
        private Object Sync = new Object();

        public Boolean IsProviderCommunicationEnabled { get; set; } = true;
        public DateTime SendingStartedDate { get; } = DateTime.Now;
        public UInt32 LastSentDataLength { get; private set; }
        public UInt32 TotalSentDataLength { get; private set; }

        public event TypedEventHandler<Camera, (String CustomText, Exception Exception)> ExceptionReceived = delegate { };
        public event TypedEventHandler<Camera, Command> CommandReceived = delegate { };



        public Camera(IProviderCameraService providerService)
        {
            ProviderService = providerService;
            StartSendingCycle();
            ProviderService.CommandReceived += (sender, command) => CommandReceived(this, command);
        }

        public void Send(IMediaData mediaData)
        {
            try
            {
                LastSentDataLength = (UInt32)mediaData.Data.Length;
                TotalSentDataLength += (UInt32)mediaData.Data.Length;

                if ((DateTime.Now.Hour >= 22) || (DateTime.Now.Hour < 6))
                    return;

                lock (Sync)
                {
                    if (Data.Count == PendingDataSize)
                        Data.Dequeue();

                    Data.Enqueue(mediaData);
                    Monitor.PulseAll(Sync);
                }
            }
            catch (Exception exception)
            {
                ExceptionReceived(this, ($"{nameof(Camera)}.{nameof(Send)}", exception));
            }
        }

        private async void StartSendingCycle() => await Task.Run(async () =>
        {
            while (true)
            {
                var data = (IMediaData)null;
                lock (Sync)
                {
                    if (Data.Count == 0)
                        Monitor.Wait(Sync);

                    data = Data.Dequeue();
                }
                
                if (IsProviderCommunicationEnabled)
                    await ProviderService.SendLiveMediaDataAsync(data.MediaDataType, data.Data, data.Timestamp, data.Duration).ConfigureAwait(false);
            }
        });
    }
}
