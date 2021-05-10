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
                var dequeueData = DataQueue.Peek();
                await InternalWebSocket.SendAsync(new ArraySegment<Byte>(dequeueData), WebSocketMessageType.Binary, true, CancellationToken).ConfigureAwait(false);

                lock (Sync)
                {
                    DataQueue.Dequeue();
                    if (DataQueue.Count == 0)
                        break;
                }
            }
        }

        public Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<Byte> buffer) => InternalWebSocket.ReceiveAsync(buffer, CancellationToken);

        public Task CloseAsync() => InternalWebSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "kek", CancellationToken);

        public void Abort() => InternalWebSocket.Abort();
    }
}
