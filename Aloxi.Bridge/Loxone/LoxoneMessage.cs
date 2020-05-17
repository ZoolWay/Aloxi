using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace ZoolWay.Aloxi.Bridge.Loxone
{
    [ImmutableObject(true)]
    public abstract class LoxoneMessage
    {
        public class LoadModel : LoxoneMessage
        {
        }
    }
}
