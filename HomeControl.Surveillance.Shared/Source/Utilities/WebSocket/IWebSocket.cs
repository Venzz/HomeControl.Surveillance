using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace HomeControl.Surveillance
{
    public interface IWebSocket
    {
        Task ConnectAsync(String url);
        Task SendAsync(Byte[] data);
        Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<Byte> buffer);
        Task CloseAsync();
        void Abort();
    }
}
