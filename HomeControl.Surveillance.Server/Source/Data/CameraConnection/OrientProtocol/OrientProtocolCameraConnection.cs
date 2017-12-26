using HomeControl.Surveillance.Data;
using HomeControl.Surveillance.Server.Data.Tcp;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;

namespace HomeControl.Surveillance.Server.Data.OrientProtocol
{
    public class OrientProtocolCameraConnection: ICameraConnection
    {
        private UInt32 ConnectionId;
        private String IpAddress;
        private UInt16 Port;
        private Object ConnectionSync = new Object();
        private TcpConnection Connection;
        private DataQueue DataQueue;
        private UInt32 SessionId;
        private ReconnectionController ReconnectionController = new ReconnectionController();

        public Boolean IsZoomingSupported => true;

        public event TypedEventHandler<ICameraConnection, Byte[]> DataReceived = delegate { };
        public event TypedEventHandler<ICameraConnection, (String CustomText, Exception Exception)> ExceptionReceived = delegate { };
        public event TypedEventHandler<ICameraConnection, (String CustomText, String Parameter)> LogReceived = delegate { };



        public OrientProtocolCameraConnection(String ipAddress, UInt16 port)
        {
            IpAddress = ipAddress;
            Port = port;
            StartConnectionRestorating();
            StartConnectionMaintaining();
        }

        public async Task StartZoomingInAsync()
        {
            var connection = TryGetConnection();
            if (connection.Value == null)
                return;

            try
            {
                LogReceived(this, ($"{nameof(OrientProtocolCameraConnection)}", "Command StartZoomingIn."));
                await connection.Value.SendAsync(new OpPtzControlZoomRequestMessage(connection.SessionId, 65535, Message.ZoomType.ZoomTile).Serialize()).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                App.Diagnostics.Debug.Log($"{nameof(OrientProtocolCameraConnection)}.{nameof(StartZoomingInAsync)}", exception);
            }
        }

        public async Task StartZoomingOutAsync()
        {
            var connection = TryGetConnection();
            if (connection.Value == null)
                return;

            try
            {
                LogReceived(this, ($"{nameof(OrientProtocolCameraConnection)}", "Command StartZoomingOut."));
                await connection.Value.SendAsync(new OpPtzControlZoomRequestMessage(connection.SessionId, 65535, Message.ZoomType.ZoomWide).Serialize()).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                App.Diagnostics.Debug.Log($"{nameof(OrientProtocolCameraConnection)}.{nameof(StartZoomingInAsync)}", exception);
            }
        }

        public async Task StopZoomingAsync()
        {
            var connection = TryGetConnection();
            if (connection.Value == null)
                return;

            try
            {
                LogReceived(this, ($"{nameof(OrientProtocolCameraConnection)}", "Command StopZooming."));
                await connection.Value.SendAsync(new OpPtzControlZoomRequestMessage(connection.SessionId, -1, Message.ZoomType.ZoomTile).Serialize()).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                App.Diagnostics.Debug.Log($"{nameof(OrientProtocolCameraConnection)}.{nameof(StartZoomingInAsync)}", exception);
            }
        }

        private async void StartConnectionRestorating() => await Task.Run(async () =>
        {
            while (true)
            {
                lock (ConnectionSync)
                {
                    if (Connection != null)
                        Monitor.Wait(ConnectionSync);
                }

                try
                {
                    var connection = new TcpConnection(++ConnectionId, IpAddress, Port);
                    connection.DataReceived += OnDataReceived;
                    await connection.SendAsync(new AuthorizationRequestMessage().Serialize()).ConfigureAwait(false);
                    LogReceived(this, ($"{nameof(OrientProtocolCameraConnection)}", "Connected."));

                    lock (ConnectionSync)
                    {
                        Connection = connection;
                        DataQueue = new DataQueue();
                        ReconnectionController.ResetPermissionGrantedDate();
                        SessionId = 0;
                        Monitor.PulseAll(ConnectionSync);
                    }
                }
                catch (Exception exception)
                {
                    ExceptionReceived(this, ($"{nameof(OrientProtocolCameraConnection)}.{nameof(StartConnectionRestorating)}", exception));
                }
            }
        });

