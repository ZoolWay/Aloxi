using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using M2Mqtt;

namespace iotdotnetcorepublisher
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            string iotEndpoint = "a1fk2tqo498isz-ats.iot.eu-west-1.amazonaws.com";
            Console.WriteLine("AWS IoT Dotnet core message publisher starting");
            int brokerPort = 8883;

            string message = "Test message";
            string topic = "Hello/World";

            string caCertFile = Path.Join(AppContext.BaseDirectory, "..\\..\\..\\..\\bridge\\config\\AmazonRootCA1.pem");
            Console.WriteLine($"CA cert file: {caCertFile}");
            var caCert = X509Certificate.CreateFromCertFile(caCertFile); //crt format?
            var clientCert = new X509Certificate2(Path.Join(AppContext.BaseDirectory, "..\\..\\..\\..\\bridge\\config\\badb237f72-certificate.pfx"));

            var client = new MqttClient(iotEndpoint, brokerPort, true, caCert, clientCert, MqttSslProtocols.TLSv1_2);

            string clientId = Guid.NewGuid().ToString();
            client.Connect(clientId);
            Console.WriteLine($"Connected to AWS IoT with client id: {clientId}.");

            int i = 0;
            while (true)
            {
                client.Publish(topic, Encoding.UTF8.GetBytes($"{message} {i}"));
                Console.WriteLine($"Published: {message} {i}");
                i++;
                Thread.Sleep(5000);
            }
        }
    }
}
