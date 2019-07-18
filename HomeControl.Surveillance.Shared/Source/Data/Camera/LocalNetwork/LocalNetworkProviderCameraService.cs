using HomeControl.Surveillance.Data.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;

namespace HomeControl.Surveillance.Data.Camera.LocalNetwork
{
    public class LocalNetworkProviderCameraService: IProviderCameraService
    {
        private Dictionary<UInt32, LocalNetworkClient> Clients = new Dictionary<UInt32, LocalNetworkClient>();
        private ITcpServer Server;
        private Object ClientSync = new Object();

        public event TypedEventHandler<IProviderCameraService, Command> CommandReceived = delegate { };
        public event TypedEventHandler<IProviderCameraService, (UInt32 ConsumerId, UInt32 Id, IMessage Message)> MessageReceived = delegate { };
        public event TypedEventHandler<IProviderCameraService, (String Message, String Parameter)> LogReceived = delegate { };
        public event TypedEventHandler<IProviderCameraService, (String Message, Exception Exception)> ExceptionReceived = delegate { };



        public LocalNetworkProviderCameraService(ITcpServer tcpServer)
        {
            Server = tcpServer;
            Server.Start();

            Server.ClientConnected += OnClientConnected;
            Server.DataReceived += OnDataReceived;
            Server.LogReceived += (sender, args) => LogReceived(this, args);
            Server.ClientDisconnected += OnClientDisconnected;
        }

        private void OnDataReceived(Object sender, (UInt32 SocketId, Byte[] Data) args)
        {
            try
            {
                var client = (LocalNetworkClient)null;
                lock (ClientSync)
                {
                    if (!Clients.ContainsKey(args.SocketId))
                        Clients.Add(args.SocketId, new LocalNetworkClient(args.SocketId));
                    client = Clients[args.SocketId];
                }

                client.DataQueue.Enqueue(args.Data);
                while (true)
                {
                    if (client.DataQueue.Length < 12)
                        break;

                    var size = BitConverter.ToInt32(client.DataQueue.Peek(12), 8);
                    if (client.DataQueue.Length - 12 < size)
                        break;

                    var data = client.DataQueue.Dequeue(size + 12);
                    if ((data[0] == 0xFF) && (data[1] == 0xFF) && (data[2] == 0xFF) && (data[3] == 0xFD))
                        break;

                    var message = new Message(args.SocketId, data);
                    MessageReceived(this, (message.ConsumerId, message.Id, Message.Create(message)));
                }
            }
            catch (Exception exception)
            {
                ExceptionReceived(this, ($"{nameof(LocalNetworkProviderCameraService)}.{nameof(OnDataReceived)}", exception));
            }
        }

        public void EnsureConnected()
        {
        }

        public Task SendStoredRecordsMetadataAsync(UInt32 consumerId, UInt32 id, IReadOnlyCollection<(String Id, DateTime Date)> storedRecordsMetadata)
        {
            var response = new StoredRecordsMetadataResponse(storedRecordsMetadata);
            var responseMessage = new Message(consumerId, id, response);
            Send(responseMessage);
            return Task.CompletedTask;
        }

        public Task SendLiveMediaDataAsync(MediaDataType type, Byte[] data, DateTime timestamp, TimeSpan duration)
        {
            var response = new LiveMediaDataResponse(type, data, timestamp, duration);
            var responseMessage = new Message(0, 0, response);
            Send(responseMessage);
            return Task.CompletedTask;
        }

        public Task SendMediaDataDescriptorsAsync(UInt32 consumerId, UInt32 id, IReadOnlyCollection<StoredRecordFile.MediaDataDescriptor> descriptors)
        {
            var response = new StoredRecordMediaDescriptorsResponse(descriptors);
            var responseMessage = new Message(consumerId, id, response);
            Send(responseMessage);
            return Task.CompletedTask;
        }

        public Task SendMediaDataAsync(UInt32 consumerId, UInt32 id, Byte[] data)
        {
            var response = new StoredRecordMediaDataResponse(data);
            var responseMessage = new Message(consumerId, id, response);
            Send(responseMessage);
            return Task.CompletedTask;
        }

        public Task SendFileListAsync(UInt32 consumerId, UInt32 id, IReadOnlyCollection<String> fileList)
        {
            var response = new FileListResponse(fileList);
            var responseMessage = new Message(consumerId, id, response);
            Send(responseMessage);
            return Task.CompletedTask;
        }

        public Task SendFileDataAsync(UInt32 consumerId, UInt32 id, Byte[] data)
        {
            var response = new FileDataResponse(data);
            var responseMessage = new Message(consumerId, id, response);
            Send(responseMessage);
            return Task.CompletedTask;
        }

        public Task SetPushChannelSettingsAsync(String clientId, String clientSecret)
        {
            var serviceMessage = new PushChannelSettings(clientId, clientSecret);
            var message = new Message(serviceMessage);
            Send(message);
            return Task.CompletedTask;
        }

        public Task SetPushMessageAsync(String content)
        {
            var serviceMessage = new PushNotification(content);
            var message = new Message(serviceMessage);
            Send(message);
            return Task.CompletedTask;
        }

        private void Send(Message message)
        {
            try
            {
                lock (ClientSync)
                {
                    if (message.ConsumerId == 0)
                    {
                        foreach (var client in Clients)
                            Server.Send(message.ConsumerId, message.Data);
                    }
                    else
                    {
                        if (!Clients.ContainsKey(message.ConsumerId))
                            return;
                        var client = Clients[message.ConsumerId];
                        Server.Send(message.ConsumerId, message.Data);
                    }
                }
            }
            catch (Exception exception)
            {
                ExceptionReceived(this, ($"{nameof(LocalNetworkProviderCameraService)}.{nameof(Send)}", exception));
            }
        }

        private void OnClientConnected(Object sender, UInt32 args)
        {
            lock (ClientSync)
            {
                if (!Clients.ContainsKey(args))
                    Clients.Add(args, new LocalNetworkClient(args));
            }
        }

        private void OnClientDisconnected(Object sender, UInt32 args)
        {
            lock (ClientSync)
            {
                if (Clients.ContainsKey(args))
                    Clients.Remove(args);
            }
        }
    }
}