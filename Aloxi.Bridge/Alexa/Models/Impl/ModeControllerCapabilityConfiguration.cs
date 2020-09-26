using System;
using System.Collections.Generic;

namespace ZoolWay.Aloxi.Bridge.Alexa.Models.Impl
{
    internal class ModeControllerCapabilityConfiguration
    {
        public class ModeResources
        {
            public List<AlexaCapabilityFriendlyName> FriendlyNames { get; set; }
        }

        public class ModeValue
        {
            public string Value { get; set; }
            public ModeResources ModeResources { get; set; }
        }

        public bool Ordered { get; set; }
        public List<ModeValue> SupportedModes { get; set; }
    }
}