        private async void StartConnectionMaintaining() => await Task.Run(async () =>
        {
            while (true)
            {
                lock (ConnectionSync)
                {
                    if (Connection == null)
                        Monitor.Wait(ConnectionSync);
                }

                try
                {
                    await Task.Delay(2000).ConfigureAwait(false);
                    if (ReconnectionController.IsAllowed())
                    {
                        LogReceived(this, ($"{nameof(OrientProtocolCameraConnection)}", $"No data captured, reconnecting... SessionId = {SessionId:x}"));
                        lock (ConnectionSync)
                        {
                            Connection.DataReceived -= OnDataReceived;
                            Connection.Dispose();
                            Connection = null;
                            Monitor.PulseAll(ConnectionSync);
                            continue;
                        }
                    }
                }
                catch (Exception exception)
                {
                    ExceptionReceived(this, ($"{nameof(OrientProtocolCameraConnection)}.{nameof(StartConnectionMaintaining)}", exception));
                    lock (ConnectionSync)
                    {
                        Connection.DataReceived -= OnDataReceived;
                        Connection.Dispose();
                        Connection = null;
                        Monitor.PulseAll(ConnectionSync);
                    }
                }
            }
        });

        private async void OnDataReceived(TcpConnection sender, Byte[] data)
        {
            (TcpConnection Connection, DataQueue DataQueue) GetVariables()
            {
                lock (ConnectionSync)
                {
                    if (Connection == null)
                        Monitor.Wait(ConnectionSync);
                    return (Connection, DataQueue);
                }
            }

            var variables = GetVariables();
            try
            {
                variables.DataQueue.Enqueue(data);
                while (variables.DataQueue.Length >= 20)
                {
                    var peekedData = variables.DataQueue.Peek(20);
                    var dataSize = peekedData[16] + peekedData[17] * 256;
                    if (variables.DataQueue.Length < dataSize + 20)
                        return;

                    var message = Message.Create(variables.DataQueue.Dequeue(dataSize + 20));
                    switch (message)
                    {
                        case AuthorizationResponseMessage authorizationResponse:
                            ReconnectionController.ResetPermissionGrantedDate();
                            LogReceived(this, ($"{nameof(OrientProtocolCameraConnection)}: Connection = {sender.Id}", $"{nameof(AuthorizationResponseMessage)}, SessionId = {authorizationResponse.SessionId:x}"));
                            SessionId = authorizationResponse.SessionId;
                            await variables.Connection.SendAsync(new OpMonitorClaimRequestMessage(authorizationResponse.SessionId).Serialize()).ConfigureAwait(false);
                            break;
                        case OpMonitorClaimResponseMessage claimResponse:
                            ReconnectionController.ResetPermissionGrantedDate();
                            LogReceived(this, ($"{nameof(OrientProtocolCameraConnection)}: Connection = {sender.Id}", $"{nameof(OpMonitorClaimResponseMessage)}, SessionId = {claimResponse.SessionId:x}"));
                            await variables.Connection.SendAsync(new OpMonitorStartRequestMessage(claimResponse.SessionId).Serialize()).ConfigureAwait(false);
                            break;
                        case VideoDataResponseMessage videoDataResponse:
                            ReconnectionController.Reset();
                            DataReceived(this, videoDataResponse.Data);
                            break;
                        case UnknownResponseMessage unknownResponse:
                            LogReceived(this, ($"{nameof(OrientProtocolCameraConnection)}: Connection = {sender.Id}", $"UnknownResponse\n{unknownResponse.Data}"));
                            break;
                    }
                }
            }
            catch (Exception exception)
            {
                ExceptionReceived(this, ($"{nameof(OrientProtocolCameraConnection)}.{nameof(OnDataReceived)}: Connection = {sender.Id}\n{data.ToHexView()}", exception));
                lock (ConnectionSync)
                {
                    if (Connection == variables.Connection)
                    {
                        Connection.DataReceived -= OnDataReceived;
                        Connection.Dispose();
                        Connection = null;
                        Monitor.PulseAll(ConnectionSync);
                    }
                }
            }
        }

        private (TcpConnection Value, UInt32 SessionId) TryGetConnection()
        {
            lock (ConnectionSync)
            {
                if ((Connection == null) || (SessionId == 0))
                    return (null, 0);
                return (Connection, SessionId);
            }
        }
    }
}
