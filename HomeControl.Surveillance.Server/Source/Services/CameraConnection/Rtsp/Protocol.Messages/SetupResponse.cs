using System;

namespace HomeControl.Surveillance.Server.Services.Rtsp
{
    public class SetupResponse: IResponse
    {
        public String Session { get; }

        public SetupResponse(RtspHeaders headers)
        {
            var sessionValue = headers["session"];
            Session = sessionValue.Split(new Char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)[0];
        }
    }
}
