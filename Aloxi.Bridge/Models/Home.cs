using System;
using System.Collections.Immutable;
using System.ComponentModel;

namespace ZoolWay.Aloxi.Bridge.Models
{
    [ImmutableObject(true)]
    public class Home
    {
        public ImmutableList<Control> Controls { get; }

        public Home(ImmutableList<Control> controls)
        {
            this.Controls = controls;
        }
    }
}
