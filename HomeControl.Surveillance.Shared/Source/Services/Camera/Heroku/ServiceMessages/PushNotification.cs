using System;

namespace HomeControl.Surveillance.Services.Heroku
{
    public class PushNotification: IServiceMessage
    {
        public ServiceMessageId Type { get; } = ServiceMessageId.PushNotification;
        public String Content { get; }

        public PushNotification(String content)
        {
            Content = content;
        }
    }
}
