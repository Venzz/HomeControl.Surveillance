﻿using System;

namespace HomeControl.Surveillance.Services.Heroku
{
    public class PushChannelUri: IServiceMessage
    {
        public ServiceMessageId Type { get; } = ServiceMessageId.PushChannelUri;
        public String PreviousChannelUri { get; }
        public String ChannelUri { get; }

        public PushChannelUri(String previousChannelUri, String channelUri)
        {
            PreviousChannelUri = previousChannelUri;
            ChannelUri = channelUri;
        }
    }
}
