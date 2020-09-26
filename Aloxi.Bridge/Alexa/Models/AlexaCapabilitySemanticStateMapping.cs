using System;
using System.Text.Json.Serialization;

namespace ZoolWay.Aloxi.Bridge.Alexa.Models
{
    internal class AlexaCapabilitySemanticStateMapping
    {
        [JsonPropertyName("@type")]
        public string Type { get; set; }
        public string[] States { get; set; }
        public string Value { get; set; }
    }
}
