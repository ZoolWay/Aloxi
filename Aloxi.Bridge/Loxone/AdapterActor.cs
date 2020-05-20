using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.Event;
using ZoolWay.Aloxi.Bridge.Models;

namespace ZoolWay.Aloxi.Bridge.Loxone
{
    public class AdapterActor : ReceiveActor
    {
        private readonly ILoggingAdapter log = Logging.GetLogger(Context);
        private readonly LoxoneConfig loxoneConfig;
        private IActorRef modelLoader;
        private Home model;
        private ICancelable scheduledModelUpdates;

        public AdapterActor(LoxoneConfig loxoneConfig)
        {
            this.loxoneConfig = loxoneConfig;
            this.modelLoader = ActorRefs.Nobody;

            Receive<LoxoneMessage.UpdatedModel>(ReceivedUpdatedModel);
        }

        protected override void PreStart()
        {
            this.modelLoader = Context.ActorOf(Props.Create(() => new ModelLoaderActor(this.loxoneConfig)));
            this.scheduledModelUpdates = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(0, 12 * 60 * 60 * 1000, this.modelLoader, new LoxoneMessage.LoadModel(), Self);
        }

        protected override void PostStop()
        {
            this.scheduledModelUpdates.CancelIfNotNull();
            this.modelLoader = ActorRefs.Nobody;
        }

        private void ReceivedUpdatedModel(LoxoneMessage.UpdatedModel message)
        {
            this.model = message.Model;
            Context.System.EventStream.Publish(new Bus.HomeModelUpdatedEvent(message.Model));
            log.Info("Got model with {0} controls", this.model.Controls.Count);
        }
    }
}
