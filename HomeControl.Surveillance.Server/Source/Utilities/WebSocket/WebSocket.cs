using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace HomeControl.Surveillance
{
    public class WebSocket: IWebSocket
    {
        private ClientWebSocket InternalWebSocket;
        private CancellationToken CancellationToken = new CancellationTokenSource().Token;
        private Object Sync = new Object();
        private Queue<Byte[]> DataQueue = new Queue<Byte[]>();
        private Boolean IsClosed;



        public WebSocket()
        {
            InternalWebSocket = new ClientWebSocket();
        }

        public Task ConnectAsync(String url) => InternalWebSocket.ConnectAsync(new Uri(url), CancellationToken);

        public async Task SendAsync(Byte[] data)
        {
            lock (Sync)
            {
                DataQueue.Enqueue(data);
                if (DataQueue.Count > 1)
                    return;
            }

            while (true)
            {
                try
                {
                    var dequeueData = DataQueue.Peek();
                    await InternalWebSocket.SendAsync(new ArraySegment<Byte>(dequeueData), WebSocketMessageType.Binary, true, CancellationToken).ConfigureAwait(false);
                }
                catch (WebSocketException webSocketException)
                {
                    if (IsClosed && webSocketException.Message.Contains("'CloseSent'"))
                        return;
                    throw webSocketException;
                }

                lock (Sync)
                {
                    DataQueue.Dequeue();
                    if (DataQueue.Count == 0)
                        break;
                }
            }
        }

        public async Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<Byte> buffer)
        {
            try
            {
                return await InternalWebSocket.ReceiveAsync(buffer, CancellationToken).ConfigureAwait(false);
            }
            catch (WebSocketException webSocketException)
            {
                if (IsClosed && webSocketException.Message.Contains("'CloseReceived'"))
                    return new WebSocketReceiveResult(0, WebSocketMessageType.Binary, true);
                throw webSocketException;
            }
        }

        public Task CloseAsync()
        {
            IsClosed = true;
            return InternalWebSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "kek", CancellationToken);
        }

        public void Abort()
        {
            IsClosed = true;
            InternalWebSocket.Abort();
        }
    }
}
