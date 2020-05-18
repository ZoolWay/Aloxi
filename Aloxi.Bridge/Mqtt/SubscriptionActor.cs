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

namespace ZoolWay.Aloxi.Bridge.Mqtt
{
    public class SubscriptionActor : ReceiveActor
    {
        private readonly ILoggingAdapter log = Logging.GetLogger(Context);
        private readonly JsonSerializer jsonSerializer;
        private readonly JsonSerializerSettings jsonSettings;
        private readonly Encoding jsonEncoding;
        private readonly MqttConfig mqttConfig;
        private readonly string topic;
        private readonly IActorRef manager;
        private readonly Dictionary<AloxiMessageOperation, List<IActorRef>> processors;
        private MqttClient client;

        public SubscriptionActor(IActorRef manager, MqttConfig mqttConfig, string topic)
        {
            this.jsonSettings = new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            this.jsonSerializer = JsonSerializer.CreateDefault(jsonSettings);
            this.jsonEncoding = Encoding.UTF8;
            this.manager = manager;
            this.mqttConfig = mqttConfig;
            this.topic = topic;
            this.processors = new Dictionary<AloxiMessageOperation, List<IActorRef>>();

            Receive<MqttMessage.Received>(ReceivedReceived);
            Receive<MqttMessage.Publish>(ReceivedPublish);
            Receive<MqttMessage.RegisterProcessor>(ReceivedRegisterProcessor);
        }

        protected override void PreStart()
        {
            log.Debug("Starting");
            IActorRef self = Self;
            this.client = MqttClientProvider.For(this.mqttConfig);
            byte flag = this.client.Connect(this.mqttConfig.ClientId);
            log.Info("Connected to MQTT bus with flag {0}", flag);
            this.client.ConnectionClosed += (sender, e) => this.manager.Tell(new MqttMessage.Log(LogLevel.InfoLevel, "Connection closed"));
            this.client.MqttMsgSubscribed += (sender, e) => this.manager.Tell(new MqttMessage.Log(LogLevel.DebugLevel, "Subscribed"));
            this.client.MqttMsgUnsubscribed += (sender, e) => this.manager.Tell(new MqttMessage.Log(LogLevel.DebugLevel, "Unsubscribe"));
            this.client.MqttMsgPublishReceived += (sender, e) => self.Tell(new MqttMessage.Received(e.Message, e.Topic, e.QosLevel, e.DupFlag, e.Retain));
            this.client.Subscribe(new string[] { this.topic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });
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

        protected override void PostRestart(Exception reason)
        {
            log.Warning("Restarted because of: {0}", reason.Message);
            base.PostRestart(reason);
        }

        private void ReceivedPublish(MqttMessage.Publish message)
        {
            JObject payload = JObject.FromObject(JsonConvert.DeserializeObject(message.Payload, this.jsonSettings), this.jsonSerializer);
            var aloxiMessage = AloxiMessage.Build(message.Operation, payload, message.ResponseTopic);
            byte[] data = this.jsonEncoding.GetBytes(JsonConvert.SerializeObject(aloxiMessage, this.jsonSettings));
            this.client.Publish(message.Topic, data);
            log.Debug("Published {0} bytes to topic '{1}'", data.Length, message.Topic);
        }


        private void ReceivedReceived(MqttMessage.Received message)
        {
            // deserialize
            AloxiMessage aloxiMessage;
            try
            {
                string data = this.jsonEncoding.GetString(message.Message.ToArray<byte>());
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
                processor.Tell(new MqttMessage.Process(aloxiMessage.Operation, payload, aloxiMessage.ResponseTopic));
            }
        }

        private void ReceivedRegisterProcessor(MqttMessage.RegisterProcessor message)
        {
            if (!this.processors.ContainsKey(message.Operation))
            {
                this.processors[message.Operation] = new List<IActorRef>();
            }
            this.processors[message.Operation].Add(message.Processor);
        }


    }
}
