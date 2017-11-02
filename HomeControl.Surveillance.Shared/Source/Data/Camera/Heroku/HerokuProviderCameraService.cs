using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;

namespace HomeControl.Surveillance.Data.Camera.Heroku
{
    public class HerokuProviderCameraService: IProviderCameraService
    {
        private Object ConnectionSync = new Object();
        private IWebSocket WebSocket;
        private String ServiceName;

        public event TypedEventHandler<IProviderCameraService, (String Message, String Parameter)> LogReceived = delegate { };
        public event TypedEventHandler<IProviderCameraService, (String Message, Exception Exception)> ExceptionReceived = delegate { };



        public HerokuProviderCameraService(String serviceName)
        {
            ServiceName = serviceName;
            StartConnectionMaintaining();
            StartReconnectionMaintaining();
        }

        private async void StartConnectionMaintaining() => await Task.Run(async () =>
        {
            while (true)
            {
                lock (ConnectionSync)
                {
                    if (WebSocket != null)
                        Monitor.Wait(ConnectionSync);
                }

                try
                {
                    var webSocket = new WebSocket();
                    await webSocket.ConnectAsync($"{PrivateData.HerokuServiceUrl}/{ServiceName}/").ConfigureAwait(false);
                    LogReceived(this, ($"{nameof(HerokuProviderCameraService)}", "Connected."));

                    lock (ConnectionSync)
                    {
                        WebSocket = webSocket;
                        Monitor.PulseAll(ConnectionSync);
                    }
                }
                catch (Exception exception)
                {
                    ExceptionReceived(this, ($"{nameof(HerokuProviderCameraService)}.{nameof(StartConnectionMaintaining)}", exception));
                }
            }
        });

        public Task SendAsync(Byte[] data) => SendAsync(WebSocket, data);

        private async Task SendAsync(IWebSocket socket, Byte[] data)
        {
            try
            {
                if (socket == null)
                    return;

                await socket.SendAsync(data).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                ExceptionReceived(this, ($"{nameof(HerokuProviderCameraService)}.{nameof(SendAsync)}", exception));
                try { await socket.CloseAsync().ConfigureAwait(false); } catch (Exception) { }
                try { socket.Abort(); } catch (Exception) { }
                lock (ConnectionSync)
                {
                    if (socket == WebSocket)
                    {
                        WebSocket = null;
                        Monitor.PulseAll(ConnectionSync);
                    }
                }
            }
        }

        private async void StartReconnectionMaintaining() => await Task.Run(async () =>
        {
            while (true)
            {
                lock (ConnectionSync)
                {
                    if (WebSocket == null)
                        Monitor.Wait(ConnectionSync);
                }

                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(5));
                    lock (ConnectionSync)
                    {
                        if (WebSocket != null)
                        {
                            WebSocket = null;
                            Monitor.PulseAll(ConnectionSync);
                        }
                    }
                }
                catch (Exception exception)
                {
                    ExceptionReceived(this, ($"{nameof(HerokuProviderCameraService)}.{nameof(StartReconnectionMaintaining)}", exception));
                }
            }
        });
    }
}