using System;
using System.Collections.Generic;
using Amazon.Lambda.Core;

namespace ZoolWay.AloxiAlexaAdapter
{
    /// <summary>
    /// Capsules all configuration settings for the function which are collected from environment variables and LambdaContext.
    /// </summary>
    internal class Configuration
    {
        private const string ENV_ENDPOINT = "ENDPOINT";
        private const string ENV_CAPATH = "CA_PATH";
        private const string ENV_CERTPATH = "CERT_PATH";
        private const string ENV_CLIENT_ID = "CLIENT_ID";
        private const string ENV_TOPIC_BRIDGE = "TOPIC_BRIDGE";
        private const string ENV_TOPIC_RESPONSE = "TOPIC_RESPONSE";

        public string Endpoint { get; private set; }
        public string CaPath { get; private set; }
        public string CertPath { get; private set; }
        public string ClientId { get; private set; }
        public string TopicBridge { get; private set; }
        public string TopicResponse { get; private set; }

        private Configuration()
        {
        }

        public static Configuration ProvideFor(ILambdaContext context)
        {
            var configuration = new Configuration();

            var env = new Dictionary<string, string>();
            if (context.ClientContext?.Environment != null)
            {
                foreach (var kvp in context.ClientContext.Environment)
                {
                    env[kvp.Key] = kvp.Value;
                }
            }
            var systemEnv = Environment.GetEnvironmentVariables();
            foreach (var keyObject in systemEnv.Keys)
            {
                string key = keyObject.ToString();
                env[key] = Environment.GetEnvironmentVariable(key);
            }

            configuration.Endpoint = env.GetValueOrDefault(ENV_ENDPOINT, String.Empty);
            configuration.CaPath = env.GetValueOrDefault(ENV_CAPATH, String.Empty);
            configuration.CertPath = env.GetValueOrDefault(ENV_CERTPATH, String.Empty);
            configuration.ClientId = env.GetValueOrDefault(ENV_CLIENT_ID) ?? Guid.NewGuid().ToString();
            configuration.TopicBridge = env.GetValueOrDefault(ENV_TOPIC_BRIDGE, "aloxi:to-bridge");
            configuration.TopicResponse = env.GetValueOrDefault(ENV_TOPIC_RESPONSE, "aloxi:alexa-response");
            return configuration;
        }
    }
}
