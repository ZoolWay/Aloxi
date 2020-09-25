using System;

namespace ZoolWay.Aloxi.Bridge.Alexa.Models
{
    internal class AlexaErrorEvent
    {
        public AlexaEventHeader Header { get; set; }
        public AlexaEventEndpoint Endpoint { get; set; }
        public AlexaErrorPayload Payload { get; set; }
    }
}
