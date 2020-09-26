using System;
using System.Collections.Generic;

namespace ZoolWay.Aloxi.Bridge.Alexa.Models.Impl
{
    internal class ModeControllerCapability : AlexaEndpointCapability
    {
        public override string Interface { get => "Alexa.ModeController"; }
        public virtual string Instance { get; }
        public AlexaCapabilityResources CapabilityResources { get; set; }
        public AlexaCapabilitySemantics Semantics { get; set; }

        public ModeControllerCapability()
        {
            this.Properties = new AlexaEndpointCapabilityPropertiesForModeController()
            {
                Supported = new[]
                {
                    new Dictionary<string, string>()
                    {
                        { "name", "mode" }
                    },
                },
                ProactivelyReported = false,
                Retrievable = false,
                NonControllable = false,
            };
        }
    }
}
