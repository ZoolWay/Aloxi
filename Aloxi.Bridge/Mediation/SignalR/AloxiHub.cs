using System;
using System.Threading.Tasks;

using Akka.Actor;

using Microsoft.Extensions.Logging;

namespace ZoolWay.Aloxi.Bridge.Mediation.SignalR
{
    public class AloxiHub : Microsoft.AspNetCore.SignalR.Hub
    {
        private readonly ILogger<AloxiHub> log;
        private readonly IActorRef mediator;

        public AloxiHub(ILogger<AloxiHub> log, ActorSystemProvider actorSystemProvider)
        {
            this.log = log;
            this.mediator = actorSystemProvider.Mediator;
            log.LogInformation("Created");
        }

        public override Task OnConnectedAsync()
        {
            log.LogInformation("AloxiHub connected");
            this.mediator.Tell(new SignalR.SignalRMessage.RegisterHub(this));
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            log.LogInformation("AloxiHub disconnected");
            this.mediator.Tell(new SignalR.SignalRMessage.UnregisterHub(this));
            return base.OnDisconnectedAsync(exception);
        }

        public void ToBridge(string messageType, string operation, string payload)
        {
            log.LogDebug($"ToBridge {messageType}/{operation}");
            this.mediator.Tell(new SignalR.SignalRMessage.ToBridge(messageType, operation, payload));
        }

        public void ToAdapter(string messageType, string operation, string payload)
        {
            log.LogDebug($"ToAdapter {messageType}/{operation}");
            Clients.All.SendCoreAsync("ToAdapter", new[] { messageType, operation, payload });
        }
    }
}
