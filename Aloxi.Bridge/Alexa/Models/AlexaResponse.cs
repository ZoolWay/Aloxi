using System;

namespace ZoolWay.Aloxi.Bridge.Alexa.Models
{
    internal class AlexaResponse
    {
        public AlexaResponseEvent Event { get; set; }
        public AlexaContext Context { get; set; }
    }
}
