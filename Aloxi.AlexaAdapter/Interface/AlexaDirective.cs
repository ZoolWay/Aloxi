using System;

using Newtonsoft.Json.Linq;

namespace ZoolWay.Aloxi.AlexaAdapter.Interface
{
    internal class AlexaDirective
    {
        public AlexaDirectiveHeader Header { get; set; }
        public JObject Endpoint { get; set; }
        public JObject Payload { get; set; }
    }
}
