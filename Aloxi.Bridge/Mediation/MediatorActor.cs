using System;
using System.Collections.Generic;

using Akka.Actor;
using Akka.Event;

using Microsoft.Extensions.Configuration;

namespace ZoolWay.Aloxi.Bridge.Mediation
{
    public class MediatorActor : ReceiveActor
    {
        private class ConfigMqtt
        {
            public string Endpoint { get; set; }
            public string CaPath { get; set; }
            public string CertPath { get; set; }
            public string ClientId { get; set; }
        }

        private class Config
        {
            public MediationClientType[] ActiveClients { get; set; }
            public string SubscriptionTopic { get; set; }
            public string AlexaResponseTopic { get; set; }
            public ConfigMqtt Mqtt { get; set; }
        }

        private readonly ILoggingAdapter log = Logging.GetLogger(Context);
        private readonly MediationConfig mediationConfig;
        private readonly List<MediationMessage> registeredProcessors;
        private IActorRef mqttManager;

        public MediatorActor(IConfigurationSection configuration)
        {
            this.mediationConfig = LoadConfigFrom(configuration);
            this.registeredProcessors = new List<MediationMessage>();
            this.mqttManager = ActorRefs.Nobody;

            SetupMediationClients();

            Receive<MediationMessage.Publish>(ForwardToSubscriptionActor);
            Receive<MediationMessage.PublishAlexaResponse>(ForwardToSubscriptionActor);
            Receive<MediationMessage.RegisterProcessor>(ReceivedRegisterProcessor);
            Receive<MediationMessage.RequestState>(ForwardToSubscriptionActor);
        }

        private void ReceivedRegisterProcessor(MediationMessage.RegisterProcessor message)
        {
            this.registeredProcessors.Add(message);
            if (!this.mqttManager.IsNobody()) this.mqttManager.Forward(message);
        }

        private void ForwardToSubscriptionActor(MediationMessage message)
        {
            bool published = false;
            if (!this.mqttManager.IsNobody())
            {
                this.mqttManager.Forward(message);
                published = true;
            }

            if (!published)
            {
                log.Warning($"No mediation client available for message {message.GetType().Name}");
            }
        }

        private void SetupMediationClients()
        {
            if (this.mediationConfig.ActiveClients.Contains(MediationClientType.Mqtt))
            {
                SetupMqttMediationClient();
            }
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

        private MediationConfig LoadConfigFrom(IConfigurationSection configuration)
        {
            var c = configuration.Get<Config>();
            return new MediationConfig(c.ActiveClients, c.SubscriptionTopic, c.AlexaResponseTopic, new Mqtt.MqttConfig(c.Mqtt.Endpoint, c.Mqtt.CaPath, c.Mqtt.CertPath, c.Mqtt.ClientId));
        }
    }
}
