using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;

using uPLibrary.Networking.M2Mqtt;

namespace ZoolWay.Aloxi.Bridge.Mediation.Mqtt
{
    internal static class MqttClientProvider
    {
        private static readonly int BROKER_PORT = 8883;

        public static MqttClient For(MqttConfig mqttConfig)
        {
            if (string.IsNullOrWhiteSpace(mqttConfig.CaCertPath)) return ConstructClientDirectlyInAws(mqttConfig.Endpoint);
            return ConstructClientBasedOnCertificate(mqttConfig.Endpoint, mqttConfig.CaCertPath, mqttConfig.ClientCertPath);
        }

        private static MqttClient ConstructClientBasedOnCertificate(string endpoint, string caPath, string certPath)
        {
            if (!Path.IsPathRooted(certPath))
            {
                string[] potentialBasePaths = { AppContext.BaseDirectory, Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) };
                string basePath = null;
                foreach (var potentialBasePath in potentialBasePaths)
                {
                    string checkName = Path.GetFullPath(Path.Join(potentialBasePath, certPath));
                    if (File.Exists(checkName))
                    {
                        basePath = potentialBasePath;
                        break;
                    }
                }
                if (basePath == null) throw new Exception("Configuration certifacte cannot be found!");
                caPath = Path.Join(basePath, caPath);
                certPath = Path.Join(basePath, certPath);
            }
            Trace.WriteLine($"Creating MQTT client with certificate from {Path.GetDirectoryName(certPath)}");
            X509Certificate caCert = X509Certificate.CreateFromCertFile(caPath);
            X509Certificate2 clientCert = new X509Certificate2(certPath, (string)null, X509KeyStorageFlags.Exportable);
            return new MqttClient(endpoint, BROKER_PORT, true, caCert, clientCert, MqttSslProtocols.TLSv1_2);
        }

        private static MqttClient ConstructClientDirectlyInAws(string endpoint)
        {
            Trace.WriteLine("Creating direct MQTT client");
            return new MqttClient(endpoint);
        }
    }
}
