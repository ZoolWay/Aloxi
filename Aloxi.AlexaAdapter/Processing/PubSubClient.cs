using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using ZoolWay.Aloxi.AlexaAdapter.Processing.Meta;

namespace ZoolWay.Aloxi.AlexaAdapter.Processing
{
    internal class PubSubClient
    {
        private const int MAX_RESPONSE_WAIT_MS = 1500;
        private static readonly int BROKER_PORT = 8883;
        private static readonly Encoding ENCODING = Encoding.UTF8;
        private static readonly JsonSerializer jsonSerializer = JsonSerializer.CreateDefault(jsonSettings);
        private static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() };

        private readonly Configuration configuration;

        public PubSubClient(Configuration config)
        {
            this.configuration = config;
        }

        public void Publish(string toTopic, AloxiMessage message)
        {
            var client = CreateClient();
            client.Publish(toTopic, translateToBytes(message), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
        }

        public Task<TResponsePayload> RequestBridge<TResponsePayload>(AloxiMessageOperation operation, object requestPayload) where TResponsePayload : class
        {
            var requestMessage = AloxiMessage.Build(operation, Pack(requestPayload), this.configuration.TopicResponse);
            return PublishAndAwaitResponse(configuration.TopicBridge, requestMessage)
                .ContinueWith<TResponsePayload>((publishTask) => 
                {
                    var responseMessage = publishTask.Result;
                    if (responseMessage == null) return null;
                    return UnpackTo<TResponsePayload>(responseMessage.Data);
                });
        }

        public Task<JObject> RequestBridgePassthrough(AloxiMessageOperation operation, JObject payload)
        {
            var requestMessage = AloxiMessage.Build(operation, payload, this.configuration.TopicResponse);
            return PublishAndAwaitResponse(configuration.TopicBridge, requestMessage)
                .ContinueWith<JObject>((publishTask) =>
                {
                    var responseMessage = publishTask.Result;
                    if (responseMessage == null) return null;
                    return responseMessage.Data;
                });
        }

        public async Task<AloxiMessage> PublishAndAwaitResponse(string toTopic, AloxiMessage message)
        {
            if (String.IsNullOrWhiteSpace(message.ResponseTopic)) throw new Exception("ResponseTopic is required for PublishAndAwaitResponse");

            string responseTopic = message.ResponseTopic;
            CancellationTokenSource cts = new CancellationTokenSource(3000);

            // start "listen" task
            Task<AloxiMessage> listenTask = Task<AloxiMessage>.Run<AloxiMessage>(() =>
            {
                ManualResetEvent manualResetEvent = new ManualResetEvent(false);
                AloxiMessage responseData = null;

                var subClient = CreateClient();
                subClient.MqttMsgSubscribed += (sender, e) =>
                {
                    Log.Debug("subscribed");
                };
                subClient.MqttMsgPublishReceived += (sender, e) =>
                {
                    Log.Debug("received");
                    try
                    {
                        AloxiMessage m = translateFromBytes(e.Message);
                        if (m.Type != AloxiMessageType.AloxiComm) return;
                        responseData = m;
                        manualResetEvent.Set();
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Received message could not be processed: {ex.Message}"); // ignore message
                    }
                };
                subClient.Subscribe(new string[] { responseTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });
                try
                {
                    bool receivedSignal = manualResetEvent.WaitOne(MAX_RESPONSE_WAIT_MS);
                    if (!receivedSignal) Log.Error($"Did not get response within {MAX_RESPONSE_WAIT_MS}ms (expected on topic {responseTopic})");
                }
                catch (Exception ex)
                {
                    throw new Exception("Failure with manual reset event", ex);
                }
                finally
                {
                    subClient.Disconnect();
                }

                return responseData;
            });

            // publish message (using seperate client)
            var pubClient = CreateClient();
            pubClient.Publish(toTopic, translateToBytes(message));

            // wait for listen to complete or timing out
            Task waitingTask = Task.Delay(MAX_RESPONSE_WAIT_MS);
            Task completedTask = await Task.WhenAny(listenTask, waitingTask);
            if (Object.ReferenceEquals(completedTask, listenTask))
            {
                return listenTask.Result;
            }
            return null;
        }

        private MqttClient CreateClient()
        {
            MqttClient client = null;
            try
            {
                if (!String.IsNullOrEmpty(this.configuration.CertPath))
                {
                    client = ConstructClientBasedOnCertificate(this.configuration.Endpoint, this.configuration.CaPath, this.configuration.CertPath);
                }
                else
                {
                    client = ConstructClientDirectlyInAws(this.configuration.Endpoint);
                }
                String clientId = $"{this.configuration.ClientId}_{Guid.NewGuid().ToString()}";
                client.Connect(clientId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to construct and connect MQTT client", ex);
            }
            return client;
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
            Log.Debug($"Creating MQTT client with certificate from {Path.GetDirectoryName(certPath)}");
            X509Certificate caCert = X509Certificate.CreateFromCertFile(caPath);
            X509Certificate2 clientCert = new X509Certificate2(certPath, (String)null, X509KeyStorageFlags.Exportable);
            return new MqttClient(endpoint, BROKER_PORT, true, caCert, clientCert, MqttSslProtocols.TLSv1_2);
        }

        private static MqttClient ConstructClientDirectlyInAws(string endpoint)
        {
            Log.Debug("Creating direct MQTT client");
            return new MqttClient(endpoint);
        }

        protected JObject Pack(object payload)
        {
            return JObject.FromObject(payload, jsonSerializer);
        }

        protected T UnpackTo<T>(JObject payload) where T : class
        {
            return payload?.ToObject<T>(jsonSerializer);
        }

        private static byte[] translateToBytes(AloxiMessage message)
        {
            string serialized = JsonConvert.SerializeObject(message, jsonSettings);
            return ENCODING.GetBytes(serialized);
        }

        private static AloxiMessage translateFromBytes(byte[] data)
        {
            string serialized = ENCODING.GetString(data);
            return JsonConvert.DeserializeObject<AloxiMessage>(serialized, jsonSettings);
        }

    }
}
