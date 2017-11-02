using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace HomeControl.Surveillance.Server.Data.Rtsp.Protocol
{
    internal class RtspClient
    {
        private RtspConnection Connection;
        private String Url;
        private UInt32 Sequence = 1;

        public event TypedEventHandler<RtspClient, Object> Connected = delegate { };
        public event TypedEventHandler<RtspClient, IResponse> ResponseReceived = delegate { };
        public event TypedEventHandler<RtspClient, List<Byte[]>> DataReceived = delegate { };



        public RtspClient(String ipAddress, UInt16 port, String url)
        {
            Url = url;
            Connection = new RtspConnection(ipAddress, port);
            Connection.Connected += (sender, args) => Connected(this, args);
            Connection.MessageReceived += OnMessageReceived;
            Connection.DataReceived += OnDataReceived;
        }

        public Task SendMessageAsync(Message message) => SendMessageAsync(message, new Dictionary<String, String>());

        public Task SendMessageAsync(Message message, IDictionary<String, String> headers, String urlOverride = null)
        {
            var request = new StringBuilder();
            request.AppendLine($"{GetMethod(message)} {urlOverride ?? Url} RTSP/1.0");
            request.AppendLine($"CSeq: {Sequence++}");
            foreach (var item in headers)
                request.AppendLine($"{item.Key}: {item.Value}");
            request.AppendLine();
            return Connection.SendAsync(Encoding.UTF8.GetBytes(request.ToString()));
        }

        private void OnMessageReceived(RtspConnection sender, (RtspHeaders Headers, Byte[] Data) message)
        {
            try
            {
                if (message.Headers.Status.StatusCode == HttpStatusCode.Unauthorized)
                {
                    ResponseReceived(this, UnauthorizedResponse.Create(message.Headers));
                }
                else
                {
                    if (message.Headers.ContainsKey("cseq") && (message.Headers["cseq"] == "1"))
                        ResponseReceived(this, new OptionsResponse());
                    if (message.Headers.ContainsKey("cseq") && (message.Headers["cseq"] == "2"))
                        ResponseReceived(this, new DescribeResponse());
                    if (message.Headers.ContainsKey("cseq") && (message.Headers["cseq"] == "3"))
                        ResponseReceived(this, new SetupResponse(message.Headers));
                }
            }
            catch (Exception)
            {
            }
        }

        private List<Byte[]> Data = new List<byte[]>();

        private void OnDataReceived(RtspConnection sender, Byte[] data)
        {
            int rtp_version = data[0] >> 6;
            int rtp_padding = data[0] >> 5 & 0x01;
            int rtp_extension = (data[0] >> 4) & 0x01;
            int rtp_csrc_count = (data[0] >> 0) & 0x0F;
            int rtp_marker = (data[1] >> 7) & 0x01;
            int rtp_payload_type = (data[1] >> 0) & 0x7F;
            UInt16 rtp_sequence_number = (UInt16)((data[2] << 8) + data[3]);
            var rtp_timestamp = (data[4] << 24) + (data[5] << 16) + (data[6] << 8) + (data[7]);
            var rtp_ssrc = (data[8] << 24) + (data[9] << 16) + (data[10] << 8) + (data[11]);

            byte[] rtp_payload = new byte[data.Length - 12];
            Array.Copy(data, 12, rtp_payload, 0, rtp_payload.Length);

            Data.Add(rtp_payload);
            if (rtp_marker == 1)
            {
                DataReceived(this, new List<byte[]>(Data));
                Data.Clear();
            }
        }

        internal class MessageReceivedEventArgs : EventArgs
        {
            public Byte[] RawData { get; }
            public Int64 MessageId { get; }
            public Int32 SequenceNumber { get; }
            public MessageReceivedEventArgs(Int64 messageId, Int32 sequenceNumber, Byte[] rawData) { MessageId = messageId; SequenceNumber = sequenceNumber; RawData = rawData; }
        }

        private String GetMethod(Message message)
        {
            switch (message)
            {
                case Message.Options:
                    return "OPTIONS";
                case Message.Describe:
                    return "DESCRIBE";
                case Message.Setup:
                    return "SETUP";
                case Message.Play:
                    return "PLAY";
                case Message.GetParameter:
                    return "GET_PARAMETER";
                default:
                    throw new NotSupportedException();
            }
        }

        public enum Message { Options, Describe, Setup, Play, GetParameter }
    }
}