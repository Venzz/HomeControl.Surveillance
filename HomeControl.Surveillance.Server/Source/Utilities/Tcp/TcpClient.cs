using System;
using System.Threading.Tasks;

namespace HomeControl.Surveillance.Server
{
    internal class TcpClient: IDisposable
    {
        private System.Net.Sockets.TcpClient InternalTcpClient = new System.Net.Sockets.TcpClient();
        private Byte[] Buffer = new Byte[4096];
        private String IpAddress;
        private UInt16 Port;

        public UInt32 Id { get; private set; }



        public TcpClient(String ipAddress, UInt16 port)
        {
            IpAddress = ipAddress;
            Port = port;
        }

        public Task ConnectAsync()
        {
            return InternalTcpClient.ConnectAsync(IpAddress, Port);
        }

        public async Task SendAsync(Byte[] data)
        {
            await InternalTcpClient.GetStream().WriteAsync(data, 0, data.Length).ConfigureAwait(false);
            await InternalTcpClient.GetStream().FlushAsync().ConfigureAwait(false);
        }

        public async Task<Byte[]> ReceiveAsync()
        {
            var readCount = await InternalTcpClient.GetStream().ReadAsync(Buffer, 0, 4096).ConfigureAwait(false);
            if (readCount > 0)
            {
                var data = new Byte[readCount];
                Array.Copy(Buffer, data, readCount);
                return data;
            }
            else
            {
                return new Byte[0];
            }
        }

        public TcpClient DisposeAndRenew(Exception exception)
        {
            InternalTcpClient.Dispose();
            return new TcpClient(IpAddress, Port) { Id = Id++ };
        }

        public void Dispose() => InternalTcpClient.Dispose();
    }
}