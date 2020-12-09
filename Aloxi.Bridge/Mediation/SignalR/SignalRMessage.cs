using System;

namespace ZoolWay.Aloxi.Bridge.Mediation.SignalR
{
    public abstract class SignalRMessage
    {
        public class RegisterHub : SignalRMessage
        {
            public AloxiHub Hub { get; private set; }

            public RegisterHub(AloxiHub hub)
            {
                this.Hub = hub;
            }
        }

        public class UnregisterHub : SignalRMessage
        {
            public AloxiHub Hub { get; private set; }

            public UnregisterHub(AloxiHub hub)
            {
                this.Hub = hub;
            }
        }

        public class ToBridge : SignalRMessage
        {
            public string MessageType { get; private set; }
            public string Operation { get; private set; }
            public string Payload { get; private set; }

            public ToBridge(string messageType, string operation, string payload)
            {
                this.MessageType = messageType;
                this.Operation = operation;
                this.Payload = payload;
            }
        }
    }
}
