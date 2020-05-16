using System;

namespace ZoolWay.Aloxi.Bridge.Api.Models
{
    public class Status
    {
        public bool MqttConnected { get; set; }
        public bool MqttSubscribed { get; set; }
        public bool MiniserverAvailable { get; set; }
        public DateTime UpdateTimestamp { get; set; }
    }
}
