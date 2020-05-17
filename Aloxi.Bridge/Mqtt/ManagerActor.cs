using System;
using Akka.Actor;
using Akka.Event;

namespace ZoolWay.Aloxi.Bridge.Mqtt
{
    public class ManagerActor : ReceiveActor
    {
        private readonly ILoggingAdapter log = Logging.GetLogger(Context);
        private readonly string subscriptionTopic;
        private readonly MqttConfig mqttConfig;
        private IActorRef subscriber;

        public ManagerActor(MqttConfig mqttConfig, string subscriptionTopic)
        {
            this.subscriptionTopic = subscriptionTopic;
            this.mqttConfig = mqttConfig;
            this.subscriber = ActorRefs.Nobody;

            Receive<MqttMessage.Publish>((msg) => this.subscriber.Forward(msg));
            Receive<MqttMessage.Log>(ReceivedLog);
            Receive<MqttMessage.RegisterProcessor>((msg) => this.subscriber.Forward(msg));
        }

        protected override void PreStart()
        {
            log.Info("Starting");
            this.subscriber = Context.ActorOf(Props.Create(() => new SubscriptionActor(Self, this.mqttConfig, this.subscriptionTopic)), $"subscription-{this.subscriptionTopic}");
        }

        protected override void PostStop()
        {
            this.subscriber = ActorRefs.Nobody;
            log.Info("Stopped");
        }

        private void ReceivedLog(MqttMessage.Log message)
        {
            log.Log(message.LogLevel, "{0}: {1}", Sender.Path.ToStringWithoutAddress(), message.Message);
        }
    }
}
