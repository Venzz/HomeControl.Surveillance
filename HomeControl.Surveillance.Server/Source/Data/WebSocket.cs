using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace HomeControl.Surveillance.Data
{
    public class WebSocket: IWebSocket
    {
        private ClientWebSocket InternalWebSocket;
        private CancellationToken CancellationToken = new CancellationTokenSource().Token;



        public WebSocket()
        {
            InternalWebSocket = new ClientWebSocket();
        }

        public Task ConnectAsync(String url) => InternalWebSocket.ConnectAsync(new Uri(url), CancellationToken);

        public Task SendAsync(Byte[] data) => InternalWebSocket.SendAsync(new ArraySegment<Byte>(data), WebSocketMessageType.Binary, true, CancellationToken);

        public Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<Byte> buffer) => InternalWebSocket.ReceiveAsync(buffer, CancellationToken);

        public Task CloseAsync() => InternalWebSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "kek", CancellationToken);

        public void Abort() => InternalWebSocket.Abort();
    }
}
