using System;
using Venz.UI.Xaml;

namespace HomeControl.Surveillance.Player
{
    public class Settings: ApplicationSettings
    {
        public String PushChannelUri
        {
            get { return Get(nameof(PushChannelUri), ""); }
            set { Set(nameof(PushChannelUri), value); }
        }
    }
}
