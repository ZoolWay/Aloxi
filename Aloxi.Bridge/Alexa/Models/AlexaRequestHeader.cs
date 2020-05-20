using System;

namespace ZoolWay.Aloxi.Bridge.Alexa.Models
{
    internal class AlexaRequestHeader
    {
        public string MessageId { get; set; }
        public string Name { get; set; }
        public string Namespace { get; set; }
        public string PayloadVersion { get; set; }
    }
}
