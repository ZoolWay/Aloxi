using System;

namespace ZoolWay.Aloxi.Bridge.Alexa.Models
{
    internal class AlexaEndpointCapability
    {
        public virtual string Type { get; set; }
        public virtual string Interface { get; set; }
        public virtual string Version { get; set; }
        public virtual AlexaEndpointCapabilityProperties Properties { get; set; }
    }
}
