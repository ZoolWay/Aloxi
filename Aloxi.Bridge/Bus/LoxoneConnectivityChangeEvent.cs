using System;
using System.ComponentModel;

namespace ZoolWay.Aloxi.Bridge.Bus
{
    [ImmutableObject(true)]
    public class LoxoneConnectivityChangeEvent
    {
        public bool IsAvailable { get; }
        public DateTime Timestamp { get; }

        public LoxoneConnectivityChangeEvent(bool isAvailable, DateTime timestamp)
        {
            this.IsAvailable = isAvailable;
            this.Timestamp = timestamp;
        }
    }
}
