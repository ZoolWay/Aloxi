using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using Newtonsoft.Json;
using ZoolWay.Aloxi.Bridge.Alexa.Models;
using ZoolWay.Aloxi.Bridge.Alexa.Models.Impl;
using ZoolWay.Aloxi.Bridge.Loxone;
using ZoolWay.Aloxi.Bridge.Models;

namespace ZoolWay.Aloxi.Bridge.Alexa
{
    public class DiscoveryResponseActor : ReceiveActor
    {
        private readonly ILoggingAdapter log = Logging.GetLogger(Context);
        private readonly IActorRef mqttDispatcher;
        private readonly JsonSerializerSettings jsonSettings;
        private Home homeModel;
        private AlexaDiscoverResponsePayload cache;

        public DiscoveryResponseActor(IActorRef mqttDispatcher)
        {
            this.jsonSettings = new JsonSerializerSettings() { ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver() };
            this.mqttDispatcher = mqttDispatcher;
            Receive<AlexaMessage.PublishDiscoveryResponse>(ReceivedPublishDiscoveryResponse);
            Receive<Bus.HomeModelUpdatedEvent>(ReceivedHomeModelUpdated);
        }

        protected override void PreStart()
        {
            Context.System.EventStream.Subscribe<Bus.HomeModelUpdatedEvent>(Self);
        }

        protected override void PostStop()
        {
            Context.System.EventStream.Unsubscribe<Bus.HomeModelUpdatedEvent>(Self);
        }

        private void ReceivedPublishDiscoveryResponse(AlexaMessage.PublishDiscoveryResponse message)
        {
            if (this.cache == null)
            {
                string errMsg = "Cannot provide DiscoveryResponse, no data present!";
                log.Error(errMsg);
                return;
            }
            log.Debug("Dispatching cached AlexaDiscoveryReponse");

            var responseEvent = new AlexaDiscoverResponseEvent()
            {
                Header = new AlexaEventHeader()
                {
                    Namespace = "Alexa.Discovery",
                    Name = "Discover.Response",
                    PayloadVersion = "3",
                    MessageId = Guid.NewGuid().ToString()
                },
                Payload = this.cache,
            };
            var response = new AlexaDiscoverResponse()
            {
                Event = responseEvent,
            };
            this.mqttDispatcher.Tell(new Mqtt.MqttMessage.PublishAlexaResponse(JsonConvert.SerializeObject(response, this.jsonSettings)));
        }

        private void ReceivedHomeModelUpdated(Bus.HomeModelUpdatedEvent message)
        {
            this.homeModel = message.HomeModel;
            BuildResponseAndCache();
        }

        private void BuildResponseAndCache()
        {
            this.cache = null;
            if (this.homeModel == null) return;
            try
            {
                List<AlexaEndpoint> endpoints = new List<AlexaEndpoint>();
                foreach (var c in this.homeModel.Controls)
                {
                    AlexaEndpoint ep = ParseControlToEndpoint(c);
                    if (ep != null)
                    {
                        endpoints.Add(ep);
                    }
                }
                this.cache = new AlexaDiscoverResponsePayload() { Endpoints = endpoints.ToArray() };
                log.Info("Caching Alexa Discovery Response with {0} endpoints", this.cache.Endpoints.Length);
            }
            catch (Exception ex)
            {
                string errMsg = $"Failed to build discovery response: {ex.Message}";
                log.Error(ex, errMsg);
            }
        }

        private AlexaEndpoint ParseControlToEndpoint(Control c)
        {
            string uuid = c.LoxoneUuid.ToString();
            if (c.Type == ControlType.LightControl)
            {
                AlexaEndpoint ep = new AlexaEndpoint()
                {
                    EndpointId = uuid,
                    ManufacturerName = "Loxone / Aloxi by ZoolWay",
                    Description = $"Lichtschalter via Loxone in {c.RoomName}",
                    FriendlyName = c.FriendlyName,
                    AdditionalAttributes = GenerateBasicAdditionalAttributes(c),
                    DisplayCategories = new[] { "LIGHT" },
                    Capabilities = new AlexaEndpointCapability[] { new PowerControllerCapability() },
                };
                AddOperationsToAdditionalAttributes(c.Operations, ep.AdditionalAttributes);
                return ep;
            }
            else if (c.Type == ControlType.LightDimmableControl)
            {
                AlexaEndpoint ep = new AlexaEndpoint()
                {
                    EndpointId = uuid,
                    ManufacturerName = "Loxone / Aloxi by ZoolWay",
                    Description = $"Dimmer via Loxone in {c.RoomName}",
                    FriendlyName = c.FriendlyName,
                    AdditionalAttributes = GenerateBasicAdditionalAttributes(c),
                    DisplayCategories = new[] { "LIGHT" },
                    Capabilities = new AlexaEndpointCapability[] { new PowerLevelControllerCapability() },
                };
                AddOperationsToAdditionalAttributes(c.Operations, ep.AdditionalAttributes);
                return ep;
            }
            else if (c.Type == ControlType.BlindControl)
            {
                AlexaEndpoint ep = new AlexaEndpoint()
                {
                    EndpointId = uuid,
                    ManufacturerName = "Loxone / Aloxi by ZoolWay",
                    Description = $"Jalousie via Loxone in {c.RoomName}",
                    FriendlyName = c.FriendlyName,
                    AdditionalAttributes = GenerateBasicAdditionalAttributes(c),
                    DisplayCategories = new[] { "INTERIOR_BLIND" },
                    Capabilities = new AlexaEndpointCapability[] { new ModeControllerCapabilityForBlinds(), new AlexaCapability() },
                };
                AddOperationsToAdditionalAttributes(c.Operations, ep.AdditionalAttributes);
                return ep;
            }
            return null;
        }

        private Dictionary<string, string> GenerateBasicAdditionalAttributes(Control c)
        {
            var attr = new Dictionary<string, string>();
            attr["LoxoneName"] = c.LoxoneName;
            attr["LoxoneUuid"] = c.LoxoneUuid.ToString();
            return attr;
        }

        private void AddOperationsToAdditionalAttributes(ImmutableDictionary<string, LoxoneUuid> operations, Dictionary<string, string> targetAttributes)
        {
            foreach (var kvp in operations)
            {
                targetAttributes.Add($"LoxoneOperation:{kvp.Key}", kvp.Value.ToString());
            }
        }
    }
}
