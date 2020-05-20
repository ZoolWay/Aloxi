using System;

namespace ZoolWay.Aloxi.Bridge.Alexa.Models
{
    internal class AlexaControlRequestPayload
    {
        public string AccessToken { get; set; }
        public AlexaAppliance Appliance { get; set; }
    }
}
