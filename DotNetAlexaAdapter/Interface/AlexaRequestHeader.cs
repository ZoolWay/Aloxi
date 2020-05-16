using System;

namespace ZoolWay.AloxiAlexaAdapter.Interface
{
    internal class AlexaRequestHeader
    {
        public String MessageId { get; set; }
        public String Name { get; set; }
        public String Namespace { get; set; }
        public String PayloadVersion { get; set; }
    }
}
