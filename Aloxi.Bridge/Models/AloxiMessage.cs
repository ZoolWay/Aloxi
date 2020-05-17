using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ZoolWay.Aloxi.Bridge.Models
{
    internal class AloxiMessage
    {
        public AloxiMessageType Type { get; set; }
        public AloxiMessageOperation Operation { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public string ResponseTopic { get; set; }
        public JObject Data { get; set; }

        public static AloxiMessage Build(AloxiMessageOperation operation, JObject payload, string responseTopic = null)
        {
            var msg = new AloxiMessage()
            {
                Type = AloxiMessageType.AloxiComm,
                Operation = operation,
                Data = payload,
                ResponseTopic = responseTopic,
            };
            return msg;
        }
    }
}
