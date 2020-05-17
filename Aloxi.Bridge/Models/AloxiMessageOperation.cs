using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ZoolWay.Aloxi.Bridge.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    internal enum AloxiMessageOperation
    {
        [EnumMember(Value = "bridgeAnnouncement")]
        BridgeAnnouncement,
        [EnumMember(Value = "echo")]
        Echo,
        [EnumMember(Value = "echoResponse")]
        EchoResponse,
        [EnumMember(Value = "pipeAlexaRequest")]
        PipeAlexaRequest,
        [EnumMember(Value = "pipeAlexaResponse")]
        PipeAlexaResponse,
        [EnumMember(Value = "unknown")]
        Unknown,
    }
}
