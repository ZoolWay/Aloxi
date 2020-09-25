using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Text.Json.Serialization;

using ZoolWay.Aloxi.Bridge.Loxone;

namespace ZoolWay.Aloxi.Bridge.Models
{
    [ImmutableObject(true)]
    public class Control
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ControlType Type { get; }
        public string FriendlyName { get; }
        public LoxoneUuid LoxoneUuid { get; }
        public string LoxoneName { get; }
        public ImmutableDictionary<string, LoxoneUuid> Operations { get; }
        public string RoomName { get; }

        public Control(ControlType type, string friendlyName, LoxoneUuid loxoneUuid, string loxoneName, ImmutableDictionary<string, LoxoneUuid> operations, string roomName)
        {
            this.Type = type;
            this.FriendlyName = friendlyName;
            this.LoxoneUuid = loxoneUuid;
            this.LoxoneName = loxoneName;
            this.Operations = operations;
            this.RoomName = roomName;
        }
    }
}
