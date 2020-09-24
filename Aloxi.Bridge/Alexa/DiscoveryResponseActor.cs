using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using Newtonsoft.Json;
using ZoolWay.Aloxi.Bridge.Alexa.Models;
using ZoolWay.Aloxi.Bridge.Alexa.Models.Impl;
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
                    if (c.Type == ControlType.LightControl)
                    {
                        string uuid = c.LoxoneUuid.ToString();
                        AlexaEndpoint ep = new AlexaEndpoint()
                        {
                            EndpointId = uuid,
                            ManufacturerName = "Loxone",
                            Description = "Lichtcontrol von Loxone",
                            FriendlyName = c.FriendlyName,
                            AdditionalAttributes = new Dictionary<string, string>()
                            {
                                { "LoxoneName", c.LoxoneName },
                                { "LoxoneUuid", uuid },
                            },
                            DisplayCategories = new[] { "LIGHT" },
                            Capabilities = new AlexaEndpointCapability[] { new PowerControllerCapability() },
                        };
                        foreach (var kvp in c.Operations)
                        {
                            ep.AdditionalAttributes.Add($"LoxoneOperation:{kvp.Key}", kvp.Value.ToString());
                        }
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
    }
}
