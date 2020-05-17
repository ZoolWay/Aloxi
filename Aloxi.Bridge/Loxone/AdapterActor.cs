using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;

namespace ZoolWay.Aloxi.Bridge.Loxone
{
    public class AdapterActor : ReceiveActor
    {
        private readonly ILoggingAdapter log = Logging.GetLogger(Context);
        private readonly LoxoneConfig loxoneConfig;

        public AdapterActor(LoxoneConfig loxoneConfig)
        {
            this.loxoneConfig = loxoneConfig;
        }

        protected override void PreStart()
        {
            Context.ActorOf(Props.Create(() => new ModelLoaderActor(this.loxoneConfig)));
        }
    }
}
