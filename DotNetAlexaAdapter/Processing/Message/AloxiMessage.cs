using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ZoolWay.AloxiAlexaAdapter.Processing.Meta
{
    internal class AloxiMessage
    {
        public AloxiMessageType Type { get; set; }
        public AloxiMessageOperation Operation { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public String ResponseTopic { get; set; }
        public JObject Data { get; set; }

        public static AloxiMessage Build(AloxiMessageOperation operation, JObject payload, String responseTopic = null)
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
