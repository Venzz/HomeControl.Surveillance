using HomeControl.Surveillance.Server.Data.Tcp;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;

namespace HomeControl.Surveillance.Server.Data.OrientProtocol
{
    public class OrientProtocolCameraConnection: ICameraConnection
    {
        private String IpAddress;
        private UInt16 Port;
        private Object ConnectionSync = new Object();
        private TcpConnection Connection;
        private DataQueue DataQueue;
        private ReconnectionController ReconnectionController = new ReconnectionController();

        public event TypedEventHandler<ICameraConnection, Byte[]> DataReceived = delegate { };



        public OrientProtocolCameraConnection(String ipAddress, UInt16 port)
        {
            IpAddress = ipAddress;
            Port = port;
            StartConnectionRestorating();
            StartConnectionMaintaining();
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
                    var connection = new TcpConnection(IpAddress, Port);
                    connection.DataReceived += OnDataReceived;
                    await connection.SendAsync(new AuthorizationRequestMessage().Serialize()).ConfigureAwait(false);
                    App.Diagnostics.Debug.Log($"{nameof(OrientProtocolCameraConnection)}", "Connected.");

                    lock (ConnectionSync)
                    {
                        Connection = connection;
                        DataQueue = new DataQueue();
                        ReconnectionController.ResetPermissionGrantedDate();
                        Monitor.PulseAll(ConnectionSync);
                    }
                }
                catch (Exception exception)
                {
                    App.Diagnostics.Debug.Log($"{nameof(OrientProtocolCameraConnection)}.{nameof(StartConnectionRestorating)}", exception);
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
                        App.Diagnostics.Debug.Log($"{nameof(OrientProtocolCameraConnection)}", "No data captured, reconnecting...");
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
                    App.Diagnostics.Debug.Log($"{nameof(OrientProtocolCameraConnection)}.{nameof(StartConnectionMaintaining)}", exception);
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
                            App.Diagnostics.Debug.Log($"{nameof(AuthorizationResponseMessage)}: SessionId = {authorizationResponse.SessionId:x}");
                            await variables.Connection.SendAsync(new OpMonitorClaimRequestMessage(authorizationResponse.SessionId).Serialize()).ConfigureAwait(false);
                            break;
                        case OpMonitorClaimResponseMessage claimResponse:
                            App.Diagnostics.Debug.Log($"{nameof(OpMonitorClaimResponseMessage)}: SessionId = {claimResponse.SessionId:x}");
                            await variables.Connection.SendAsync(new OpMonitorStartRequestMessage(claimResponse.SessionId).Serialize()).ConfigureAwait(false);
                            break;
                        case VideoDataResponseMessage videoDataResponse:
                            ReconnectionController.Reset();
                            DataReceived(this, videoDataResponse.Data);
                            break;
                        case UnknownResponseMessage unknownResponse:
                            App.Diagnostics.Debug.Log($"{nameof(OrientProtocolCameraConnection)}.{nameof(OnDataReceived)}", $"UnknownResponse\n{unknownResponse.Data}");
                            break;
                    }
                }
            }
            catch (Exception exception)
            {
                App.Diagnostics.Debug.Log($"{nameof(OrientProtocolCameraConnection)}.{nameof(OnDataReceived)}: {data.ToHexView()}", exception);
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
    }
}
