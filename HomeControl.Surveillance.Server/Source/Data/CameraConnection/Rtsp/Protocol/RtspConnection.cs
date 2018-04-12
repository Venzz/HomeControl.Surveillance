using HomeControl.Surveillance.Data;
using HomeControl.Surveillance.Server.Data.Tcp;
using System;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace HomeControl.Surveillance.Server.Data.Rtsp.Protocol
{
    internal class RtspConnection: IDisposable
    {
        private TcpConnection Connection;
        private DataQueue DataQueue;

        public event TypedEventHandler<RtspConnection, Object> Connected = delegate { };
        public event TypedEventHandler<RtspConnection, (RtspHeaders Headers, Byte[] Data)> MessageReceived = delegate { };
        public event TypedEventHandler<RtspConnection, Byte[]> DataReceived = delegate { };



        public RtspConnection(String ipAddress, UInt16 port)
        {
            DataQueue = new DataQueue();
            Connection = new TcpConnection(0, ipAddress, port);
            Connection.Connected += (sender, args) => Connected(this, args);
            Connection.DataReceived += OnDataReceived;
        }

        public Task SendAsync(Byte[] data) => Connection.SendAsync(data);

        private void OnDataReceived(TcpConnection sender, Byte[] receivedData)
        {
            try
            {
                DataQueue.Enqueue(receivedData);
                while (DataQueue.Length >= 4)
                {
                    var peekedData = DataQueue.Peek(4);
                    if (Encoding.UTF8.GetString(peekedData) == "RTSP")
                    {
                        var index = DataQueue.IndexOf("\r\n\r\n");
                        if (index.HasValue)
                        {
                            var peekedHeaders = DataQueue.PeekString(index.Value + 4);
                            var headers = RtspHeaders.Create(peekedHeaders);
                            if (headers.ContainsKey("content-length"))
                            {
                                var contentLength = Convert.ToInt32(headers["content-length"]);
                                if (DataQueue.Length < peekedHeaders.Length + contentLength)
                                    return;

                                DataQueue.Dequeue(index.Value + 4);
                                var data = DataQueue.Dequeue(contentLength);
                                MessageReceived(this, (headers, data));
                            }
                            else
                            {
                                DataQueue.Dequeue(index.Value + 4);
                                MessageReceived(this, (headers, new Byte[0]));
                            }
                        }
                    }
                    else if (peekedData[0] == 0x24)
                    {
                        var rtpDataLength = peekedData[2] * 256 + peekedData[3];
                        if (DataQueue.Length < rtpDataLength + 4)
                            return;

                        DataQueue.Dequeue(4);
                        var data = DataQueue.Dequeue(rtpDataLength);
                        DataReceived(this, data);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public void Dispose() => Connection.Dispose();
    }
}