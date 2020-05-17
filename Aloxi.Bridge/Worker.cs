using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ZoolWay.Aloxi.Bridge
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> log;
        private readonly ActorSystemProvider actorSystemProvider;

        public Worker(ILogger<Worker> log, ActorSystemProvider actorSystemProvider)
        {
            this.log = log;
            this.actorSystemProvider = actorSystemProvider;
            this.log.LogDebug("test");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            log.LogInformation("Startup");
            this.actorSystemProvider.Init();

            while (!stoppingToken.IsCancellationRequested)
            {
                //log.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }

            log.LogInformation("Shutdown initialted");
            await this.actorSystemProvider.ActorSystem.Terminate();
            log.LogInformation("Actorsystem terminated");
        }
    }
}
