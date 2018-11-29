using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;

namespace HomeControl.Surveillance.Data.Camera.Heroku
{
    public class HerokuConsumerCameraService: IConsumerCameraService
    {
        private Object ConnectionSync = new Object();
        private IWebSocket WebSocket;
        private String ServiceName;
        private DataQueue DataQueue = new DataQueue();
        private IDictionary<UInt32, TaskCompletionSource<IMessage>> Messages = new Dictionary<UInt32, TaskCompletionSource<IMessage>>();

        public event TypedEventHandler<IConsumerCameraService, Byte[]> DataReceived = delegate { };
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

        public async Task<IReadOnlyCollection<DateTime>> GetStoredRecordsMetadataAsync()
        {
            var message = new Message(Message.GetId(), new StoredRecordsMetadataRequest());
            var responseMessage = await RequestAsync(message);
            if (responseMessage.Type != MessageId.StoredRecordsMetadataResponse)
                throw new InvalidOperationException();
            var response = (StoredRecordsMetadataResponse)responseMessage;
            return response.RecordsMetadata;
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
                        var peekedData = DataQueue.Peek(8);
                        var dataLength = BitConverter.ToInt32(peekedData, 4);
                        if (dataLength + 8 > DataQueue.Length)
                            break;

                        if ((peekedData[0] == 0xFF) && (peekedData[1] == 0xFF) && (peekedData[2] == 0xFF) && (peekedData[3] == 0xFE))
                        {
                            var data = DataQueue.Dequeue(dataLength + 8);
                            var message = new Message(data);
                            if (!Messages.ContainsKey(message.Id))
                                continue;
                            Messages[message.Id].SetResult(Message.Create(message));
                            Messages.Remove(message.Id);
                        }
                        else
                        {
                            DataQueue.Dequeue(8);
                            DataReceived(this, DataQueue.Dequeue(dataLength));
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