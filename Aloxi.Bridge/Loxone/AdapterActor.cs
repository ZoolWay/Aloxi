using System;
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
        private IActorRef sender;
        private Home model;
        private ICancelable scheduledModelUpdates;

        public AdapterActor(LoxoneConfig loxoneConfig)
        {
            this.loxoneConfig = loxoneConfig;
            this.modelLoader = ActorRefs.Nobody;
            this.sender = ActorRefs.Nobody;

            Receive<LoxoneMessage.ControlSwitch>(m => this.sender.Forward(m));
            Receive<LoxoneMessage.UpdatedModel>(ReceivedUpdatedModel);
            Receive<LoxoneMessage.InitAdapter>(ReceivedInitAdapter);
        }

        protected override void PreStart()
        {
            this.modelLoader = Context.ActorOf(Props.Create(() => new ModelLoaderActor(this.loxoneConfig)));
            this.sender = Context.ActorOf(Props.Create(() => new SenderActor(this.loxoneConfig)));
        }

        protected override void PostStop()
        {
            this.scheduledModelUpdates.CancelIfNotNull();
            this.modelLoader = ActorRefs.Nobody;
            this.sender = ActorRefs.Nobody;
        }

        private void ReceivedUpdatedModel(LoxoneMessage.UpdatedModel message)
        {
            this.model = message.Model;
            Context.System.EventStream.Publish(new Bus.HomeModelUpdatedEvent(message.Model));
            log.Info("Got model with {0} controls", this.model.Controls.Count);
        }

        private void ReceivedInitAdapter(LoxoneMessage.InitAdapter message)
        {
            log.Info("Initializing adapter");
            this.scheduledModelUpdates = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(0, 12 * 3600 * 1000, this.modelLoader, new LoxoneMessage.LoadModel(), Self);
        }

    }
}
