using System;

using Newtonsoft.Json.Linq;

namespace ZoolWay.Aloxi.Bridge.Alexa.Models
{
    internal class AlexaDirective
    {
        public AlexaDirectiveHeader Header { get; set; }
        public AlexaDirectiveEndpoint Endpoint { get; set; }
        public JObject Payload { get; set; }
    }
}
