using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;

namespace HomeControl.Surveillance.Data.Camera.Heroku
{
    public class HerokuConsumerCameraService: IConsumerCameraService
    {
        private Object ConnectionSync = new Object();
        private IWebSocket WebSocket;
        private String ServiceName;

        public event TypedEventHandler<IConsumerCameraService, Byte[]> DataReceived = delegate { };
        public event TypedEventHandler<IConsumerCameraService, (String Message, String Parameter)> LogReceived = delegate { };
        public event TypedEventHandler<IConsumerCameraService, (String Message, Exception Exception)> ExceptionReceived = delegate { };



        public HerokuConsumerCameraService(String serviceName)
        {
            ServiceName = serviceName;
            StartConnectionMaintaining();
            StartReceiving();
        }

        public async Task PerformAsync(Command command)
        {
            try
            {
                var webSocket = WebSocket;
                if (webSocket == null)
                    return;

                await webSocket.SendAsync(new Byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0x01, 0x00, 0x00, 0x00, (Byte)command }).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                ExceptionReceived(this, ($"{nameof(HerokuConsumerCameraService)}.{nameof(PerformAsync)}", exception));
            }
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
                    LogReceived(this, ($"{nameof(HerokuConsumerCameraService)}", "Connected."));

                    lock (ConnectionSync)
                    {
                        WebSocket = webSocket;
                        Monitor.PulseAll(ConnectionSync);
                    }
                }
                catch (Exception exception)
                {
                    ExceptionReceived(this, ($"{nameof(HerokuConsumerCameraService)}.{nameof(StartConnectionMaintaining)}", exception));
                }
            }
        });

        private async void StartReceiving() => await Task.Run(async () =>
        {
            var buffer = new ArraySegment<Byte>(new Byte[1024 * 1024]);
            while (true)
            {
                lock (ConnectionSync)
                {
                    if (WebSocket == null)
                        Monitor.Wait(ConnectionSync);
                }

                try
                {
                    var result = await WebSocket.ReceiveAsync(buffer).ConfigureAwait(false);
                    var data = new Byte[result.Count];
                    Array.Copy(buffer.Array, data, result.Count);
                    DataReceived(this, data);
                }
                catch (Exception exception)
                {
                    ExceptionReceived(this, ($"{nameof(HerokuConsumerCameraService)}.{nameof(StartReceiving)}", exception));
                    try { await WebSocket.CloseAsync().ConfigureAwait(false); } catch (Exception) { }
                    try { WebSocket.Abort(); } catch (Exception) { }
                    lock (ConnectionSync)
                    {
                        WebSocket = null;
                        Monitor.PulseAll(ConnectionSync);
                    }
                }
            }
        });
    }
}