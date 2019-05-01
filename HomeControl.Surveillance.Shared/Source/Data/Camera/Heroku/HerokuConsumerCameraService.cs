using HomeControl.Surveillance.Data.Storage;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;

namespace HomeControl.Surveillance.Data.Camera.Heroku
{
    public class HerokuConsumerCameraService: IConsumerCameraService
    {
        private UInt32 Id = 0;
        private Object ConnectionSync = new Object();
        private IWebSocket WebSocket;
        private String ServiceName;
        private DataQueue DataQueue = new DataQueue();
        private IDictionary<UInt32, TaskCompletionSource<IMessage>> Messages = new Dictionary<UInt32, TaskCompletionSource<IMessage>>();
        private IDictionary<UInt32, DataQueue> PartialMessages = new Dictionary<UInt32, DataQueue>();

        public event TypedEventHandler<IConsumerCameraService, (MediaDataType MediaType, Byte[] Data, DateTime Timestamp, TimeSpan Duration)> MediaDataReceived = delegate { };
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
                        DataQueue = new DataQueue();
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
                    DataQueue.Enqueue(buffer.Array, 0, result.Count);
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
                                        Messages[message.Id].SetResult(Message.Create(message));
                                        Messages.Remove(message.Id);
                                        PartialMessages.Remove(message.Id);
                                    }
                                }
                            }
                        }
                    }
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

        private async Task<IMessage> RequestAsync(Message message)
        {
            var webSocket = WebSocket;
            if (webSocket == null)
                throw new InvalidOperationException();

            var messageResponseAwaiter = new TaskCompletionSource<IMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
            Messages.Add(message.Id, messageResponseAwaiter);
            await webSocket.SendAsync(message.Data).ConfigureAwait(false);
            return await messageResponseAwaiter.Task.ConfigureAwait(false);
        }
    }
}