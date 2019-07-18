using HomeControl.Surveillance.Data.Storage;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;

namespace HomeControl.Surveillance.Data.Camera.LocalNetwork
{
    public class LocalNetworkConsumerCameraService: IConsumerCameraService
    {
        private UInt32 Id = 0;
        private Object ConnectionSync = new Object();
        private Object MessagesSync = new Object();
        private DataQueue DataQueue = new DataQueue();
        private IDictionary<UInt32, TaskCompletionSource<IMessage>> Messages = new Dictionary<UInt32, TaskCompletionSource<IMessage>>();
        private IDictionary<UInt32, DataQueue> PartialMessages = new Dictionary<UInt32, DataQueue>();

        private TcpClient TcpClient;
        private IPEndPoint Endpoint;

        public event TypedEventHandler<IConsumerCameraService, (MediaDataType MediaType, Byte[] Data, DateTime Timestamp, TimeSpan Duration)> MediaDataReceived = delegate { };
        public event TypedEventHandler<IConsumerCameraService, (String Message, String Parameter)> LogReceived = delegate { };
        public event TypedEventHandler<IConsumerCameraService, (String Message, Exception Exception)> ExceptionReceived = delegate { };



        public LocalNetworkConsumerCameraService(String endpoint)
        {
            Endpoint = new IPEndPoint(IPAddress.Parse(endpoint), 666);
            StartConnectionMaintaining();
            StartReceiving();
        }

        public void EnsureConnected()
        {
            lock (ConnectionSync)
            {
                if (TcpClient == null)
                    Monitor.Wait(ConnectionSync);
            }
        }

        public Task PerformAsync(Command command)
        {
            return Task.CompletedTask;
        }

        public async Task<IReadOnlyCollection<(String Id, DateTime Date)>> GetStoredRecordsMetadataAsync()
        {
            var message = new Message(Id, Message.GetId(), new StoredRecordsMetadataRequest());
            var responseMessage = await RequestAsync(message);
            if (responseMessage.Type != MessageId.StoredRecordsMetadataResponse)
                throw new InvalidOperationException();
            var response = (StoredRecordsMetadataResponse)responseMessage;
            return response.RecordsMetadata;
        }

        public async Task<IReadOnlyCollection<StoredRecordFile.MediaDataDescriptor>> GetMediaDataDescriptorsAsync(String id)
        {
            var message = new Message(Id, Message.GetId(), new StoredRecordMediaDescriptorsRequest(id));
            var responseMessage = await RequestAsync(message);
            if (responseMessage.Type != MessageId.StoredRecordMediaDescriptorsResponse)
                throw new InvalidOperationException();
            var response = (StoredRecordMediaDescriptorsResponse)responseMessage;
            return response.MediaDescriptors;
        }

        public async Task<Byte[]> GetMediaDataAsync(String id, UInt32 offset)
        {
            var message = new Message(Id, Message.GetId(), new StoredRecordMediaDataRequest(id, offset));
            var responseMessage = await RequestAsync(message);
            if (responseMessage.Type != MessageId.StoredRecordMediaDataResponse)
                throw new InvalidOperationException();
            var response = (StoredRecordMediaDataResponse)responseMessage;
            return response.Data;
        }

        public async Task<IReadOnlyCollection<String>> GetFileListAsync()
        {
            var message = new Message(Id, Message.GetId(), new FileListRequest());
            var responseMessage = await RequestAsync(message);
            if (responseMessage.Type != MessageId.FileListResponse)
                throw new InvalidOperationException();
            var response = (FileListResponse)responseMessage;
            return response.Files;
        }

        public async Task<Byte[]> GetFileDataAsync(String id, UInt32 offset, UInt32 length, CancellationToken cancellationToken)
        {
            var message = new Message(Id, Message.GetId(), new FileDataRequest(id, offset, length));
            var responseMessage = await RequestAsync(message, cancellationToken);
            if (responseMessage.Type != MessageId.FileDataResponse)
                throw new InvalidOperationException();
            var response = (FileDataResponse)responseMessage;
            return response.Data;
        }

        public async Task SetPushChannelUriAsync(String previousChannelUri, String channelUri)
        {
            var message = new Message(new PushChannelUri(previousChannelUri, channelUri));
            await PerformAsync(message).ConfigureAwait(false);
        }

