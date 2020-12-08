using System;

using Akka.Actor;
using Akka.Event;

using ZoolWay.Aloxi.Bridge.Bus;
using ZoolWay.Aloxi.Bridge.Loxone;
using ZoolWay.Aloxi.Bridge.Mediation;
using ZoolWay.Aloxi.Bridge.Models;

namespace ZoolWay.Aloxi.Bridge.Meta
{
    public class StatusConsolidatorActor : ReceiveActor
    {
        private readonly ILoggingAdapter log = Context.GetLogger();
        private readonly IActorRef loxoneAdapter;
        private readonly IActorRef mediator;
        private bool receivedModel;
        private bool receivedMqtt;
        private bool receivedLoxone;
        private Home homeModel;
        private DateTime homeModelUpdate;
        private bool isMqttConnected;
        private bool isMqttSubscribed;
        private DateTime mqttUpdate;
        private bool isMiniserverAvailable;
        private DateTime miniserverUpdate;

        public StatusConsolidatorActor(IActorRef loxoneAdapter, IActorRef mediator)
        {
            this.loxoneAdapter = loxoneAdapter;
            this.mediator = mediator;

            this.receivedModel = false;
            this.receivedMqtt = false;
            this.receivedLoxone = false;

            Receive<Bus.HomeModelUpdatedEvent>(ReceivedHomeModelUpdated);
            Receive<Bus.MqttConnectivityChangeEvent>(ReceivedMqttConnectivityChange);
            Receive<Bus.LoxoneConnectivityChangeEvent>(ReceivedLoxoneConnectivityChange);
            Receive<StatusMessage.Request>(ReceivedStatusRequest);
            Receive<LoxoneMessage.PublishModel>(ReceivedPublishModel);
            Receive<MediationMessage.CurrentState>(ReceivedCurrentMqttState);
            Receive<StatusMessage.Init>(ReceivedInit);
        }

        private void ReceivedInit(StatusMessage.Init message)
        {
            this.loxoneAdapter.Tell(new LoxoneMessage.RequestModel());
            this.mediator.Tell(new MediationMessage.RequestState());
        }

        protected override void PreStart()
        {
            Context.System.EventStream.Subscribe<HomeModelUpdatedEvent>(Self);
            Context.System.EventStream.Subscribe<MqttConnectivityChangeEvent>(Self);
            Context.System.EventStream.Subscribe<LoxoneConnectivityChangeEvent>(Self);
            Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(10), Self, new StatusMessage.Init(), Self);
        }

        protected override void PostStop()
        {
            Context.System.EventStream.Unsubscribe<HomeModelUpdatedEvent>(Self);
            Context.System.EventStream.Unsubscribe<MqttConnectivityChangeEvent>(Self);
            Context.System.EventStream.Unsubscribe<LoxoneConnectivityChangeEvent>(Self);
        }

        private void ReceivedHomeModelUpdated(Bus.HomeModelUpdatedEvent message)
        {
            this.receivedModel = true;
            this.receivedLoxone = true;
            this.homeModel = message.HomeModel;
            this.homeModelUpdate = message.UpdateTimestamp;
            this.miniserverUpdate = message.UpdateTimestamp;
        }

        private void ReceivedLoxoneConnectivityChange(Bus.LoxoneConnectivityChangeEvent message)
        {
            this.receivedLoxone = true;
            this.isMiniserverAvailable = message.IsAvailable;
            this.miniserverUpdate = message.Timestamp;
        }

        private void ReceivedPublishModel(LoxoneMessage.PublishModel message)
        {
            if (this.receivedModel) return; // ignore pull if we already got data
            this.receivedModel = true;
            this.homeModel = message.Model;
            this.homeModelUpdate = message.UpdateTimestamp;
        }

        private void ReceivedMqttConnectivityChange(Bus.MqttConnectivityChangeEvent message)
        {
            this.receivedMqtt = true;
            this.isMqttConnected = message.IsConnected;
            this.isMqttSubscribed = message.IsSubscribed;
            this.mqttUpdate = message.UpdateTimestamp;
        }

        private void ReceivedCurrentMqttState(MediationMessage.CurrentState message)
        {
            if (this.receivedMqtt) return; // ignore pull if we already got data
            this.receivedMqtt = true;
            this.isMqttConnected = message.IsConnected;
            this.isMqttSubscribed = message.IsSubscribed;
            this.mqttUpdate = message.Timestamp;
        }

        private void ReceivedStatusRequest(StatusMessage.Request message)
        {
            if (!AllInitialized())
            {
                Sender.Tell(new StatusMessage.Response(false, false, false, false, DateTime.Now));
            }
            else
            {
                DateTime latestUpdate = GetLatestUpdateTimestamp();
                var response = new StatusMessage.Response(true, this.isMqttConnected, this.isMqttSubscribed, this.isMiniserverAvailable, latestUpdate);
                Sender.Tell(response);
            }
        }

        private bool AllInitialized()
        {
            return this.receivedMqtt && this.receivedLoxone && this.receivedModel;
        }

        private DateTime GetLatestUpdateTimestamp()
        {
            DateTime v = DateTime.MinValue;
            if (this.homeModelUpdate > v) v = this.homeModelUpdate;
            if (this.mqttUpdate > v) v = this.mqttUpdate;
            if (this.miniserverUpdate > v) v = this.miniserverUpdate;
            return v;
        }
    }
}
