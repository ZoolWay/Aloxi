using System;
using System.Text.Json.Serialization;

namespace ZoolWay.Aloxi.Bridge.Alexa.Models
{
    internal class AlexaCapabilityFriendlyName
    {
        [JsonPropertyName("@type")]
        public string Type { get; set; }
        public AlexaCapabilityFriendlyNameValue Value { get; set; }

        public AlexaCapabilityFriendlyName()
        {
        }

        public AlexaCapabilityFriendlyName(string type, string text, string locale)
        {
            this.Type = type;
            this.Value = new AlexaCapabilityFriendlyNameValue() { Text = text, Locale = locale };
        }

        public AlexaCapabilityFriendlyName(string type, string assetId)
        {
            this.Type = type;
            this.Value = new AlexaCapabilityFriendlyNameValue() { AssetId = assetId };
        }
    }
}
