﻿using System;
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
        private (TimeSpan From, TimeSpan Duration)? IdlePeriod;
        private DateTime IdlingStartedDate;
        private Boolean IsIdlingActive => IdlePeriod.HasValue && (DateTime.Now - IdlingStartedDate < IdlePeriod.Value.Duration);

        public event TypedEventHandler<IProviderCameraService, (String Message, String Parameter)> LogReceived = delegate { };
        public event TypedEventHandler<IProviderCameraService, (String Message, Exception Exception)> ExceptionReceived = delegate { };
        public event TypedEventHandler<IProviderCameraService, Command> CommandReceived = delegate { };



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
                        LogReceived(this, ($"{nameof(HerokuProviderCameraService)}", $"IdlePeriod.HasValue = {IdlePeriod.HasValue}, now.TimeOfDay = {now.TimeOfDay}, IdlePeriod.Value.From = {IdlePeriod.Value.From}, !IsIdlingActive = {!IsIdlingActive}"));
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