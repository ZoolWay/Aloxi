using System;
using Newtonsoft.Json.Linq;

namespace ZoolWay.AloxiAlexaAdapter.Interface
{
    internal class AlexaSmartHomeRequest
    {
        public AlexaRequestHeader Header { get; set; }
        public JObject Payload { get; set; }
    }
}
