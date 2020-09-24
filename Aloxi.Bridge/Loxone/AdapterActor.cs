using System;
using Akka.Actor;
using Akka.Event;
using ZoolWay.Aloxi.Bridge.Models;

namespace ZoolWay.Aloxi.Bridge.Loxone
{
    public class AdapterActor : ReceiveActor
    {
        private const int INTERVAL_MODEL_UPDATE_MS = 3600 * 1000; // once per hour
        private const int INTERVAL_AVAILABILITY_TEST_MS = 5 * 60 * 1000; // every 5 minutes
        private readonly ILoggingAdapter log = Logging.GetLogger(Context);
        private readonly LoxoneConfig loxoneConfig;
        private IActorRef modelLoader;
        private IActorRef sender;
        private Home model;
        private DateTime modelUpdateTimestamp;
        private ICancelable scheduledModelUpdates;
        private ICancelable scheduledAvailabilityTests;
        private bool? lastIsMiniserverAvailable;

        public AdapterActor(LoxoneConfig loxoneConfig)
        {
            this.loxoneConfig = loxoneConfig;
            this.modelLoader = ActorRefs.Nobody;
            this.sender = ActorRefs.Nobody;

            Receive<LoxoneMessage.ControlSwitch>(m => this.sender.Forward(m));
            Receive<LoxoneMessage.PublishModel>(ReceivedUpdatedModel);
            Receive<LoxoneMessage.ReportAvailability>(ReceivedReportAvailability);
            Receive<LoxoneMessage.InitAdapter>(ReceivedInitAdapter);
            Receive<LoxoneMessage.RequestModel>(ReceivedRequestModel);
        }

        protected override void PreStart()
        {
            this.modelLoader = Context.ActorOf(Props.Create(() => new ModelLoaderActor(this.loxoneConfig, Self)));
            this.sender = Context.ActorOf(Props.Create(() => new SenderActor(this.loxoneConfig, Self)));
        }

        protected override void PostStop()
        {
            this.scheduledModelUpdates.CancelIfNotNull();
            this.scheduledAvailabilityTests.CancelIfNotNull();
            this.modelLoader = ActorRefs.Nobody;
            this.sender = ActorRefs.Nobody;
        }

        private void ReceivedReportAvailability(LoxoneMessage.ReportAvailability message)
        {
            if ((lastIsMiniserverAvailable.HasValue) && (lastIsMiniserverAvailable.Value == message.IsMiniserverAvailable)) return;
            lastIsMiniserverAvailable = message.IsMiniserverAvailable;
            Context.System.EventStream.Publish(new Bus.LoxoneConnectivityChangeEvent(message.IsMiniserverAvailable, message.Timestamp));
        }

        private void ReceivedUpdatedModel(LoxoneMessage.PublishModel message)
        {
            this.model = message.Model;
            this.modelUpdateTimestamp = message.UpdateTimestamp;
            Context.System.EventStream.Publish(new Bus.HomeModelUpdatedEvent(message.Model, message.UpdateTimestamp));
            log.Info("Got model with {0} controls", this.model.Controls.Count);
        }

        private void ReceivedRequestModel(LoxoneMessage.RequestModel message)
        {
            Sender.Tell(new LoxoneMessage.PublishModel(this.model, this.modelUpdateTimestamp));
        }

        private void ReceivedInitAdapter(LoxoneMessage.InitAdapter message)
        {
            log.Info("Initializing adapter");
            this.scheduledModelUpdates = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(0, INTERVAL_MODEL_UPDATE_MS, this.modelLoader, new LoxoneMessage.LoadModel(), Self);
            this.scheduledAvailabilityTests = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(INTERVAL_AVAILABILITY_TEST_MS, INTERVAL_AVAILABILITY_TEST_MS, this.sender, new LoxoneMessage.TestAvailability(), Self);
        }
    }
}
