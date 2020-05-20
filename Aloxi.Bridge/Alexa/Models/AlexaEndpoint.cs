using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace ZoolWay.Aloxi.Bridge.Alexa.Models
{
    internal class AlexaEndpoint
    {
        public string EndpointId { get; set; }
        public string ManufacturerName { get; set; }
        public string Description { get; set; }
        public string FriendlyName { get; set; }
        public Dictionary<string, string> AdditionalAttributes { get; set; }
        public string[] DisplayCategories { get; set; }
        public AlexaEndpointCapability[] Capabilities { get; set; }
    }
}
