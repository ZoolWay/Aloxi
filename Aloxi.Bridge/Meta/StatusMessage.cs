using System;
using System.ComponentModel;

namespace ZoolWay.Aloxi.Bridge.Meta
{
    [ImmutableObject(true)]
    public abstract class StatusMessage
    {
        public class Request : StatusMessage
        {
        }

        public class Response : StatusMessage
        {
            public bool IsReady { get; }
            public bool IsMqttConnected { get; }
            public bool IsMqttSubscribed { get; }
            public bool IsMiniserverAvailable { get; }
            public DateTime UpdateTimestamp { get; }

            public Response(bool isReady, bool isMqttConnected, bool isMqttSubscribed, bool isMiniserverAvailable, DateTime updateTimestamp)
            {
                this.IsReady = isReady;
                this.IsMqttConnected = isMqttConnected;
                this.IsMqttSubscribed = isMqttSubscribed;
                this.IsMiniserverAvailable = isMiniserverAvailable;
                this.UpdateTimestamp = updateTimestamp;
            }
        }

        public class Init : StatusMessage
        {
        }
    }
}
