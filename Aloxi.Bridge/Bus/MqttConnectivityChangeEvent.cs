using System;
using System.ComponentModel;

namespace ZoolWay.Aloxi.Bridge.Bus
{
    [ImmutableObject(true)]
    public class MqttConnectivityChangeEvent
    {
        public bool IsConnected { get; }
        public bool IsSubscribed { get; }
        public DateTime UpdateTimestamp { get; }

        public MqttConnectivityChangeEvent(bool isConnected, bool isSubscribed, DateTime updateTimestamp)
        {
            this.IsConnected = isConnected;
            this.IsSubscribed = isSubscribed;
            this.UpdateTimestamp = updateTimestamp;
        }
    }
}
