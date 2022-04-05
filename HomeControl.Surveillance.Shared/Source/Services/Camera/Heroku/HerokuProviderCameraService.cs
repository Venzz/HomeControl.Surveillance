using HomeControl.Surveillance.Services.Heroku;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;

namespace HomeControl.Surveillance.Services
{
    public class HerokuProviderCameraService: IProviderCameraService
    {
        private const UInt32 MaximumMessageSize = 100000;

        private Object ConnectionSync = new Object();
        private IWebSocket WebSocket;
        private String ServiceName;
        private (TimeSpan From, TimeSpan Duration)? IdlePeriod;
        private DateTime IdlingStartedDate;
        private Boolean IsIdlingActive => IdlePeriod.HasValue && (DateTime.Now - IdlingStartedDate < IdlePeriod.Value.Duration);

        public event TypedEventHandler<IProviderCameraService, Command> CommandReceived = delegate { };
        public event TypedEventHandler<IProviderCameraService, (UInt32 ConsumerId, UInt32 Id, IMessage Message)> MessageReceived = delegate { };
        public event TypedEventHandler<IProviderCameraService, (String Message, String Parameter)> LogReceived = delegate { };
        public event TypedEventHandler<IProviderCameraService, (String Message, Exception Exception)> ExceptionReceived = delegate { };



        public HerokuProviderCameraService(String serviceName, (TimeSpan From, TimeSpan Duration)? idlePeriod)
        {
            ServiceName = serviceName;
            IdlePeriod = idlePeriod;
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

                if (IsIdlingActive)
                {
                    await Task.Delay(TimeSpan.FromMinutes(1)).ConfigureAwait(false);
                }
                else
                {
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
            }
        });
        
        private Task SendAsync(Message message) => SendAsync(WebSocket, message);

        private async Task SendAsync(IWebSocket socket, Message message)
        {
            try
            {
                if (socket == null || IsIdlingActive)
                    return;

                if (message.Data.Length >= MaximumMessageSize)
                {
                    var packetsCount = (message.Data.Length / MaximumMessageSize) + (message.Data.Length % MaximumMessageSize > 0 ? 1 : 0);
                    for (var i = 0; i < packetsCount; i++)
                    {
                        var packetData = new Byte[((i == packetsCount - 1) && (message.Data.Length % MaximumMessageSize > 0)) ? message.Data.Length % MaximumMessageSize : MaximumMessageSize];
                        Array.Copy(message.Data, i * (Int32)MaximumMessageSize, packetData, 0, packetData.Length);
                        var partialMessageResponse = new PartialMessageResponse(i == packetsCount - 1, packetData);
                        await socket.SendAsync(new Message(message.ConsumerId, message.Id, partialMessageResponse).Data).ConfigureAwait(false);
                    }
                }
                else
                {
                    await socket.SendAsync(message.Data).ConfigureAwait(false);
                }
            }
            catch (Exception exception)
            {
                ExceptionReceived(this, ($"{nameof(HerokuProviderCameraService)}.{nameof(SendAsync)}", exception));
                await CloseSocketAsync(socket).ConfigureAwait(false);

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
                var socket = (IWebSocket)null;
                lock (ConnectionSync)
                {
                    if (WebSocket == null)
                        Monitor.Wait(ConnectionSync);
                    socket = WebSocket;
                }

                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(5));
                    await CloseSocketAsync(socket).ConfigureAwait(false);

                    lock (ConnectionSync)
                    {
                        var now = DateTime.Now;
                        if (IdlePeriod.HasValue && (now.TimeOfDay > IdlePeriod.Value.From) && !IsIdlingActive)
                        {
                            IdlingStartedDate = new DateTime(now.Year, now.Month, now.Day, IdlePeriod.Value.From.Hours, IdlePeriod.Value.From.Minutes, IdlePeriod.Value.From.Seconds, 0, now.Kind);
                            LogReceived(this, ($"{nameof(HerokuProviderCameraService)}", "Idling started."));
                        }

                        if ((WebSocket == socket) && (WebSocket != null) && !IsIdlingActive)
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

        public void EnsureConnected()
        {
            lock (ConnectionSync)
            {
                if (WebSocket == null)
                    Monitor.Wait(ConnectionSync);
            }
        }

        public Task SendStoredRecordsMetadataAsync(UInt32 consumerId, UInt32 id, IReadOnlyCollection<(String Id, DateTime Date)> storedRecordsMetadata)
        {
            var response = new StoredRecordsMetadataResponse(storedRecordsMetadata);
            var responseMessage = new Message(consumerId, id, response);
            return SendAsync(responseMessage);
        }

        public Task SendLiveMediaDataAsync(MediaDataType type, Byte[] data, DateTime timestamp, TimeSpan duration)
        {
            var response = new LiveMediaDataResponse(type, data, timestamp, duration);
            var responseMessage = new Message(0, 0, response);
            return SendAsync(responseMessage);
        }

        public Task SendMediaDataDescriptorsAsync(UInt32 consumerId, UInt32 id, IReadOnlyCollection<StoredRecordFile.MediaDataDescriptor> descriptors)
        {
            var response = new StoredRecordMediaDescriptorsResponse(descriptors);
            var responseMessage = new Message(consumerId, id, response);
            return SendAsync(responseMessage);
        }

        public Task SendMediaDataAsync(UInt32 consumerId, UInt32 id, Byte[] data)
        {
            var response = new StoredRecordMediaDataResponse(data);
            var responseMessage = new Message(consumerId, id, response);
            return SendAsync(responseMessage);
        }

        public Task SetPushChannelSettingsAsync(String clientId, String clientSecret)
        {
            var serviceMessage = new PushChannelSettings(clientId, clientSecret);
            var message = new Message(serviceMessage);
            return SendAsync(message);
        }

        public Task SetPushMessageAsync(String content)
        {
            var serviceMessage = new PushNotification(content);
            var message = new Message(serviceMessage);
            return SendAsync(message);
        }

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
                    {
                        CommandReceived(this, (Command)data[8]);
                    }
                    else if ((data.Length > 12) && (data[0] == 0xFF) && (data[1] == 0xFF) && (data[2] == 0xFF) && (data[3] == 0xFE))
                    {
                        var message = new Message(data);
                        MessageReceived(this, (message.ConsumerId, message.Id, Message.Create(message)));
                    }
                }
                catch (Exception exception)
                {
                    ExceptionReceived(this, ($"{nameof(HerokuProviderCameraService)}.{nameof(StartReceiving)}", exception));
                    await CloseSocketAsync(socket).ConfigureAwait(false);

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

        private async Task CloseSocketAsync(IWebSocket socket)
        {
            try { await socket.CloseAsync().ConfigureAwait(false); } catch (Exception) { }
            try { socket.Abort(); } catch (Exception) { }
        }
    }
}