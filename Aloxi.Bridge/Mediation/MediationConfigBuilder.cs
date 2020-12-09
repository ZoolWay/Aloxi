using System;

using Microsoft.Extensions.Configuration;

namespace ZoolWay.Aloxi.Bridge.Mediation
{
    public static class MediationConfigBuilder
    {
        private class ConfigMqtt
        {
            public string Endpoint { get; set; }
            public string CaPath { get; set; }
            public string CertPath { get; set; }
            public string ClientId { get; set; }
        }

        private class ConfigSignalR
        {
            public string ConnectionString { get; set; }
        }

        private class Config
        {
            public MediationClientType[] ActiveClients { get; set; }
            public string SubscriptionTopic { get; set; }
            public string AlexaResponseTopic { get; set; }
            public ConfigMqtt Mqtt { get; set; }
            public ConfigSignalR SignalR { get; set; }
        }

        public static MediationConfig From(IConfigurationSection configurationSection)
        {
            var c = configurationSection.Get<Config>();
            var mMqtt = new Mqtt.MqttConfig(c.Mqtt.Endpoint, c.Mqtt.CaPath, c.Mqtt.CertPath, c.Mqtt.ClientId);
            var mSignalR = new SignalR.Config(c.SignalR.ConnectionString);
            return new MediationConfig(c.ActiveClients, c.SubscriptionTopic, c.AlexaResponseTopic, mMqtt, mSignalR);
        }
    }
}
