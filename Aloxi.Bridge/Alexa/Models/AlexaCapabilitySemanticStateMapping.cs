using System;

using Newtonsoft.Json;

namespace ZoolWay.Aloxi.Bridge.Alexa.Models
{
    internal class AlexaCapabilitySemanticStateMapping
    {
        [JsonProperty(PropertyName = "@type")]
        public string Type { get; set; }
        public string[] States { get; set; }
        public string Value { get; set; }
    }
}
