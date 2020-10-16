using System;

using Newtonsoft.Json;

namespace ZoolWay.Aloxi.Bridge.Alexa.Models
{
    internal class AlexaCapabilitySemanticActionMapping
    {
        public class AmDirectivePayload
        {
            public string Mode { get; set; }
        }

        public class AmDirective
        {
            public string Name { get; set; }
            public AmDirectivePayload Payload { get; set; }

            public AmDirective()
            {
            }

            public AmDirective(string name, string mode)
            {
                this.Name = name;
                this.Payload = new AmDirectivePayload() { Mode = mode };
            }
        }

        [JsonProperty(PropertyName = "@type")]
        public string Type { get; set; }
        public string[] Actions { get; set; }
        public AmDirective Directive { get; set; }
    }
}
