using System;

namespace HomeControl.Surveillance.Server.Services.Rtsp
{
    public class UnauthorizedResponse: IResponse
    {
        private UnauthorizedResponse() { }

        public UInt32 Sequence { get; private set; }
        public String DigestRealm { get; private set; }
        public String Nonce { get; private set; }

        public static UnauthorizedResponse Create(RtspHeaders headers)
        {
            var response = new UnauthorizedResponse();
            var authenticateHeaderValuesStrings = headers["www-authenticate"].Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            response.DigestRealm = authenticateHeaderValuesStrings[0].Split(new Char[] { '=' }, StringSplitOptions.RemoveEmptyEntries)[1].Replace("\"", "");
            response.Nonce = authenticateHeaderValuesStrings[1].Split(new Char[] { '=' }, StringSplitOptions.RemoveEmptyEntries)[1].Replace("\"", "");
            return response;
        }

        /*
            RTSP/1.0 401 Unauthorized
            Cseq: 2 
            Server: Rtsp Server 1280*720*30*4096
            WWW-Authenticate: Digest realm="Surveillance Server", nonce="61321599"
         */
    }
}
