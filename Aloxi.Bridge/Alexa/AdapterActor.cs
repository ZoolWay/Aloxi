using System;
using Akka.Actor;
using Akka.Event;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ZoolWay.Aloxi.Bridge.Alexa.Models;
using ZoolWay.Aloxi.Bridge.Loxone;
using ZoolWay.Aloxi.Bridge.Models;
using ZoolWay.Aloxi.Bridge.Mqtt;

namespace ZoolWay.Aloxi.Bridge.Alexa
{
    public class AdapterActor : ReceiveActor
    {
        private const string NS_DISCOVERY = "Alexa.Discovery";
        private const string NS_POWERCONTROL = "Alexa.PowerController";
        private readonly ILoggingAdapter log = Logging.GetLogger(Context);
        private readonly JsonSerializerSettings jsonSettings;
        private readonly IActorRef mqttDispatcher;
        private readonly IActorRef loxoneDispatcher;
        private IActorRef discoveryResponseHandler;

        public AdapterActor(IActorRef mqttDispatcher, IActorRef loxoneDispatcher)
        {
            this.mqttDispatcher = mqttDispatcher;
            this.loxoneDispatcher = loxoneDispatcher;
            this.jsonSettings = new JsonSerializerSettings()
            {
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
            };
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
            if (request.Directive.Header.Namespace == NS_DISCOVERY)
            {
                ProcessDiscovery(request.Directive.Header.Name, request.Directive);
            }
            if (request.Directive.Header.Namespace == NS_POWERCONTROL)
            {
                ProcessPowerController(request.Directive.Header.Name, request.Directive);
            }
        }

        private void ProcessPowerController(string name, AlexaDirective directive)
        {
            string targetValue = "";
            if (name == "TurnOn")
            {
                this.loxoneDispatcher.Tell(new LoxoneMessage.ControlSwitch(directive.Endpoint.EndpointId, LoxoneMessage.ControlSwitch.DesiredStateType.On));
                targetValue = "ON";
            }
            else if (name == "TurnOff")
            {
                this.loxoneDispatcher.Tell(new LoxoneMessage.ControlSwitch(directive.Endpoint.EndpointId, LoxoneMessage.ControlSwitch.DesiredStateType.Off));
                targetValue = "OFF";
            }
            else
            {
                string message = $"Unknown control request '{name}'";
                log.Error(message);
            }
            var response = new AlexaResponse()
            {
                Event = new AlexaResponseEvent()
                {
                    Header = new AlexaEventHeader()
                    {
                        Namespace = "Alexa",
                        Name = "Response",
                        MessageId = Guid.NewGuid().ToString(),
                        CorrelationId = directive.Header.CorrelationId,
                        PayloadVersion = "3",
                    },
                    Endpoint = new AlexaEventEndpoint()
                    {
                        EndpointId = directive.Endpoint.EndpointId,
                    },
                },
                Context = new AlexaContext()
                {
                    Properties = new System.Collections.Generic.List<AlexaProperty>()
                    {
                        new AlexaProperty()
                        {
                            Namespace = "Alexa.PowerController",
                            Name = "powerState",
                            Value = targetValue,
                            TimeOfSample = DateTime.Now,
                        }
                    }
                },
            };
            this.mqttDispatcher.Tell(new Mqtt.MqttMessage.PublishAlexaResponse(JsonConvert.SerializeObject(response, this.jsonSettings)));
        }

        private void ProcessDiscovery(string name, AlexaDirective directive)
        {
            if (name != "Discover")
            {
                string message = $"Unknown discovery request '{name}'";
                log.Error(message);
                return;
            }

            this.discoveryResponseHandler.Tell(new AlexaMessage.PublishDiscoveryResponse());
        }
    }
}
