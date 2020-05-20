using System;
using System.Collections.Generic;

namespace ZoolWay.Aloxi.Bridge.Alexa.Models
{
    internal class AlexaEndpointCapabilityProperties
    {
        public Dictionary<string, string>[] Supported { get; set; }
        public bool ProactivelyReported { get; set; }
        public bool Retrievable { get; set; }
    }
}
