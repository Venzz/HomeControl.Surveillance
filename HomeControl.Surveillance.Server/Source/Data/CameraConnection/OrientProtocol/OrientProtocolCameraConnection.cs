using HomeControl.Surveillance.Server.Data.Rtsp.Protocol;
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
        private DateTime LastDataTransmissionDate;
        private DataQueue DataQueue = new DataQueue();

        public event TypedEventHandler<ICameraConnection, Byte[]> DataReceived = delegate { };



        public OrientProtocolCameraConnection(String ipAddress, UInt16 port)
        {
            LastDataTransmissionDate = DateTime.Now.AddSeconds(10);
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

                await Task.Delay(2000).ConfigureAwait(false);

                try
                {
                    if ((DateTime.Now - LastDataTransmissionDate) > TimeSpan.FromSeconds(10))
                    {
                        App.Diagnostics.Debug.Log($"{nameof(OrientProtocolCameraConnection)}", "No data captured within 10 second, reconnecting...");
                        lock (ConnectionSync)
                        {
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
                        Connection = null;
                        Monitor.PulseAll(ConnectionSync);
                    }
                }
            }
        });

        private async void OnDataReceived(TcpConnection sender, Byte[] data)
        {
            var connection = (TcpConnection)null;
            lock (ConnectionSync)
            {
                if (Connection == null)
                    Monitor.Wait(ConnectionSync);
                connection = Connection;
            }

            try
            {
                DataQueue.Enqueue(data);
                while (DataQueue.Length >= 20)
                {
                    var peekedData = DataQueue.Peek(20);
                    var dataSize = peekedData[16] + peekedData[17] * 256;
                    if (DataQueue.Length < dataSize + 20)
                        return;

                    var message = Message.Create(DataQueue.Dequeue(dataSize + 20));
                    switch (message)
                    {
                        case AuthorizationResponseMessage authorizationResponse:
                            await connection.SendAsync(new OpMonitorClaimRequestMessage(authorizationResponse.SessionId).Serialize()).ConfigureAwait(false);
                            break;
                        case OpMonitorClaimResponseMessage claimResponse:
                            await connection.SendAsync(new OpMonitorStartRequestMessage(claimResponse.SessionId).Serialize()).ConfigureAwait(false);
                            break;
                        case VideoDataResponseMessage videoDataResponse:
                            LastDataTransmissionDate = DateTime.Now;
                            DataReceived(this, videoDataResponse.Data);
                            break;
                        case UnknownResponseMessage unknownResponse:
                            App.Diagnostics.Debug.Log($"{nameof(OrientProtocolCameraConnection)}.{nameof(OnDataReceived)}", $"UnknownResponse\n{unknownResponse.Data}");
                            break;
                    }
                }
            }
            catch (Exception)
            {
                lock (ConnectionSync)
                {
                    Connection = null;
                    Monitor.PulseAll(ConnectionSync);
                }
            }
        }
    }
}
