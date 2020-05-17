using System;
using System.ComponentModel;

namespace ZoolWay.Aloxi.Bridge.Mqtt
{
    [ImmutableObject(true)]
    public class MqttConfig
    {
        public string Endpoint { get; private set; }
        public string CaCertPath { get; private set; }
        public string ClientCertPath { get; private set; }
        public string ClientId { get; private set; }

        public MqttConfig(string endpoint, string caCertPath, string clientCertPath, string clientId)
        {
            Endpoint = endpoint;
            CaCertPath = caCertPath;
            ClientCertPath = clientCertPath;
            ClientId = clientId;
        }
    }
}
