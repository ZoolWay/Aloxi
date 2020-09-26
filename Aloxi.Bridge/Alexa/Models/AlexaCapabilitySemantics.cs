using System;
using System.Collections.Generic;

namespace ZoolWay.Aloxi.Bridge.Alexa.Models
{
    internal class AlexaCapabilitySemantics
    {
        public List<AlexaCapabilitySemanticActionMapping> ActionMappings { get; set; }
        public List<AlexaCapabilitySemanticStateMapping> StateMappings { get; set; }
    }
}
