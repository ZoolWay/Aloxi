using System;
using System.Collections.Generic;

using Akka.Actor;
using Akka.Event;

using Microsoft.Extensions.Configuration;

namespace ZoolWay.Aloxi.Bridge.Mediation
{
    public class MediatorActor : ReceiveActor
    {
        private readonly ILoggingAdapter log = Logging.GetLogger(Context);
        private readonly MediationConfig mediationConfig;
        private readonly List<MediationMessage> registeredProcessors;
        private IActorRef mqttManager;
        private IActorRef signalRManager;

        public MediatorActor(IConfigurationSection configuration)
        {
            this.mediationConfig = MediationConfigBuilder.From(configuration);
            this.registeredProcessors = new List<MediationMessage>();
            this.mqttManager = ActorRefs.Nobody;
            this.signalRManager = ActorRefs.Nobody;

            SetupMediationClients();

            Receive<MediationMessage.Publish>(ForwardToMediationClientActor);
            Receive<MediationMessage.PublishAlexaResponse>(ForwardToMediationClientActor);
            Receive<MediationMessage.RegisterProcessor>(ReceivedRegisterProcessor);
            Receive<MediationMessage.RequestState>(ForwardToMediationClientActor);
            Receive<Mqtt.MqttMessage>(ForwardMqttMessage);
            Receive<SignalR.SignalRMessage>(ForwardSignalRMessage);
        }

        private void ReceivedRegisterProcessor(MediationMessage.RegisterProcessor message)
        {
            this.registeredProcessors.Add(message);
            if (!this.mqttManager.IsNobody()) this.mqttManager.Forward(message);
            if (!this.signalRManager.IsNobody()) this.signalRManager.Forward(message);
        }

        private void ForwardToMediationClientActor(MediationMessage message)
        {
            bool published = false;
            if (!this.mqttManager.IsNobody())
            {
                this.mqttManager.Forward(message);
                published = true;
            }
            if (!this.signalRManager.IsNobody())
            {
                this.signalRManager.Forward(message);
                published = true;
            }

            if (!published)
            {
                log.Warning($"No mediation client available for message {message.GetType().Name}");
            }
        }

        private void ForwardSignalRMessage(SignalR.SignalRMessage message)
        {
            if (this.signalRManager.IsNobody())
            {
                log.Warning($"Must discard message because no SignalR manager is active: {message.GetType().Name}");
                return;
            }
            this.signalRManager.Forward(message);
        }

        private void ForwardMqttMessage(Mqtt.MqttMessage message)
        {
            if (this.mqttManager.IsNobody())
            {
                log.Warning($"Must discard message because no Mqtt manager is active: {message.GetType().Name}");
                return;
            }
            this.mqttManager.Forward(message);
        }

        private void SetupMediationClients()
        {
            if (this.mediationConfig.ActiveClients.Contains(MediationClientType.Mqtt))
            {
                SetupMqttMediationClient();
            }
            if (this.mediationConfig.ActiveClients.Contains(MediationClientType.SignalR))
            {
                SetupSignalRMediationClient();
            }
        }

        private void SetupSignalRMediationClient()
        {
            var signalRConfig = new SignalR.Config(mediationConfig.SignalR.ConnectionString);
            this.signalRManager = Context.ActorOf(Props.Create(() => new SignalR.ManagerActor(signalRConfig)), "signalr");
        }

        private void SetupMqttMediationClient()
        {
            try
            {
                var mqttConfig = new Mqtt.MqttConfig(mediationConfig.Mqtt.Endpoint, mediationConfig.Mqtt.CaCertPath, mediationConfig.Mqtt.ClientCertPath, mediationConfig.Mqtt.ClientId);
                this.mqttManager = Context.ActorOf(Props.Create(() => new Mqtt.ManagerActor(mqttConfig, mediationConfig.SubscriptionTopic, mediationConfig.AlexaResponseTopic)), "mqtt");
            }
            catch (Exception ex)
            {
                log.Error(ex, "Initializing of MQTT node failed");
            }
        }
    }
}