        private async void StartConnectionMaintaining() => await Task.Run(async () =>
        {
            while (true)
            {
                lock (ConnectionSync)
                {
                    if (TcpClient != null)
                        Monitor.Wait(ConnectionSync);
                }

                try
                {

                    var tcpClient = new TcpClient();
                    await tcpClient.ConnectAsync(Endpoint.Address, Endpoint.Port).ConfigureAwait(false);
                    LogReceived(this, ($"{nameof(LocalNetworkConsumerCameraService)}", "Connected."));

                    lock (ConnectionSync)
                    {
                        DataQueue = new DataQueue();
                        TcpClient = tcpClient;
                        Monitor.PulseAll(ConnectionSync);
                    }
                }
                catch (Exception exception)
                {
                    ExceptionReceived(this, ($"{nameof(LocalNetworkConsumerCameraService)}.{nameof(StartConnectionMaintaining)}", exception));
                }
            }
        });

        private async void StartReceiving() => await Task.Run(async () =>
        {
            var buffer = new Byte[1 * 1024 * 1024];
            while (true)
            {
                var tcpClient = (TcpClient)null;
                lock (ConnectionSync)
                {
                    if (TcpClient == null)
                        Monitor.Wait(ConnectionSync);
                    tcpClient = TcpClient;
                }

                try
                {
                    var result = await tcpClient.GetStream().ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                    DataQueue.Enqueue(buffer, 0, result);
                    while (DataQueue.Length > 8)
                    {
                        var peekedData = DataQueue.Peek(12);
                        var dataLength = BitConverter.ToInt32(peekedData, 8);
                        if (dataLength + 12 > DataQueue.Length)
                            break;

                        if ((peekedData[0] == 0xFF) && (peekedData[1] == 0xFF) && (peekedData[2] == 0xFF) && (peekedData[3] == 0xFE))
                        {
                            var data = DataQueue.Dequeue(dataLength + 12);
                            var message = new Message(data);
                            if (message.Id == 0)
                            {
                                switch (Message.Create(message))
                                {
                                    case LiveMediaDataResponse liveMediaData:
                                        MediaDataReceived(this, (liveMediaData.MediaType, liveMediaData.Data, liveMediaData.Timestamp, liveMediaData.Duration));
                                        break;
                                }
                            }
                            else
                            {
                                lock (MessagesSync)
                                {
                                    if (!Messages.ContainsKey(message.Id))
                                        continue;

                                    var messageResponse = Message.Create(message);
                                    if (!(messageResponse is PartialMessageResponse partialMessageResponse))
                                    {
                                        Messages[message.Id].SetResult(messageResponse);
                                        Messages.Remove(message.Id);
                                    }
                                    else
                                    {
                                        if (!PartialMessages.ContainsKey(message.Id))
                                            PartialMessages.Add(message.Id, new DataQueue());
                                        PartialMessages[message.Id].Enqueue(partialMessageResponse.Data);

                                        if (partialMessageResponse.Final)
                                        {
                                            message = new Message(PartialMessages[message.Id].Dequeue(PartialMessages[message.Id].Length));
                                            Messages[message.Id].TrySetResult(Message.Create(message));
                                            Messages.Remove(message.Id);
                                            PartialMessages.Remove(message.Id);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    ExceptionReceived(this, ($"{nameof(LocalNetworkConsumerCameraService)}.{nameof(StartReceiving)}", exception));
                    try { tcpClient.Dispose(); } catch (Exception) { }
                    lock (ConnectionSync)
                    {
                        if (TcpClient == tcpClient)
                        {
                            TcpClient = null;
                            Monitor.PulseAll(ConnectionSync);
                        }
                    }
                }
            }
        });

        private async Task<IMessage> RequestAsync(Message message, CancellationToken cancellationToken = new CancellationToken())
        {
            var tcpClient = TcpClient;
            if (tcpClient == null)
                throw new InvalidOperationException();

            var messageResponseAwaiter = new TaskCompletionSource<IMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
            Action cancelAction = () =>
            {
                lock (MessagesSync)
                {
                    messageResponseAwaiter.TrySetCanceled();
                    if (Messages.ContainsKey(message.Id))
                        Messages.Remove(message.Id);
                    if (PartialMessages.ContainsKey(message.Id))
                        PartialMessages.Remove(message.Id);
                }
            };
            using (cancellationToken.Register(cancelAction))
            {
                Messages.Add(message.Id, messageResponseAwaiter);
                await tcpClient.GetStream().WriteAsync(message.Data, 0, message.Data.Length, cancellationToken).ConfigureAwait(false);
                return await messageResponseAwaiter.Task.ConfigureAwait(false);
            }
        }

        private async Task PerformAsync(Message message)
        {
            var tcpClient = TcpClient;
            if (tcpClient == null)
                return;

            await tcpClient.GetStream().WriteAsync(message.Data, 0, message.Data.Length).ConfigureAwait(false);
        }
    }
}