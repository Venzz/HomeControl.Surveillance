using HomeControl.Surveillance.Server.Services.OrientProtocol;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;

namespace HomeControl.Surveillance.Server.Services
{
    public class OrientProtocolCameraConnection: ICameraConnection
    {
        private UInt32 ConnectionId;
        private String IpAddress;
        private UInt16 Port;
        private Object ConnectionSync = new Object();
        private TcpConnection Connection;
        private SessionProperties Session;
        private ReconnectionController Reconnection = new ReconnectionController();

        public Boolean IsZoomingSupported => true;

        public event TypedEventHandler<ICameraConnection, IMediaData> MediaReceived = delegate { };
        public event TypedEventHandler<ICameraConnection, (String, String)> Log = delegate { };
        public event TypedEventHandler<ICameraConnection, (String, String, String)> DetailedLog = delegate { };
        public event TypedEventHandler<ICameraConnection, (String, String, Exception)> Exception = delegate { };



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
                Log(this, ($"{nameof(OrientProtocolCameraConnection)}", "Command StartZoomingIn."));
                await connection.Value.SendAsync(new OpPtzControlZoomRequestMessage(connection.SessionId, 65535, Message.ZoomType.ZoomTile).Serialize()).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Exception(this, ($"{nameof(OrientProtocolCameraConnection)}.{nameof(StartZoomingInAsync)}", null, exception));
            }
        }

        public async Task StartZoomingOutAsync()
        {
            var connection = TryGetConnection();
            if (connection.Value == null)
                return;

            try
            {
                Log(this, ($"{nameof(OrientProtocolCameraConnection)}", "Command StartZoomingOut."));
                await connection.Value.SendAsync(new OpPtzControlZoomRequestMessage(connection.SessionId, 65535, Message.ZoomType.ZoomWide).Serialize()).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Exception(this, ($"{nameof(OrientProtocolCameraConnection)}.{nameof(StartZoomingOutAsync)}", null, exception));
            }
        }

        public async Task StopZoomingAsync()
        {
            var connection = TryGetConnection();
            if (connection.Value == null)
                return;

            try
            {
                Log(this, ($"{nameof(OrientProtocolCameraConnection)}", "Command StopZooming."));
                await connection.Value.SendAsync(new OpPtzControlZoomRequestMessage(connection.SessionId, -1, Message.ZoomType.ZoomTile).Serialize()).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Exception(this, ($"{nameof(OrientProtocolCameraConnection)}.{nameof(StartZoomingOutAsync)}", null, exception));
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
                    Log(this, ($"{nameof(OrientProtocolCameraConnection)}", "Connected."));

                    lock (ConnectionSync)
                    {
                        Connection = connection;
                        Session = new SessionProperties();
                        Reconnection.ResetPermissionGrantedDate();
                        Monitor.PulseAll(ConnectionSync);
                    }
                }
                catch (Exception exception)
                {
                    Exception(this, ($"{nameof(OrientProtocolCameraConnection)}.{nameof(StartConnectionRestorating)}", null, exception));
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
                    if (Reconnection.IsAllowed())
                    {
                        Log(this, ($"{nameof(OrientProtocolCameraConnection)}", $"No data captured, reconnecting... SessionId = {Session.Id:x}"));
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
                    Exception(this, ($"{nameof(OrientProtocolCameraConnection)}.{nameof(StartConnectionMaintaining)}", null, exception));
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
            (TcpConnection Connection, SessionProperties Session) GetVariables()
            {
                lock (ConnectionSync)
                {
                    if (Connection == null)
                        Monitor.Wait(ConnectionSync);
                    return (Connection, Session);
                }
            }

            var variables = GetVariables();
            try
            {
                variables.Session.DataQueue.Enqueue(data);
                while (variables.Session.DataQueue.Length >= 20)
                {
                    var peekedData = variables.Session.DataQueue.Peek(20);
                    var dataSize = peekedData[16] + peekedData[17] * 256;
                    if (variables.Session.DataQueue.Length < dataSize + 20)
                        return;

                    var message = Message.Create(variables.Session.DataQueue.Dequeue(dataSize + 20));
                    switch (message)
                    {
                        case AuthorizationResponseMessage authorizationResponse:
                            Reconnection.ResetPermissionGrantedDate();
                            Log(this, ($"{nameof(OrientProtocolCameraConnection)}: Connection = {sender.Id}", $"{nameof(AuthorizationResponseMessage)}, SessionId = {authorizationResponse.SessionId:x}"));
                            variables.Session.Id = authorizationResponse.SessionId;
                            await variables.Connection.SendAsync(new OpMonitorClaimRequestMessage(authorizationResponse.SessionId).Serialize()).ConfigureAwait(false);
                            break;
                        case OpMonitorClaimResponseMessage claimResponse:
                            Reconnection.ResetPermissionGrantedDate();
                            Log(this, ($"{nameof(OrientProtocolCameraConnection)}: Connection = {sender.Id}", $"{nameof(OpMonitorClaimResponseMessage)}, SessionId = {claimResponse.SessionId:x}"));
                            await variables.Connection.SendAsync(new OpMonitorStartRequestMessage(claimResponse.SessionId).Serialize()).ConfigureAwait(false);
                            break;
                        case MediaDataResponseMessage mediaDataResponse:
                            Reconnection.Reset();
                            OnMediaReceived(variables.Session, mediaDataResponse);
                            break;
                        case UnknownResponseMessage unknownResponse:
                            Log(this, ($"{nameof(OrientProtocolCameraConnection)}: Connection = {sender.Id}", $"UnknownResponse\n{unknownResponse.Data}"));
                            break;
                    }
                }
            }
            catch (Exception exception)
            {
                Exception(this, ($"{nameof(OrientProtocolCameraConnection)}.{nameof(OnDataReceived)}: Connection = {sender.Id}", data.ToHexView(), exception));
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

        private void OnMediaReceived(SessionProperties session, MediaDataResponseMessage mediaDataResponse)
        {
            session.MediaDataQueue.Enqueue(mediaDataResponse.Data);
            while (session.MediaDataQueue.Length >= 16)
            {
                var peekedData = session.MediaDataQueue.Peek(16);
                var operationCode = peekedData[2] * 256 + peekedData[3];
                var dataSize = 0;
                switch (operationCode)
                {
                    case (UInt16)Message.Operation.AudioFrame:
                        dataSize = BitConverter.ToInt16(peekedData, 6);
                        break;
                    case (UInt16)Message.Operation.PredictionFrame:
                        dataSize = BitConverter.ToInt32(peekedData, 4);
                        break;
                    case (UInt16)Message.Operation.InterFrame:
                        dataSize = BitConverter.ToInt32(peekedData, 12);
                        break;
                }

                if (session.MediaDataQueue.Length < dataSize + 16)
                    return;

                var now = DateTime.UtcNow;
                switch (operationCode)
                {
                    case (UInt16)Message.Operation.AudioFrame:
                        if (!session.LastAudioTimestamp.HasValue)
                            session.LastAudioTimestamp = now;
                        var duration = now - session.LastAudioTimestamp.Value;
                        if (duration.TotalMilliseconds == 0)
                            duration = TimeSpan.FromMilliseconds(1.0 / 50);

                        session.LastAudioTimestamp = now;
                        session.MediaDataQueue.Dequeue(8);
                        MediaReceived(this, new AudioMediaData(session.MediaDataQueue.Dequeue(dataSize), now, duration));
                        break;
                    case (UInt16)Message.Operation.PredictionFrame:
                        if (!session.LastVideoTimestamp.HasValue)
                            session.LastVideoTimestamp = now;
                        duration = now - session.LastVideoTimestamp.Value;
                        if (duration.TotalMilliseconds == 0)
                            duration = TimeSpan.FromMilliseconds(1.0 / 12.5);

                        session.LastVideoTimestamp = now;
                        session.MediaDataQueue.Dequeue(8);
                        MediaReceived(this, new PredictionFrameMediaData(session.MediaDataQueue.Dequeue(dataSize), now, duration));
                        break;
                    case (UInt16)Message.Operation.InterFrame:
                        if (!session.LastVideoTimestamp.HasValue)
                            session.LastVideoTimestamp = now;
                        duration = now - session.LastVideoTimestamp.Value;
                        if (duration.TotalMilliseconds == 0)
                            duration = TimeSpan.FromMilliseconds(1.0 / 12.5);

                        session.LastVideoTimestamp = now;
                        session.MediaDataQueue.Dequeue(16);
                        MediaReceived(this, new InterFrameMediaData(session.MediaDataQueue.Dequeue(dataSize), now, duration));
                        break;
                    default:
                        var queuedData = session.MediaDataQueue.Peek(session.MediaDataQueue.Length);
                        DetailedLog(this, ($"{nameof(OrientProtocolCameraConnection)}", $"MediaDataCode = {operationCode}", $"Header = {mediaDataResponse.Header}\n{queuedData.ToHexView()}"));
                        session.MediaDataQueue.Clear();
                        break;
                }
            }
        }

        private (TcpConnection Value, UInt32 SessionId) TryGetConnection()
        {
            lock (ConnectionSync)
            {
                if ((Connection == null) || (Session.Id == 0))
                    return (null, 0);
                return (Connection, Session.Id);
            }
        }



        private class SessionProperties
        {
            public UInt32 Id { get; set; }
            public DataQueue DataQueue { get; }
            public DataQueue MediaDataQueue { get; }
            public DateTime? LastAudioTimestamp { get; set; }
            public DateTime? LastVideoTimestamp { get; set; }

            public SessionProperties()
            {
                Id = 0;
                DataQueue = new DataQueue();
                MediaDataQueue = new DataQueue();
                LastAudioTimestamp = null;
                LastVideoTimestamp = null;
            }
        }
    }
}
