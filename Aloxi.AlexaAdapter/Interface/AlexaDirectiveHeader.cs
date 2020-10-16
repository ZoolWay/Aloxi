using System;

namespace ZoolWay.Aloxi.AlexaAdapter.Interface
{
    internal class AlexaDirectiveHeader
    {
        public string Namespace { get; set; }
        public string Instance { get; set; }
        public string Name { get; set; }
        public string MessageId { get; set; }
        public string CorrelationId { get; set; }
        public string PayloadVersion { get; set; }
    }
}
