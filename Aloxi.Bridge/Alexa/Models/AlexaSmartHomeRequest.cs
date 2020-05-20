using System;
using Newtonsoft.Json.Linq;

namespace ZoolWay.Aloxi.Bridge.Alexa.Models
{
    internal class AlexaSmartHomeRequest
    {
        public AlexaRequestHeader Header { get; set; }
        public JObject Payload { get; set; }
    }
}
