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
        private const string NS_POWERLEVELCONTROL = "Alexa.PowerLevelController";
        private const string NS_MODECONTROL = "Alexa.ModeController";
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
            else if (request.Directive.Header.Namespace == NS_POWERCONTROL)
            {
                ProcessPowerController(request.Directive.Header.Name, request.Directive);
            }
            else if (request.Directive.Header.Namespace == NS_POWERLEVELCONTROL)
            {
                ProcessPowerlevelController(request.Directive.Header.Name, request.Directive);
            }
            else if (request.Directive.Header.Namespace == NS_MODECONTROL)
            {
                ProcessModeController(request.Directive);
            }
            else
            {
                string msg = $"Adapter does not support directive namespace {request.Directive.Header.Namespace}";
                log.Warning(msg);
                SendResponseToAlexa(CreateError(request.Directive.Endpoint.EndpointId, AlexaErrorType.INVALID_DIRECTIVE, msg));
            }
        }

        private void ProcessModeController(AlexaDirective directive)
        {
            string name = directive.Header.Name;
            string instance = directive.Header.Instance;
            string targetMode = directive.Payload["mode"]?.ToString();
            log.Info("Processing MopdeController request with name '{0}' for instance '{1}' targetting '{2}'", name, instance, targetMode);

            if (instance == "Blinds.BlindTargetState")
            {
                if (name == "SetMode")
                {
                    LoxoneMessage.ControlBlinds.BlindCmd command = LoxoneMessage.ControlBlinds.BlindCmd.FullUp;
                    switch (targetMode)
                    {
                        case "BlindTargetState.FullUp":
                            command = LoxoneMessage.ControlBlinds.BlindCmd.FullUp;
                            break;
                        case "BlindTargetState.FullDown":
                            command = LoxoneMessage.ControlBlinds.BlindCmd.FullDown;
                            break;
                        case "BlindTargetState.Stop":
                            command = LoxoneMessage.ControlBlinds.BlindCmd.Stop;
                            break;
                    }
                    this.loxoneDispatcher.Tell(new LoxoneMessage.ControlBlinds(AlexaUuidTranslator.ToLoxoneUuid(directive.Endpoint.EndpointId), command));
                    var response = CreateResponse(directive.Header.CorrelationId, directive.Endpoint.EndpointId);
                    response.Context.Properties.Add(new AlexaProperty("Alexa.ModeController", "mode", targetMode, DateTime.Now));
                    SendResponseToAlexa(response);
                    return;
                }
            }

            SendResponseToAlexa(CreateError(directive.Endpoint.EndpointId, AlexaErrorType.INVALID_DIRECTIVE, $"Directive {name} for instance {instance} is not implemented!"));
        }

        private void ProcessPowerlevelController(string name, AlexaDirective directive)
        {
            log.Info("Processing PowerlevelController request with name '{0}'", name);
            if (name == "SetPowerLevel")
            {
                string targetPowerLevelProp = directive.Payload["powerLevel"].ToString();
                bool valueParseable = Int32.TryParse(targetPowerLevelProp, out int targetPowerLevel);


                this.loxoneDispatcher.Tell(new LoxoneMessage.ControlDimmer(AlexaUuidTranslator.ToLoxoneUuid(directive.Endpoint.EndpointId), LoxoneMessage.ControlDimmer.DimType.Set, targetPowerLevel));

                var response = CreateResponse(directive.Header.CorrelationId, directive.Endpoint.EndpointId);
                response.Context.Properties.Add(new AlexaProperty("Alexa.PowerLevelController", "powerLevel", targetPowerLevelProp, DateTime.Now));
                SendResponseToAlexa(response);
            }
            else if (name == "AdjustPowerLevel")
            {
                // TODO
                SendResponseToAlexa(CreateError(directive.Endpoint.EndpointId, AlexaErrorType.INVALID_DIRECTIVE, "Adapter does not support adjusting power level"));
            }
            else
            {
                SendResponseToAlexa(CreateError(directive.Endpoint.EndpointId, AlexaErrorType.INVALID_DIRECTIVE, $"PowerLevelAdapter does not support '{name}'"));
            }
        }

        private void ProcessPowerController(string name, AlexaDirective directive)
        {
            log.Info("Processing PowerController request with name '{0}'", name);
            string targetValue = "";
            if (name == "TurnOn")
            {
                this.loxoneDispatcher.Tell(new LoxoneMessage.ControlSwitch(AlexaUuidTranslator.ToLoxoneUuid(directive.Endpoint.EndpointId), LoxoneMessage.ControlSwitch.DesiredStateType.On));
                targetValue = "ON";
            }
            else if (name == "TurnOff")
            {
                this.loxoneDispatcher.Tell(new LoxoneMessage.ControlSwitch(AlexaUuidTranslator.ToLoxoneUuid(directive.Endpoint.EndpointId), LoxoneMessage.ControlSwitch.DesiredStateType.Off));
                targetValue = "OFF";
            }
            else
            {
                string message = $"Unknown control request '{name}'";
                log.Error(message);
            }
            var response = CreateResponse(directive.Header.CorrelationId, directive.Endpoint.EndpointId);
            response.Context.Properties.Add(new AlexaProperty("Alexa.PowerController", "powerState", targetValue, DateTime.Now));
            SendResponseToAlexa(response);
        }

        private AlexaError CreateError(string endpointId, AlexaErrorType errorType, string internalMessage)
        {
            return new AlexaError()
            {
                Event = new AlexaErrorEvent()
                {
                    Header = new AlexaEventHeader()
                    {
                        Namespace = "Alexa",
                        Name = "ErrorResponse",
                        MessageId = Guid.NewGuid().ToString(),
                        PayloadVersion = "3",
                    },
                    Endpoint = new AlexaEventEndpoint()
                    {
                        EndpointId = endpointId
                    },
                    Payload = new AlexaErrorPayload()
                    {
                        Type = errorType,
                        Message = internalMessage,
                    },
                }
            };
        }

        private AlexaResponse CreateResponse(string correlationId, string endpointId)
        {
            return new AlexaResponse()
            {
                Event = new AlexaResponseEvent()
                {
                    Header = new AlexaEventHeader()
                    {
                        Namespace = "Alexa",
                        Name = "Response",
                        MessageId = Guid.NewGuid().ToString(),
                        CorrelationId = correlationId,
                        PayloadVersion = "3",
                    },
                    Endpoint = new AlexaEventEndpoint()
                    {
                        EndpointId = endpointId,
                    },
                },
                Context = new AlexaContext()
                {
                    Properties = new System.Collections.Generic.List<AlexaProperty>(),
                },
            };
        }

        private void SendResponseToAlexa(object response)
        {
            this.mqttDispatcher.Tell(new Mqtt.MqttMessage.PublishAlexaResponse(JsonConvert.SerializeObject(response, this.jsonSettings)));
        }

        private void ProcessDiscovery(string name, AlexaDirective directive)
        {
            log.Info("Processing Discovery request with name '{0}'", name);
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
