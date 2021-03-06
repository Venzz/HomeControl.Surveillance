﻿using System;

namespace HomeControl.Surveillance.Services.Heroku
{
    public class PushChannelSettings: IServiceMessage
    {
        public ServiceMessageId Type { get; } = ServiceMessageId.PushChannelSettings;
        public String ClientId { get; }
        public String ClientSecret { get; }

        public PushChannelSettings(String clientId, String clientSecret)
        {
            ClientId = clientId;
            ClientSecret = clientSecret;
        }
    }
}
