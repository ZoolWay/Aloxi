using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Akka.Actor;
using Akka.Event;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

using ZoolWay.Aloxi.Bridge.Models;

namespace ZoolWay.Aloxi.Bridge.Mediation.Mqtt
{
    public class SubscriptionActor : ReceiveActor
    {
        private readonly ILoggingAdapter log = Context.GetLogger();
        private readonly JsonSerializer jsonSerializer;
        private readonly JsonSerializerSettings jsonSettings;
        private readonly JsonLoadSettings jsonLoadSettings;
        private readonly Encoding jsonEncoding;
        private readonly MqttConfig mqttConfig;
        private readonly string topic;
        private readonly string alexaResponseTopic;
        private readonly IActorRef manager;
        private readonly Dictionary<AloxiMessageOperation, List<IActorRef>> processors;
        private MqttClient client;
        private bool isSubscribed;
        private ICancelable scheduledReconnect;

        public SubscriptionActor(IActorRef manager, MqttConfig mqttConfig, string topic, string alexaResponseTopic)
        {
            log.Info("Creating...");
            this.jsonSettings = new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            this.jsonSerializer = JsonSerializer.CreateDefault(jsonSettings);
            this.jsonEncoding = Encoding.UTF8;
            this.manager = manager;
            this.mqttConfig = mqttConfig;
            this.topic = topic;
            this.alexaResponseTopic = alexaResponseTopic;
            this.processors = new Dictionary<AloxiMessageOperation, List<IActorRef>>();
            this.isSubscribed = false;

            Receive<MediationMessage.Received>(ReceivedReceived);
            Receive<MediationMessage.Publish>(ReceivedPublish);
            Receive<MediationMessage.PublishAlexaResponse>(ReceivedPublishAlexaResponse);
            Receive<MediationMessage.RegisterProcessor>(ReceivedRegisterProcessor);
            Receive<MediationMessage.StateConnectionClosed>(ReceivedStateConnectionClosed);
            Receive<MediationMessage.StateSubscribed>(ReceivedStateSubscribed);
            Receive<MediationMessage.StateUnsubscribed>(ReceivedStateUnsubscribed);
            Receive<MediationMessage.RequestState>(ReceivedRequestState);
            Receive<MediationMessage.RequestConnect>(ReceivedConnect);
        }

        protected override void PreStart()
        {
            log.Debug("Starting, preparing MQTT client");
            IActorRef self = Self;
            this.client = MqttClientProvider.For(this.mqttConfig);
            this.client.MqttMsgSubscribed += (sender, e) => self.Tell(new MediationMessage.StateSubscribed());
            this.client.MqttMsgUnsubscribed += (sender, e) => self.Tell(new MediationMessage.StateUnsubscribed());
            this.client.MqttMsgPublishReceived += (sender, e) => self.Tell(new MediationMessage.Received(e.Message, e.Topic, e.QosLevel, e.DupFlag, e.Retain));
            this.client.ConnectionClosed += (sender, e) => self.Tell(new MediationMessage.StateConnectionClosed());
            Self.Tell(new MediationMessage.RequestConnect());
        }

        protected override void PostStop()
        {
            if (this.client != null)
            {
                if (this.client.IsConnected) this.client.Disconnect();
                this.client = null;
            }
            log.Debug("Stopped");
        }

        protected override void PreRestart(Exception reason, object message)
        {
            log.Error("Restart required, exception occured: {0}", reason.Message);
            base.PreRestart(reason, message);
        }

        protected override void PostRestart(Exception reason)
        {
            log.Warning("Restarted because of: {0}", reason.Message);
            base.PostRestart(reason);
        }

        private void ReceivedConnect(MediationMessage.RequestConnect message)
        {
            byte flag = this.client.Connect(this.mqttConfig.ClientId);
            log.Info("Connected to MQTT bus with flag {0}", flag);
            Context.System.EventStream.Publish(new Bus.MqttConnectivityChangeEvent(true, false, DateTime.Now));

            this.client.Subscribe(new string[] { this.topic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });
        }


