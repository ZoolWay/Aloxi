using System;
using System.Text.Json.Serialization;

using ZoolWay.Aloxi.Bridge.Alexa.Converters;

namespace ZoolWay.Aloxi.Bridge.Alexa.Models
{
    internal class AlexaProperty
    {
        public string Namespace { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        [JsonConverter(typeof(DateTimeUtcConverter))]
        public DateTime TimeOfSample { get; set; }
        public int UncertaintyInMilliseconds { get; set; }

        public AlexaProperty()
        {
            this.UncertaintyInMilliseconds = 500;
        }

        public AlexaProperty(string ns, string name, string value, DateTime timeOfSample) : this()
        {
            this.Namespace = ns;
            this.Name = name;
            this.Value = value;
            this.TimeOfSample = timeOfSample;
        }
    }
}
