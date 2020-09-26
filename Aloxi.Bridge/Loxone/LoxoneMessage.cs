using System;
using System.ComponentModel;
using ZoolWay.Aloxi.Bridge.Models;

namespace ZoolWay.Aloxi.Bridge.Loxone
{
    [ImmutableObject(true)]
    public abstract class LoxoneMessage
    {
        public class InitAdapter : LoxoneMessage
        {
        }

        public class LoadModel : LoxoneMessage
        {
        }

        public class PublishModel : LoxoneMessage
        {
            public Home Model { get; }
            public DateTime UpdateTimestamp { get; }

            public PublishModel(Home model, DateTime updateTimestamp)
            {
                this.Model = model;
                this.UpdateTimestamp = updateTimestamp;
            }
        }

        public class RequestModel : LoxoneMessage
        {
        }

        public class ControlSwitch : LoxoneMessage
        {
            public enum DesiredStateType { On, Off, ByUuid };

            public string LoxoneUuid { get; }
            public DesiredStateType DesiredState { get; }

            public ControlSwitch(string loxoneUuid, DesiredStateType desiredState)
            {
                this.LoxoneUuid = loxoneUuid;
                this.DesiredState = desiredState;
            }
        }

        public class ControlDimmer : LoxoneMessage
        {
            public enum DimType { Set, Adjust };

            public string LoxoneUuid { get; }
            public DimType Type { get; }
            public int Value { get; }

            public ControlDimmer(string loxoneUuid, DimType dimType, int value)
            {
                this.LoxoneUuid = loxoneUuid;
                this.Type = dimType;
                this.Value = value;
            }
        }

        public class ControlBlinds : LoxoneMessage
        {
            public enum BlindCmd { FullUp, FullDown, Stop };

            public string LoxoneUuid { get; }
            public BlindCmd Command { get; }

            public ControlBlinds(string loxoneUuid, BlindCmd command)
            {
                this.LoxoneUuid = loxoneUuid;
                this.Command = command;
            }
        }

        public class TestAvailability : LoxoneMessage
        {
        }

        public class ReportAvailability : LoxoneMessage
        {
            public bool IsMiniserverAvailable { get; }
            public DateTime Timestamp { get; }

            public ReportAvailability(bool isMiniserverAvailable, DateTime timestamp)
            {
                this.IsMiniserverAvailable = isMiniserverAvailable;
                this.Timestamp = timestamp;
            }
        }
    }
}
