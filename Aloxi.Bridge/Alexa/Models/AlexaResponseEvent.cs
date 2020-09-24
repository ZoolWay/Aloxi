using System;

using Newtonsoft.Json.Linq;

namespace ZoolWay.Aloxi.Bridge.Alexa.Models
{
    internal class AlexaResponseEvent
    {
        public AlexaEventHeader Header { get; set; }
        public AlexaEventEndpoint Endpoint { get; set; }
        public JObject Payload { get; set; }
    }
}
