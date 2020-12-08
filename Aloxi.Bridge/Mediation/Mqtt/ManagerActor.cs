using System;
using System.Collections.Generic;

using Akka.Actor;
using Akka.Event;
using Akka.Pattern;

namespace ZoolWay.Aloxi.Bridge.Mediation.Mqtt
{
    public class ManagerActor : ReceiveActor
    {
        private readonly ILoggingAdapter log = Context.GetLogger();
        private readonly string subscriptionTopic;
        private readonly string alexaResponseTopic;
        private readonly MqttConfig mqttConfig;
        private readonly List<MediationMessage> registeredProcessors;
        private IActorRef subscriber;

        public ManagerActor(MqttConfig mqttConfig, string subscriptionTopic, string alexaResponseTopic)
        {
            this.subscriptionTopic = subscriptionTopic;
            this.alexaResponseTopic = alexaResponseTopic;
            this.mqttConfig = mqttConfig;
            this.subscriber = ActorRefs.Nobody;
            this.registeredProcessors = new List<MediationMessage>();

            Receive<MediationMessage.Publish>(ForwardToSubscriptionActor);
            Receive<MediationMessage.PublishAlexaResponse>(ForwardToSubscriptionActor);
            Receive<MediationMessage.RegisterProcessor>(ReceivedRegisterProcessor);
            Receive<MediationMessage.RequestState>(ForwardToSubscriptionActor);
            Receive<MqttMessage.EstablishSubscription>(ReceivedEstablishSubscription);
            Receive<Terminated>(ReceivedSubscriberTerminated);
        }

        protected override void PreStart()
        {
            log.Info("Starting");
            Self.Tell(new MqttMessage.EstablishSubscription());
        }

        protected override void PostStop()
        {
            if (!this.subscriber.IsNobody())
            {
                Context.Unwatch(this.subscriber);
                this.subscriber = ActorRefs.Nobody;
            }
            log.Info("Stopped");
        }

        private void ReceivedEstablishSubscription(MqttMessage.EstablishSubscription message)
        {
            log.Info($"Establishing subscription actor (currently {this.registeredProcessors.Count} processors registered)");
            if (!this.subscriber.IsNobody())
            {
                Context.Unwatch(this.subscriber);
            }
            var subscriberProps = Props.Create(() => new SubscriptionActor(Self, this.mqttConfig, this.subscriptionTopic, this.alexaResponseTopic));
            var supervisedProps = BackoffSupervisor.Props(
                Backoff.OnFailure(
                    subscriberProps,
                    childName: $"subscription-{this.subscriptionTopic}",
                    minBackoff: TimeSpan.FromSeconds(3),
                    maxBackoff: TimeSpan.FromSeconds(600),
                    randomFactor: 0.2,
                    maxNrOfRetries: 10
                ));
            this.subscriber = Context.ActorOf(supervisedProps, $"supervisor-sub-{this.subscriptionTopic}");
            Context.Watch(this.subscriber);
            this.registeredProcessors.ForEach(this.subscriber.Tell);
        }

        private void ReceivedSubscriberTerminated(Terminated message)
        {
            log.Warning($"Termination of actor detected: {message.ActorRef}");
            if (message.ActorRef != this.subscriber) return;
            log.Info("Subscription actor was terminated! We try to start over in 15 minutes!");
            this.subscriber = ActorRefs.Nobody;
            Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromMinutes(15), Self, new MqttMessage.EstablishSubscription(), Self);
        }

        private void ReceivedRegisterProcessor(MediationMessage.RegisterProcessor message)
        {
            log.Info($"Registering processor for op '{message.Operation}': {message.Processor.Path.ToStringWithoutAddress()}");
            this.registeredProcessors.Add(message);
            if (!this.subscriber.IsNobody())
            {
                this.registeredProcessors.ForEach(this.subscriber.Tell);
            }
        }

        private void ForwardToSubscriptionActor(MediationMessage message)
        {
            if (this.subscriber.IsNobody())
            {
                log.Warning($"Message '{message.GetType().Name}' is lost, subscription is inactive!");
                return;
            }
            this.subscriber.Forward(message);
        }
    }
}
