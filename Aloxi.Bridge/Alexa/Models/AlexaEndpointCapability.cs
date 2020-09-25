using System;

namespace ZoolWay.Aloxi.Bridge.Alexa.Models
{
    internal class AlexaEndpointCapability
    {
        public virtual string Type { get => "AlexaInterface"; }
        public virtual string Interface { get; set; }
        public virtual string Version { get => "3"; }
        public virtual AlexaEndpointCapabilityProperties Properties { get; set; }
    }
}
