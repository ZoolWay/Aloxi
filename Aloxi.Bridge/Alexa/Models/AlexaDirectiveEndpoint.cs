using System;
using System.Collections.Generic;

namespace ZoolWay.Aloxi.Bridge.Alexa.Models
{
    internal class AlexaDirectiveEndpoint
    {
        public string EndpointId { get; set; }
        public Dictionary<string, string> Cookie { get; set; }
    }
}
