﻿using System;
using System.Collections.Generic;

namespace ZoolWay.Aloxi.Bridge.Alexa.Models.Impl
{
    /// <summary>
    /// Capability for a device which can be turned on and off (e.g. light switch).
    /// </summary>
    internal class PowerControllerCapability : AlexaEndpointCapability
    {
        public override string Interface { get => "Alexa.PowerController"; }

        public PowerControllerCapability()
        {
            this.Properties = new AlexaEndpointCapabilityProperties()
            {
                Supported = new[] 
                {
                    new Dictionary<string, string>()
                    {
                        { "name", "powerState" }
                    },
                },
                ProactivelyReported = false,
                Retrievable = false,
            };
        }
    }
}
