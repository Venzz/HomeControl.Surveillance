using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HomeControl.Surveillance.Server.Data.Rtsp.Protocol
{
    public class RtspHeaders: Dictionary<String, String>
    {
        public ResponseStatus Status { get; private set; }

        private RtspHeaders() { }

        public static RtspHeaders Create(Byte[] headersData) => Create(Encoding.UTF8.GetString(headersData, 0, headersData.Length));

        public static RtspHeaders Create(String headersString)
        {
            var headers = new RtspHeaders();
            var headersStrings = headersString.Split(new Char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            headers.Status = ResponseStatus.Create(headersStrings[0]);
            foreach (var headerString in headersStrings.Skip(1))
            {
                var headerStringParts = headerString.Split(new String[] { ": " }, StringSplitOptions.RemoveEmptyEntries);
                headers.Add(headerStringParts[0].Trim().ToLower(), headerStringParts[1].Trim());
            }

            return headers;
        }
    }
}
