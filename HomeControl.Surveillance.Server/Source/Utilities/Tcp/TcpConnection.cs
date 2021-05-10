using System;
using System.Threading.Tasks;
using Windows.Foundation;

namespace HomeControl.Surveillance.Server
{
    internal class TcpConnection: IDisposable
    {
        private Boolean IsDisposed;
        private TcpClient TcpClient;
        private Object Sync = new Object();
        private Task ConnectOperationTask;

        public Byte[] OnConnectedData { get; set; }
        public UInt32 Id { get; private set; }

        public event TypedEventHandler<TcpConnection, Object> Connected = delegate { };
        public event TypedEventHandler<TcpConnection, Byte[]> DataReceived = delegate { };
        public event TypedEventHandler<TcpConnection, Object> Disconnected = delegate { };



        public TcpConnection(UInt32 id, String ipAddress, UInt16 port)
        {
            TcpClient = new TcpClient(ipAddress, port);
            Id = id;
        }

        private Task ConnectAsync(TcpClient tcpClient, TimeSpan? delay) => Task.Run(async () =>
        {
            try
            {
                if (delay.HasValue)
                    await Task.Delay(delay.Value).ConfigureAwait(false);

                await tcpClient.ConnectAsync().ConfigureAwait(false);
                if (OnConnectedData != null)
                    await tcpClient.SendAsync(OnConnectedData).ConfigureAwait(false);
                Connected(this, null);
                ContinueReceivingSequence(tcpClient);
            }
            catch (Exception)
            {
                throw;
            }
        });

        public Task SendAsync(Byte[] data)
        {
            lock (Sync)
            {
                if (ConnectOperationTask == null)
                    ConnectOperationTask = ConnectAsync(TcpClient, delay: null);
                return SendAsync(TcpClient, ConnectOperationTask, data);
            }
        }

        private async Task SendAsync(TcpClient tcpClient, Task connectOperation, Byte[] data)
        {
            try
            {
                await connectOperation.ConfigureAwait(false);
            }
            catch (Exception)
            {
                throw;
            }

            try
            {
                await tcpClient.SendAsync(data).ConfigureAwait(false);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<Boolean> ReceiveAsync(TcpClient tcpClient)
        {
            try
            {
                var data = await tcpClient.ReceiveAsync().ConfigureAwait(false);
                if (data.Length > 0)
                {
                    DataReceived(this, data);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void ContinueReceivingSequence(TcpClient tcpClient)
        {
            ReceiveAsync(tcpClient).ContinueWith(task =>
            {
                var continueReceivingSequence = false;
                lock (Sync)
                    continueReceivingSequence = task.Result && (TcpClient.Id == tcpClient.Id) && !IsDisposed;

                if (continueReceivingSequence)
                    ContinueReceivingSequence(tcpClient);
            });
        }

        public void Dispose()
        {
            lock (Sync)
            {
                IsDisposed = true;
                TcpClient.Dispose();
            }
        }
    }
}