using System;
using Akka.Actor;
using Akka.Event;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ZoolWay.Aloxi.Bridge.Alexa.Models;
using ZoolWay.Aloxi.Bridge.Models;
using ZoolWay.Aloxi.Bridge.Mqtt;

namespace ZoolWay.Aloxi.Bridge.Alexa
{
    public class AdapterActor : ReceiveActor
    {
        private const string NS_DISCOVERY = "Alexa.ConnectedHome.Discovery";
        private readonly ILoggingAdapter log = Logging.GetLogger(Context);
        private readonly JsonSerializerSettings jsonSettings;
        private readonly IActorRef mqttDispatcher;
        private IActorRef discoveryResponseHandler;

        public AdapterActor(IActorRef mqttDispatcher)
        {
            this.mqttDispatcher = mqttDispatcher;
            this.jsonSettings = new JsonSerializerSettings() { ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver() };
            this.discoveryResponseHandler = ActorRefs.Nobody;
            Receive<MqttMessage.Process>(ReceivedProcess);
        }

        protected override void PreStart()
        {
            this.discoveryResponseHandler = Context.ActorOf(Props.Create(() => new DiscoveryResponseActor(this.mqttDispatcher)), "discovery-response-handler");
        }

        protected override void PostStop()
        {
            this.discoveryResponseHandler = ActorRefs.Nobody;
        }

        private void ReceivedProcess(MqttMessage.Process message)
        {
            if (message.Operation != AloxiMessageOperation.PipeAlexaRequest)
            {
                log.Error("Got non-compatible Aloxi operation: {0}", message.Operation);
                return;
            }
            var request = JsonConvert.DeserializeObject<AlexaSmartHomeRequest>(message.Payload, this.jsonSettings);
            if (request.Header.Namespace == NS_DISCOVERY)
            {
                ProcessDiscovery(request.Header.Name, request.Payload);
            }
        }

        private void ProcessDiscovery(string name, JObject payload)
        {
            if (name != "DiscoverAppliancesRequest")
            {
                string message = $"Unknown discovery request '{name}'";
                log.Error(message);
                return;
            }

            this.discoveryResponseHandler.Tell(new AlexaMessage.PublishDiscoveryResponse());
        }
    }
}
