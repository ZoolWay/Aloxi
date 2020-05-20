using System;
using System.ComponentModel;
using ZoolWay.Aloxi.Bridge.Models;

namespace ZoolWay.Aloxi.Bridge.Loxone
{
    [ImmutableObject(true)]
    public abstract class LoxoneMessage
    {
        public class LoadModel : LoxoneMessage
        {
        }

        public class UpdatedModel : LoxoneMessage
        {
            public Home Model { get; }

            public UpdatedModel(Home model)
            {
                this.Model = model;
            }
        }

        public class ControlSwitch : LoxoneMessage
        {
            public enum DesiredStateType { On, Off, ByUuid };

            public string LoxoneUuid { get; }
            public DesiredStateType DesiredState { get; }

            public ControlSwitch(string loxoneUuid,  DesiredStateType desiredState)
            {
                this.LoxoneUuid = loxoneUuid;
                this.DesiredState = desiredState;
            }
        }
    }
}
