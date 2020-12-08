using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

using Akka.Actor;
using Akka.Configuration;
using Akka.Logger.Extensions.Logging;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using ZoolWay.Aloxi.Bridge.Loxone;
using ZoolWay.Aloxi.Bridge.Mediation;
using ZoolWay.Aloxi.Bridge.Meta;

namespace ZoolWay.Aloxi.Bridge
{
    public class ActorSystemProvider
    {
        private readonly IConfiguration configuration;
        private readonly ILogger<ActorSystemProvider> log;
        private ActorSystem actorSystem;
        private IActorRef mediator;
        private IActorRef metaProcessor;
        private IActorRef loxoneAdapter;
        private IActorRef alexaAdapter;
        private IActorRef statusConsolidator;

        public ActorSystem ActorSystem 
        { 
            get
            {
                if (this.actorSystem == null) throw new Exception("ActorSystem is not initialized!");
                return this.actorSystem;
            }
        }

        public IActorRef LoxoneAdapter
        {
            get => this.loxoneAdapter;
        }

        public IActorRef StatusConsolidator
        {
            get => this.statusConsolidator;
        }

        public IActorRef Mediator
        {
            get => this.mediator;
        }

        public ActorSystemProvider(IConfiguration configuration, ILogger<ActorSystemProvider> log, ILoggerFactory loggerFactory)
        {
            this.configuration = configuration;
            this.log = log;
            this.actorSystem = null;
            this.metaProcessor = ActorRefs.Nobody;
            this.loxoneAdapter = ActorRefs.Nobody;
            this.alexaAdapter = ActorRefs.Nobody;
            this.statusConsolidator = ActorRefs.Nobody;
            this.mediator = ActorRefs.Nobody;
            LoggingLogger.LoggerFactory = loggerFactory;
        }

        public void Init()
        {
            log.LogInformation("Initializing ActorSystem");
            string configurationFile = Path.Join(AppContext.BaseDirectory, "akka.hocon");
            string configurationData = File.ReadAllText(configurationFile);
            Config akkaConfig = ConfigurationFactory.ParseString(configurationData);
            this.actorSystem = ActorSystem.Create("aloxi-bridge", akkaConfig);

            // build mediator system
            var mediationConfig = configuration.GetSection("Mediation");
            this.mediator = this.actorSystem.ActorOf(Props.Create(() => new Mediation.MediatorActor(mediationConfig)), "mediation");

            // build meta
            this.metaProcessor = this.actorSystem.ActorOf(Props.Create(() => new Meta.MetaProcessorActor(this.mediator)), "meta");

            // build Loxone
            this.loxoneAdapter = ActorRefs.Nobody;
            try
            {
                var c = configuration.GetSection("Loxone");
                ImmutableArray<string> ignoreCats = c.GetSection("IgnoreCategories").GetChildren().ToList().Select(x => x.Value).ToImmutableArray();
                ImmutableArray<string> ignoreControls = c.GetSection("IgnoreControls").GetChildren().ToList().Select(x => x.Value).ToImmutableArray();
                var loxoneConfig = new Loxone.LoxoneConfig(c["Miniserver"], c["Username"], c["Password"], ignoreCats, ignoreControls);
                this.loxoneAdapter = this.actorSystem.ActorOf(Props.Create(() => new Loxone.AdapterActor(loxoneConfig)), "loxone");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Initializing of Loxone node failed");
            }

            // build alexa
            this.alexaAdapter = ActorRefs.Nobody;
            try
            {
                this.alexaAdapter = this.actorSystem.ActorOf(Props.Create(() => new Alexa.AdapterActor(this.mediator, this.loxoneAdapter)),  "alexa");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Initializing of Alexa node failed");
            }

            // init and configuration
            this.mediator.Tell(new MediationMessage.RegisterProcessor(Models.AloxiMessageOperation.Echo, this.metaProcessor));
            this.mediator.Tell(new MediationMessage.RegisterProcessor(Models.AloxiMessageOperation.PipeAlexaRequest, this.alexaAdapter));
            this.loxoneAdapter.Tell(new LoxoneMessage.InitAdapter());

            // status consolidator
            this.statusConsolidator = this.actorSystem.ActorOf(Props.Create(() => new StatusConsolidatorActor(this.loxoneAdapter, this.mediator)));
        }
    }
}
