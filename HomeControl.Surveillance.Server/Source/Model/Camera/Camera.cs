using HomeControl.Surveillance.Data.Camera;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HomeControl.Surveillance.Server.Model
{
    public class Camera
    {
        private const Int32 PendingDataSize = 50;

        private IProviderCameraService ProviderService;
        private Queue<Byte[]> Data = new Queue<Byte[]>(PendingDataSize);
        private Object Sync = new Object();



        public Camera(IProviderCameraService providerService)
        {
            ProviderService = providerService;
            StartSendingCycle();
        }

        public void Send(Byte[] data)
        {
            if ((DateTime.Now.Hour >= 22) || (DateTime.Now.Hour < 6))
                return;

            lock (Sync)
            {
                if (Data.Count == PendingDataSize)
                    Data.Dequeue();

                Data.Enqueue(data);
                Monitor.PulseAll(Sync);
            }
        }

        private async void StartSendingCycle() => await Task.Run(async () =>
        {
            while (true)
            {
                var data = (Byte[])null;
                lock (Sync)
                {
                    if (Data.Count == 0)
                        Monitor.Wait(Sync);

                    data = Data.Dequeue();
                }
                
                await ProviderService.SendAsync(data).ConfigureAwait(false);
            }
        });
    }
}
