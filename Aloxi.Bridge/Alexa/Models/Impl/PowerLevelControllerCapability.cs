using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZoolWay.Aloxi.Bridge.Alexa.Models.Impl
{
    internal class PowerLevelControllerCapability : AlexaEndpointCapability
    {
        public override string Interface { get => "Alexa.PowerLevelController"; }

        public PowerLevelControllerCapability()
        {
            this.Properties = new AlexaEndpointCapabilityProperties()
            {
                Supported = new[]
                {
                    new Dictionary<string, string>()
                    {
                        { "name", "powerLevel" }
                    },
                },
                ProactivelyReported = false,
                Retrievable = false,
            };
        }
    }
}
