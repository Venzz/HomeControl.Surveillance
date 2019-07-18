using System;

namespace HomeControl.Surveillance.Data.Camera.LocalNetwork
{
    internal class LocalNetworkClient
    {
        public UInt32 Id { get; }
        public DataQueue DataQueue { get; } = new DataQueue();

        public LocalNetworkClient(UInt32 id)
        {
            Id = id;
        }
    }
}
