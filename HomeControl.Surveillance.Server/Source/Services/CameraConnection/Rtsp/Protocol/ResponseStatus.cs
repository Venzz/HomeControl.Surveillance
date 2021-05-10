using System;
using System.Net;

namespace HomeControl.Surveillance.Server.Services.Rtsp
{
    public class ResponseStatus
    {
        public String ProtocolVersion { get; private set; }
        public HttpStatusCode StatusCode { get; private set; }

        private ResponseStatus() { }

        public static ResponseStatus Create(String statusString)
        {
            var parts = statusString.Split(new String[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            return new ResponseStatus() { ProtocolVersion = parts[0], StatusCode = (HttpStatusCode)Convert.ToInt32(parts[1]) };
        }
    }
}
