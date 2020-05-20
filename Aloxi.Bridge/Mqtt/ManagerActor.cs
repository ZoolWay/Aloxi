using System;
using Akka.Actor;
using Akka.Event;
using Akka.Pattern;

namespace ZoolWay.Aloxi.Bridge.Mqtt
{
    public class ManagerActor : ReceiveActor
    {
        private readonly ILoggingAdapter log = Logging.GetLogger(Context);
        private readonly string subscriptionTopic;
        private readonly string alexaResponseTopic;
        private readonly MqttConfig mqttConfig;
        private IActorRef subscriber;

        public ManagerActor(MqttConfig mqttConfig, string subscriptionTopic, string alexaResponseTopic)
        {
            this.subscriptionTopic = subscriptionTopic;
            this.alexaResponseTopic = alexaResponseTopic;
            this.mqttConfig = mqttConfig;
            this.subscriber = ActorRefs.Nobody;

            Receive<MqttMessage.Publish>((msg) => this.subscriber.Forward(msg));
            Receive<MqttMessage.PublishAlexaResponse>((msg) => this.subscriber.Forward(msg));
            Receive<MqttMessage.Log>(ReceivedLog);
            Receive<MqttMessage.RegisterProcessor>((msg) => this.subscriber.Forward(msg));
        }

        protected override void PreStart()
        {
            log.Info("Starting");

            var subscriberProps = Props.Create(() => new SubscriptionActor(Self, this.mqttConfig, this.subscriptionTopic, this.alexaResponseTopic));
            var supervisedProps = BackoffSupervisor.Props(
                Backoff.OnFailure(
                    subscriberProps,
                    childName: $"subscription-{this.subscriptionTopic}",
                    minBackoff: TimeSpan.FromSeconds(3),
                    maxBackoff: TimeSpan.FromSeconds(60),
                    randomFactor: 0.2,
                    maxNrOfRetries: 5
                ));
            this.subscriber = Context.ActorOf(supervisedProps, $"supervisor-sub-{this.subscriptionTopic}");
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