        private void ReceivedPublish(MediationMessage.Publish message)
        {
            JObject payload = StringToJObject(message.Payload);
            var aloxiMessage = AloxiMessage.Build(message.Operation, payload, message.ResponseTopic);
            PublishAloxiMessage(aloxiMessage, message.Topic);
        }

        private void ReceivedPublishAlexaResponse(MediationMessage.PublishAlexaResponse message)
        {
            JObject payload = StringToJObject(message.SerializedResponse);
            var aloxiMessage = AloxiMessage.Build(AloxiMessageOperation.PipeAlexaResponse, payload);
            PublishAloxiMessage(aloxiMessage, this.alexaResponseTopic);
        }

        private void ReceivedReceived(MediationMessage.Received message)
        {
            log.Debug($"Received a message on topic '{message.Topic}'");
            // deserialize
            AloxiMessage aloxiMessage;
            try
            {
                string data = this.jsonEncoding.GetString(message.Message.ToArray());
                aloxiMessage = JsonConvert.DeserializeObject<AloxiMessage>(data, this.jsonSettings);
            }
            catch (Exception ex)
            {
                log.Error(ex, "Failed to construct AloxiMessage from received payload");
                return;
            }

            // checks
            if (aloxiMessage.Type != AloxiMessageType.AloxiComm)
            {
                log.Error($"Not supported AloxiMessage of type '{aloxiMessage.Type}'");
                return;
            }
            if (!this.processors.ContainsKey(aloxiMessage.Operation))
            {
                log.Warning($"No processor registered for operation '{aloxiMessage.Operation}'");
                return;
            }

            // distribute to processors
            string payload = JsonConvert.SerializeObject(aloxiMessage.Data, this.jsonSettings);
            foreach (IActorRef processor in this.processors[aloxiMessage.Operation])
            {
                processor.Tell(new MediationMessage.Process(aloxiMessage.Operation, payload, aloxiMessage.ResponseTopic));
            }
        }

        private void ReceivedRegisterProcessor(MediationMessage.RegisterProcessor message)
        {
            if (!this.processors.ContainsKey(message.Operation))
            {
                this.processors[message.Operation] = new List<IActorRef>();
            }
            this.processors[message.Operation].Add(message.Processor);
        }

        private void ReceivedStateSubscribed(MediationMessage.StateSubscribed message)
        {
            this.isSubscribed = true;
            Context.System.EventStream.Publish(new Bus.MqttConnectivityChangeEvent(this.client.IsConnected, this.isSubscribed, DateTime.Now));
            log.Debug("Subscribed");
        }

        private void ReceivedStateUnsubscribed(MediationMessage.StateUnsubscribed message)
        {
            this.isSubscribed = false;
            Context.System.EventStream.Publish(new Bus.MqttConnectivityChangeEvent(this.client.IsConnected, this.isSubscribed, DateTime.Now));
            log.Debug("Unsubscribed");
        }

        private void ReceivedStateConnectionClosed(MediationMessage.StateConnectionClosed message)
        {
            log.Info("Connection closed");
            this.isSubscribed = false;
            Context.System.EventStream.Publish(new Bus.MqttConnectivityChangeEvent(false, this.isSubscribed, DateTime.Now));

            TimeSpan reconnectIn = TimeSpan.FromMinutes(10);
            Context.System.Scheduler.ScheduleTellOnceCancelable(reconnectIn, Self, new MediationMessage.RequestConnect(), Self);
            log.Info("Scheduled re-connect in {0}", reconnectIn);
        }

        private void ReceivedRequestState(MediationMessage.RequestState message)
        {
            Sender.Tell(new MediationMessage.CurrentState(this.client.IsConnected, this.isSubscribed, DateTime.Now));
        }

        private void PublishAloxiMessage(AloxiMessage aloxiMessage, string topic)
        {
            byte[] data = this.jsonEncoding.GetBytes(JsonConvert.SerializeObject(aloxiMessage, this.jsonSettings));
            this.client.Publish(topic, data);
            log.Debug("Published {0} bytes to topic '{1}'", data.Length, topic);
        }

        private JObject StringToJObject(string data)
        {
            return JObject.FromObject(JsonConvert.DeserializeObject(data, this.jsonSettings), this.jsonSerializer);
        }
    }
}
