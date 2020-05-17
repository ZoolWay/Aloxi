using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using Akka.Logger.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ZoolWay.Aloxi.Bridge.Mqtt;

namespace ZoolWay.Aloxi.Bridge
{
    public class ActorSystemProvider
    {
        private readonly IConfiguration configuration;
        private readonly ILogger<ActorSystemProvider> log;
        private ActorSystem actorSystem;
        private IActorRef mqttManager;
        private IActorRef metaProcessor;

        public ActorSystem ActorSystem 
        { 
            get
            {
                if (this.actorSystem == null) throw new Exception("ActorSystem is not initialized!");
                return this.actorSystem;
            }
        }

        public IActorRef MqttManager
        {
            get => this.mqttManager;
        }

        public ActorSystemProvider(IConfiguration configuration, ILogger<ActorSystemProvider> log, ILoggerFactory loggerFactory)
        {
            this.configuration = configuration;
            this.log = log;
            this.actorSystem = null;
            this.mqttManager = ActorRefs.Nobody;
            this.metaProcessor = ActorRefs.Nobody;

            LoggingLogger.LoggerFactory = loggerFactory;
        }

        public void Init()
        {
            log.LogInformation("Initializing ActorSystem");
            string configurationFile = Path.Join(AppContext.BaseDirectory, "akka.hocon");
            string configurationData = File.ReadAllText(configurationFile);
            Config akkaConfig = ConfigurationFactory.ParseString(configurationData);
            this.actorSystem = ActorSystem.Create("aloxi-bridge", akkaConfig);
            
            this.mqttManager = ActorRefs.Nobody;
            try
            {
                var c = configuration.GetSection("Mqtt");
                var mqttConfig = new Mqtt.MqttConfig(c["Endpoint"], c["CaPath"], c["CertPath"], c["ClientId"]);
                string subscriptionTopic = c["SubscriptionTopic"];
                this.mqttManager = this.actorSystem.ActorOf(Props.Create(() => new Mqtt.ManagerActor(mqttConfig, subscriptionTopic)), "mqtt");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Initializing of MQTT node failed");
            }

            this.metaProcessor = this.actorSystem.ActorOf(Props.Create(() => new Meta.MetaProcessorActor(this.mqttManager)), "meta");

            this.mqttManager.Tell(new MqttMessage.RegisterProcessor(Models.AloxiMessageOperation.Echo, this.metaProcessor));
        }
    }
}
