namespace System.Net.WebSockets
{
    public enum WebSocketCloseStatus
    {
        NormalClosure = 1000,
        EndpointUnavailable = 1001,
        ProtocolError = 1002,
        InvalidMessageType = 1003,
        Empty = 1005,
        InvalidPayloadData = 1007,
        PolicyViolation = 1008,
        MessageTooBig = 1009,
        MandatoryExtension = 1010,
        InternalServerError = 1011
    }

    public enum WebSocketError
    {
        Success = 0,
        InvalidMessageType = 1,
        Faulted = 2,
        NativeError = 3,
        NotAWebSocket = 4,
        UnsupportedVersion = 5,
        UnsupportedProtocol = 6,
        HeaderError = 7,
        ConnectionClosedPrematurely = 8,
        InvalidState = 9
    }

    public enum WebSocketMessageType
    {
        Text = 0,
        Binary = 1,
        Close = 2
    }

    public enum WebSocketState
    {
        None = 0,
        Connecting = 1,
        Open = 2,
        CloseSent = 3,
        CloseReceived = 4,
        Closed = 5,
        Aborted = 6
    }

    public sealed class ClientWebSocketOptions
    {
        public CookieContainer Cookies { get; set; }
        public ICredentials Credentials { get; set; }
        public TimeSpan KeepAliveInterval { get; set; }
        public IWebProxy Proxy { get; set; }

        public void AddSubProtocol(String subProtocol) { }
        public void SetRequestHeader(String headerName, String headerValue) { }
    }

    public class WebSocketReceiveResult
    {
        public WebSocketCloseStatus? CloseStatus { get; }
        public String CloseStatusDescription { get; }
        public Int32 Count { get; }
        public Boolean EndOfMessage { get; }
        public WebSocketMessageType MessageType { get; }

        public WebSocketReceiveResult(Int32 count, WebSocketMessageType messageType, Boolean endOfMessage)
        {
            Count = count;
            MessageType = messageType;
            EndOfMessage = endOfMessage;
        }

        public WebSocketReceiveResult(Int32 count, WebSocketMessageType messageType, Boolean endOfMessage, WebSocketCloseStatus? closeStatus, String closeStatusDescription)
        {
            Count = count;
            MessageType = messageType;
            EndOfMessage = endOfMessage;
            CloseStatus = closeStatus;
            CloseStatusDescription = closeStatusDescription;
        }
    }

    public sealed class WebSocketException: Exception
    {
        public Int32 ErrorCode { get; }
        public WebSocketError WebSocketErrorCode { get; }

        public WebSocketException(Int32 nativeError) { }
        public WebSocketException(WebSocketError error) { }
        public WebSocketException(String message) { }
        public WebSocketException(Int32 nativeError, Exception innerException) { }
        public WebSocketException(Int32 nativeError, String message) { }
        public WebSocketException(WebSocketError error, Exception innerException) { }
        public WebSocketException(WebSocketError error, Int32 nativeError) { }
        public WebSocketException(WebSocketError error, String message) { }
        public WebSocketException(String message, Exception innerException) { }
        public WebSocketException(WebSocketError error, Int32 nativeError, Exception innerException) { }
        public WebSocketException(WebSocketError error, Int32 nativeError, String message) { }
        public WebSocketException(WebSocketError error, String message, Exception innerException) { }
        public WebSocketException(WebSocketError error, Int32 nativeError, String message, Exception innerException) { }
    }
}
