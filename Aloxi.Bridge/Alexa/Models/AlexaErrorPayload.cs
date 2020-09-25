using System;
using System.Text.Json.Serialization;

namespace ZoolWay.Aloxi.Bridge.Alexa.Models
{
    public class AlexaErrorPayload
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AlexaErrorType Type { get; set; }
        public string Message { get; set; }
    }
}
