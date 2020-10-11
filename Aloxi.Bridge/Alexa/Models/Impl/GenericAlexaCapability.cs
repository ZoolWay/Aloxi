using System;

namespace ZoolWay.Aloxi.Bridge.Alexa.Models.Impl
{
    /// <summary>
    /// This capability is required for every endpoint according to discovery best practices.
    /// Compare: https://github.com/alexa/alexa-smarthome/wiki/Tips-Tricks-and-Insights#tips-about-device-discovery
    /// </summary>
    internal class GenericAlexaCapability : AlexaEndpointCapability
    {
        public override string Interface { get => "Alexa"; }
    }
}
