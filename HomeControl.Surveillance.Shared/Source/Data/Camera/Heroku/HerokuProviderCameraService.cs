using System;
using System.IO;
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
        public event TypedEventHandler<IProviderCameraService, Command> CommandReceived = delegate { };



        public HerokuProviderCameraService(String serviceName)
        {
            ServiceName = serviceName;
            StartConnectionMaintaining();
            StartReconnectionMaintaining();
            StartReceiving();
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

                await socket.SendAsync(CreateMessage(data)).ConfigureAwait(false);
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
        
        private Byte[] CreateMessage(Byte[] data)
        {
            var message = new Byte[8 + data.Length];
            using (var packetStream = new MemoryStream(8 + data.Length))
            using (var writer = new BinaryWriter(packetStream))
            {
                writer.Write(new Byte[] { 0xFF, 0xFF, 0xFF, 0xFF });
                writer.Write(data.Length);
                writer.Write(data);
                packetStream.Position = 0;
                packetStream.Read(message, 0, (Int32)packetStream.Length);
            }
            return message;
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

        private async void StartReceiving() => await Task.Run(async () =>
        {
            var buffer = new ArraySegment<Byte>(new Byte[1024 * 1024]);
            while (true)
            {
                var socket = (IWebSocket)null;
                lock (ConnectionSync)
                {
                    if (WebSocket == null)
                        Monitor.Wait(ConnectionSync);
                    socket = WebSocket;
                }

                try
                {
                    var result = await socket.ReceiveAsync(buffer).ConfigureAwait(false);
                    var data = new Byte[result.Count];
                    Array.Copy(buffer.Array, data, result.Count);
                    if ((data.Length == 9) && (data[0] == 0xFF) && (data[1] == 0xFF) && (data[2] == 0xFF) && (data[3] == 0xFF))
                        CommandReceived(this, (Command)data[8]);
                }
                catch (Exception exception)
                {
                    ExceptionReceived(this, ($"{nameof(HerokuConsumerCameraService)}.{nameof(StartReceiving)}", exception));
                    try { await socket.CloseAsync().ConfigureAwait(false); } catch (Exception) { }
                    try { socket.Abort(); } catch (Exception) { }
                    lock (ConnectionSync)
                    {
                        if (WebSocket == socket)
                        {
                            WebSocket = null;
                            Monitor.PulseAll(ConnectionSync);
                        }
                    }
                }
            }
        });
    }
}