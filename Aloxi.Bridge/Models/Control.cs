using System;
using System.Collections.Immutable;
using System.ComponentModel;

namespace ZoolWay.Aloxi.Bridge.Models
{
    [ImmutableObject(true)]
    public class Control
    {
        public ControlType Type { get; }
        public string FriendlyName { get; }
        public Guid LoxoneUuid { get; }
        public string LoxoneName { get; }
        public ImmutableDictionary<string, Guid> Operations { get; }

        public Control(ControlType type, string friendlyName, Guid loxoneUuid, string loxoneName, ImmutableDictionary<string, Guid> operations)
        {
            this.Type = type;
            this.FriendlyName = friendlyName;
            this.LoxoneUuid = loxoneUuid;
            this.LoxoneName = loxoneName;
            this.Operations = operations;
        }
    }
}
