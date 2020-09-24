using System;

namespace ZoolWay.Aloxi.Bridge.Alexa.Models
{
    internal class AlexaDiscoverResponseEvent
    {
        public AlexaEventHeader Header { get; set; }
        public AlexaDiscoverResponsePayload Payload { get; set; }
    }
}
