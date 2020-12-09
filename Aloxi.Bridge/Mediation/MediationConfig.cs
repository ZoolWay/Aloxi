using System;
using System.Collections.Immutable;
using System.ComponentModel;

namespace ZoolWay.Aloxi.Bridge.Mediation
{
    [ImmutableObject(true)]
    public class MediationConfig
    {
        public ImmutableArray<MediationClientType> ActiveClients { get; private set; }
        public string SubscriptionTopic { get; private set; }
        public string AlexaResponseTopic { get; private set; }
        public Mqtt.MqttConfig Mqtt { get; private set; }
        public SignalR.Config SignalR { get; private set; }

        public MediationConfig(MediationClientType[] activeClients, string subscriptionTopic, string alexaResponseTopic, Mqtt.MqttConfig mqtt, SignalR.Config signalR)
        {
            this.ActiveClients = activeClients.ToImmutableArray();
            this.SubscriptionTopic = subscriptionTopic;
            this.AlexaResponseTopic = alexaResponseTopic;
            this.Mqtt = mqtt;
            this.SignalR = signalR;
        }
    }
}
