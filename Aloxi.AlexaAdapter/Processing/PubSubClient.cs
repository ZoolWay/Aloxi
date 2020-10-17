using System;
using System.IO;
using System.Net.Security;
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
        private const int MAX_WAIT_FOR_PUBLISH = 2000;
        private const int MAX_WAIT_FOR_RESPONSE = 2000;
        private const int MAX_WAIT_FOR_RESPONSE_TIMEOUT = 2500;
        private static readonly int BROKER_PORT = 8883;
        private static readonly Encoding ENCODING = Encoding.UTF8;
        private static readonly JsonSerializer jsonSerializer = JsonSerializer.CreateDefault(jsonSettings);
        private static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() };

        private readonly Configuration configuration;
        private readonly ILambdaContext lambdaContext;

        public PubSubClient(Configuration config, ILambdaContext lambdaContext)
        {
            this.configuration = config;
            this.lambdaContext = lambdaContext;
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
            Log.Debug(this.lambdaContext, $"PSC/RBP: Publishing AloxiMessage '{operation}' and waiting for response");
            return PublishAndAwaitResponse(configuration.TopicBridge, requestMessage)
                .ContinueWith<JObject>((publishTask) =>
                {
                    if (publishTask.Result == null)
                    {
                        Log.Warn(this.lambdaContext, $"PSC/RBP: Passing NULL through (we did not get a response?)");
                        return null;
                    }
                    Log.Debug(this.lambdaContext, $"PSC/RBP: Continueing with {publishTask?.Result?.Data?.Count} datanodes");
                    return publishTask.Result.Data;
                });
        }

        public async Task<AloxiMessage> PublishAndAwaitResponse(string toTopic, AloxiMessage message)
        {
            if (String.IsNullOrWhiteSpace(message.ResponseTopic)) throw new Exception("ResponseTopic is required for PublishAndAwaitResponse");

            ManualResetEvent msePublished = new ManualResetEvent(false);
            string responseTopic = message.ResponseTopic;

            // start "listen" task
            Log.Debug(this.lambdaContext, "PSC/PAAR: Starting request-response task");
            Task<AloxiMessage> requestReponseTask = Task<AloxiMessage>.Run<AloxiMessage>(() =>
            {
                ManualResetEvent mseReceived = new ManualResetEvent(false);
                AloxiMessage responseData = null;

                var subClient = CreateClient();
                Log.Debug(this.lambdaContext, "PSC/PAAR/RRT: RequestResponseTask got client and connected");
                subClient.MqttMsgSubscribed += (sender, e) =>
                {
                    Log.Debug(this.lambdaContext, "PSC/PAAR/RRT: subscribed, sending request");
                    var publishFlag = subClient.Publish(toTopic, translateToBytes(message));
                    msePublished.Set();
                    Log.Debug(this.lambdaContext, $"PSC/PAAR/RRT: published to topic '{toTopic}', return flag = {publishFlag}");
                };
                subClient.MqttMsgPublishReceived += (sender, e) =>
                {
                    Log.Debug(this.lambdaContext, "PSC/PAAR/RRT: received message");
                    try
                    {
                        AloxiMessage m = translateFromBytes(e.Message);
                        if (m.Type != AloxiMessageType.AloxiComm) return;
                        responseData = m;
                        mseReceived.Set();
                        Log.Debug(this.lambdaContext, "PSC/PAAR/RRT: manual reset event completed");
                    }
                    catch (Exception ex)
                    {
                        Log.Error(this.lambdaContext, $"PSC/PAAR/RRT: received message could not be processed: {ex.Message}"); // ignore message
                    }
                };
                var subscribeFlag = subClient.Subscribe(new string[] { responseTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });
                Log.Debug(this.lambdaContext, $"PSC/PAAR/RRT: subscribed response topic '{responseTopic}', return flag = {subscribeFlag}");
                bool? receivedSignal = null;
                try
                {
                    // TODO wait for subscription confirmation
                    receivedSignal = mseReceived.WaitOne(MAX_WAIT_FOR_RESPONSE);
                    if (!receivedSignal.Value) Log.Error(this.lambdaContext, $"PSC/PAAR/RRT: did not get response within {MAX_WAIT_FOR_RESPONSE}ms (expected on topic {responseTopic})");
                }
                catch (Exception ex)
                {
                    throw new Exception("Failure with manual reset event", ex);
                }
                finally
                {
                    subClient.Disconnect();
                }

                Log.Debug(this.lambdaContext, $"PSC/PAAR/RRT: Returning from RequestSponseTask, receivedSignal={receivedSignal}, data type={responseData?.GetType().FullName}.");
                return responseData;
            });

            // wait for listen to complete or timing out
            Task waitingTask = Task.Delay(MAX_WAIT_FOR_RESPONSE + MAX_WAIT_FOR_PUBLISH);
            Log.Debug(this.lambdaContext, $"PSC/PAAR: Task id #{waitingTask.Id} is waiting task, task id #{requestReponseTask.Id} is request-response task");
            Task completedTask = await Task.WhenAny(requestReponseTask, waitingTask);
            string taskname = "unknown";
            if (Object.ReferenceEquals(completedTask, requestReponseTask)) taskname = "REQ-RESP";
            if (Object.ReferenceEquals(completedTask, waitingTask)) taskname = "WAITING";
            Log.Debug(this.lambdaContext, $"PSC/PAAR: A task completed, id #{completedTask.Id}, is {taskname}");
            if (Object.ReferenceEquals(completedTask, requestReponseTask))
            {
                if (requestReponseTask.Result == null)
                {
                    Log.Warn(this.lambdaContext, $"PSC/PAAR: Returning NULL as the listenTask returned it!");
                    return null;
                }
                else
                {
                    Log.Debug(this.lambdaContext, $"PSC/PAAR: Returning data from listenTask is of type {requestReponseTask.Result?.GetType().FullName}");
                    return requestReponseTask.Result;
                }
            }
            Log.Warn(this.lambdaContext, $"PSC/PAAR: Returning NULL as we got not answer in time!");
            return null;
        }

        private MqttClient CreateClient()
        {
            MqttClient client = null;
            try
            {
                if (!String.IsNullOrEmpty(this.configuration.CertPath))
                {
                    client = ConstructClientBasedOnCertificate(this.lambdaContext, this.configuration.Endpoint, this.configuration.CaPath, this.configuration.CertPath);
                }
                else
                {
                    client = ConstructClientDirectlyInAws(this.lambdaContext, this.configuration.Endpoint);
                }
                string clientId = $"{this.configuration.ClientId}_{Guid.NewGuid().ToString()}";
                Log.Debug(this.lambdaContext, $"PSC/CC: Client constructed, now connectting with id '{clientId}'");
                byte flag = client.Connect(clientId);
                Log.Debug(this.lambdaContext, $"PSC/CC: Client connected with flag = {flag}");
            }
            catch (Exception ex)
            {
                Log.Error(this.lambdaContext, $"PSC/CC: Failure during construction and connection: {ex.Message}");
                throw new Exception($"Failed to construct and connect MQTT client", ex);
            }
            Log.Debug(this.lambdaContext, "PSC/CC: Returning client");
            return client;
        }

        private static MqttClient ConstructClientBasedOnCertificate(ILambdaContext context, string endpoint, string caPath, string certPath)
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
            Log.Debug(context, $"PSC/CCBOC: Creating MQTT client with certificate {Path.GetFileName(certPath)} from {Path.GetDirectoryName(certPath)}");
            X509Certificate caCert = X509Certificate.CreateFromCertFile(caPath);
            X509Certificate2 clientCert = new X509Certificate2(certPath, (String)null, X509KeyStorageFlags.Exportable);
            return new MqttClient(endpoint, BROKER_PORT, true, caCert, clientCert, MqttSslProtocols.TLSv1_2);
        }

        private static MqttClient ConstructClientDirectlyInAws(ILambdaContext  context, string endpoint)
        {
            Log.Debug(context, "PSC/CCDIA: Creating direct MQTT client");
            return new MqttClient(endpoint, BROKER_PORT, true, MqttSslProtocols.TLSv1_2, new RemoteCertificateValidationCallback(Rcvc), new LocalCertificateSelectionCallback(Lcsc));
        }

        private static X509Certificate Lcsc(object sender, string targetHost, X509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[] acceptableIssuers)
        {
            return null;
        }

        private static bool Rcvc(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
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
