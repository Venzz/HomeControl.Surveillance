using HomeControl.Surveillance.Server.Services.OrientProtocol;
using System;
using System.Collections.Generic;
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
            var incomingStartsWithPackage = StartsWithFrame(mediaDataResponse.Data);
            if (session.MediaDataQueue.Length > 0 && incomingStartsWithPackage)
            {
                // get package size, fill with zeros, create.
                // corrupted data

                foreach (var mediaFrame in ExtractMediaFrames(session.MediaDataQueue, complementWithZeros: true))
                    MediaReceived(this, mediaFrame);

                session.MediaDataQueue.Clear();
                session.MediaDataQueue.Enqueue(mediaDataResponse.Data);


                Log(this, ($"{nameof(OrientProtocolCameraConnection)}", $"Corrupted Data."));
            }
            else if (session.MediaDataQueue.Length > 0 && !incomingStartsWithPackage)
            {
                var startsWithPackage = StartsWithFrame(session.MediaDataQueue.Peek(4));
                if (startsWithPackage)
                {
                    session.MediaDataQueue.Enqueue(mediaDataResponse.Data);
                }
                else
                {
                    // get package size, fill with zeros, create.
                    // corrupted data
                    foreach (var mediaFrame in ExtractMediaFrames(session.MediaDataQueue, complementWithZeros: true))
                        MediaReceived(this, mediaFrame);

                    session.MediaDataQueue.Clear();
                    Log(this, ($"{nameof(OrientProtocolCameraConnection)}", $"Corrupted Data."));
                }
            }
            else if (session.MediaDataQueue.Length == 0 && incomingStartsWithPackage)
            {
                session.MediaDataQueue.Enqueue(mediaDataResponse.Data);
            }
            else if (session.MediaDataQueue.Length == 0 && !incomingStartsWithPackage)
            {
                for (var i = 4; i < mediaDataResponse.Data.Length - 8; i++)
                {
                    if (mediaDataResponse.Data[i] == 0x00 && mediaDataResponse.Data[i + 1] == 0x00 && mediaDataResponse.Data[i + 2] == 0x01)
                    {
                        if (mediaDataResponse.Data[i + 3] == 0xFA || mediaDataResponse.Data[i + 3] == 0xFC || mediaDataResponse.Data[i + 3] == 0xFD)
                        {
                            session.MediaDataQueue.Enqueue(mediaDataResponse.Data, i, mediaDataResponse.Data.Length - i);
                            break;
                        }
                    }
                }
            }

            foreach (var mediaFrame in ExtractMediaFrames(session.MediaDataQueue, complementWithZeros: false))
                MediaReceived(this, mediaFrame);
        }

        private void EnqueueData(SessionProperties session, MediaDataResponseMessage mediaDataResponse)
        {
            if ((session.LastSequenceNumber != 0) && (mediaDataResponse.SequenceNumber - session.LastSequenceNumber != 1))
            {
                if (session.MediaDataQueue.Length > 0)
                    Log(this, ($"{nameof(OrientProtocolCameraConnection)}", $"Package Lost: {session.LastSequenceNumber + 1}"));

                session.SequenceNumberLost = true;
                session.MediaDataQueue.Clear();
            }
            if (session.SequenceNumberLost)
            {
                var packageFound = false;
                for (var i = 0; i < mediaDataResponse.Data.Length - 16; i++)
                {
                    if (mediaDataResponse.Data[i] == 0x00 && mediaDataResponse.Data[i + 1] == 0x00 && mediaDataResponse.Data[i + 2] == 0x01)
                    {
                        var audioFrameFound = mediaDataResponse.Data[i + 3] == 0xFA && mediaDataResponse.Data[i + 6] == 0xA0 && mediaDataResponse.Data[i + 7] == 0x00; // 00 00 01 FA 0E 02 A0 00 D5 D5 D5 D5 55 D5 55 D5 55 D5 D5 55 55 55 55 D5 55 D5 D5 55 55 D5
                        var interFrameFound = mediaDataResponse.Data[i + 3] == 0xFC && mediaDataResponse.Data[i + 14] <= 0x0A && mediaDataResponse.Data[i + 15] == 0x00; // 00 00 01 FC 02 0D F0 87 9F 25 26 59 DD 04 01 00 00 00 00 01 67 42 00 2A 95 A8 1E 00 89 F9
                        var predictionFrameFound = mediaDataResponse.Data[i + 3] == 0xFD && mediaDataResponse.Data[i + 6] <= 0x02 && mediaDataResponse.Data[i + 7] == 0x00; // 00 00 01 FD BF 3E 00 00 00 00 00 01 61 E0 20 47 CD 09 FA B2 FA F1 32 0A DF E1 11 6C 9F ED
                        if (audioFrameFound || interFrameFound || predictionFrameFound)
                        {
                            session.MediaDataQueue.Enqueue(mediaDataResponse.Data, i, mediaDataResponse.Data.Length - i);
                            session.SequenceNumberLost = false;
                            packageFound = true;
                            break;
                        }
                    }
                }
                //if (!packageFound)
                //{
                //    session.MediaDataQueue.Enqueue(mediaDataResponse.Data);
                //}
            }
            else
            {
                session.MediaDataQueue.Enqueue(mediaDataResponse.Data);
            }

            session.LastSequenceNumber = mediaDataResponse.SequenceNumber;
        }

        private IReadOnlyCollection<IMediaData> ExtractMediaFrames(DataQueue mediaDataQueue, Boolean complementWithZeros = false)
        {
            var mediaData = new List<IMediaData>();
            while (mediaDataQueue.Length >= 16)
            {
                var peekedData = mediaDataQueue.Peek(16);
                var operationCode = peekedData[2] * 256 + peekedData[3];
                var dataSize = 0;
                var packageSize = 0;
                switch (operationCode)
                {
                    case (UInt16)Message.Operation.AudioFrame:
                        dataSize = BitConverter.ToInt16(peekedData, 6);
                        packageSize = dataSize + 8;
                        break;
                    case (UInt16)Message.Operation.PredictionFrame:
                        dataSize = BitConverter.ToInt32(peekedData, 4);
                        packageSize = dataSize + 8;
                        break;
                    case (UInt16)Message.Operation.InterFrame:
                        dataSize = BitConverter.ToInt32(peekedData, 12);
                        packageSize = dataSize + 16;
                        break;
                }

                if ((packageSize == 0 || mediaDataQueue.Length < packageSize) && !complementWithZeros)
                    break;

                var now = DateTime.UtcNow;
                switch (operationCode)
                {
                    case (UInt16)Message.Operation.AudioFrame:
                        var duration = TimeSpan.FromMilliseconds(1000.0 / 50);
                        mediaDataQueue.Dequeue(8);
                        mediaData.Add(new AudioMediaData(mediaDataQueue.Dequeue(dataSize, complementWithZeros), now, duration));
                        break;
                    case (UInt16)Message.Operation.PredictionFrame:
                        duration = TimeSpan.FromMilliseconds(1000.0 / 12.5);
                        mediaDataQueue.Dequeue(8);
                        mediaData.Add(new PredictionFrameMediaData(mediaDataQueue.Dequeue(dataSize, complementWithZeros), now, duration));
                        break;
                    case (UInt16)Message.Operation.InterFrame:
                        duration = TimeSpan.FromMilliseconds(1000.0 / 12.5);
                        mediaDataQueue.Dequeue(16);
                        mediaData.Add(new InterFrameMediaData(mediaDataQueue.Dequeue(dataSize, complementWithZeros), now, duration));
                        break;
                }
            }
            return mediaData;
        }

        private Boolean StartsWithFrame(Byte[] data)
        {
            if (data.Length >= 4)
            {
                if (data[0] == 0x00 && data[1] == 0x00 && data[2] == 0x01)
                {
                    if (data[3] == 0xFA || data[3] == 0xFC || data[3] == 0xFD)
                        return true;
                }
            }
            return false;
        }

        /*private void OnMediaReceived(SessionProperties session, MediaDataResponseMessage mediaDataResponse)
        {
            EnqueueData(session, mediaDataResponse);
            while (session.MediaDataQueue.Length >= 16)
            {
                var peekedData = session.MediaDataQueue.Peek(16);
                var operationCode = peekedData[2] * 256 + peekedData[3];
                var dataSize = 0;
                var packageSize = 0;
                switch (operationCode)
                {
                    case (UInt16)Message.Operation.AudioFrame:
                        dataSize = BitConverter.ToInt16(peekedData, 6);
                        packageSize = dataSize + 8;
                        break;
                    case (UInt16)Message.Operation.PredictionFrame:
                        dataSize = BitConverter.ToInt32(peekedData, 4);
                        packageSize = dataSize + 8;
                        break;
                    case (UInt16)Message.Operation.InterFrame:
                        dataSize = BitConverter.ToInt32(peekedData, 12);
                        packageSize = dataSize + 16;
                        break;
                }

                if (session.MediaDataQueue.Length < packageSize)
                    return;

                var now = DateTime.UtcNow;
                switch (operationCode)
                {
                    case (UInt16)Message.Operation.AudioFrame:
                        var duration = TimeSpan.FromMilliseconds(1000.0 / 50);
                        session.MediaDataQueue.Dequeue(8);
                        MediaReceived(this, new AudioMediaData(session.MediaDataQueue.Dequeue(dataSize), now, duration));
                        break;
                    case (UInt16)Message.Operation.PredictionFrame:
                        duration = TimeSpan.FromMilliseconds(1000.0 / 12.5);
                        session.MediaDataQueue.Dequeue(8);
                        MediaReceived(this, new PredictionFrameMediaData(session.MediaDataQueue.Dequeue(dataSize), now, duration));
                        break;
                    case (UInt16)Message.Operation.InterFrame:
                        duration = TimeSpan.FromMilliseconds(1000.0 / 12.5);
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

        private void EnqueueData(SessionProperties session, MediaDataResponseMessage mediaDataResponse)
        {
            if ((session.LastSequenceNumber != 0) && (mediaDataResponse.SequenceNumber - session.LastSequenceNumber != 1))
            {
                if (session.MediaDataQueue.Length > 0)
                    Log(this, ($"{nameof(OrientProtocolCameraConnection)}", $"Package Lost: {session.LastSequenceNumber + 1}"));

                session.SequenceNumberLost = true;
                session.MediaDataQueue.Clear();
            }
            if (session.SequenceNumberLost)
            {
                var packageFound = false;
                for (var i = 0; i < mediaDataResponse.Data.Length - 16; i++)
                {
                    if (mediaDataResponse.Data[i] == 0x00 && mediaDataResponse.Data[i + 1] == 0x00 && mediaDataResponse.Data[i + 2] == 0x01)
                    {
                        var audioFrameFound = mediaDataResponse.Data[i + 3] == 0xFA && mediaDataResponse.Data[i + 6] == 0xA0 && mediaDataResponse.Data[i + 7] == 0x00; // 00 00 01 FA 0E 02 A0 00 D5 D5 D5 D5 55 D5 55 D5 55 D5 D5 55 55 55 55 D5 55 D5 D5 55 55 D5
                        var interFrameFound = mediaDataResponse.Data[i + 3] == 0xFC && mediaDataResponse.Data[i + 14] <= 0x0A && mediaDataResponse.Data[i + 15] == 0x00; // 00 00 01 FC 02 0D F0 87 9F 25 26 59 DD 04 01 00 00 00 00 01 67 42 00 2A 95 A8 1E 00 89 F9
                        var predictionFrameFound = mediaDataResponse.Data[i + 3] == 0xFD && mediaDataResponse.Data[i + 6] <= 0x02 && mediaDataResponse.Data[i + 7] == 0x00; // 00 00 01 FD BF 3E 00 00 00 00 00 01 61 E0 20 47 CD 09 FA B2 FA F1 32 0A DF E1 11 6C 9F ED
                        if (audioFrameFound || interFrameFound || predictionFrameFound)
                        {
                            session.MediaDataQueue.Enqueue(mediaDataResponse.Data, i, mediaDataResponse.Data.Length - i);
                            session.SequenceNumberLost = false;
                            packageFound = true;
                            break;
                        }
                    }
                }
                //if (!packageFound)
                //{
                //    session.MediaDataQueue.Enqueue(mediaDataResponse.Data);
                //}
            }
            else
            {
                session.MediaDataQueue.Enqueue(mediaDataResponse.Data);
            }

            session.LastSequenceNumber = mediaDataResponse.SequenceNumber;
        }*/

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
            public UInt32 LastSequenceNumber { get; set; }
            public Boolean SequenceNumberLost { get; set; }

            public SessionProperties()
            {
                Id = 0;
                DataQueue = new DataQueue();
                MediaDataQueue = new DataQueue();
                LastSequenceNumber = 0;
            }
        }
    }
}
