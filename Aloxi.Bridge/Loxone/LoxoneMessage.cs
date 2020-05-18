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
    }
}
