using System;
using System.Threading.Tasks;

using Akka.Actor;

using Microsoft.AspNetCore.Mvc;

using ZoolWay.Aloxi.Bridge.Meta;

namespace ZoolWay.Aloxi.Bridge.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StatusController : ControllerBase
    {
        private static readonly TimeSpan MAX_ACTOR_WAIT = TimeSpan.FromSeconds(5);
        private readonly IActorRef statusConsolidator;

        public StatusController(ActorSystemProvider actorSystemProvider)
        {
            this.statusConsolidator = actorSystemProvider.StatusConsolidator;
        }

        [HttpGet]
        public async Task<ActionResult<Models.Status>> Get()
        {
            var response = await this.statusConsolidator.Ask<StatusMessage.Response>(new StatusMessage.Request(), MAX_ACTOR_WAIT);
            var s = new Models.Status();
            s.UpdateTimestamp = response.UpdateTimestamp;

            if (response.IsReady)
            {
                s.IsReady = true;
                s.MqttConnected = response.IsMqttConnected;
                s.MqttSubscribed = response.IsMqttSubscribed;
                s.MiniserverAvailable = response.IsMiniserverAvailable;

                var modelUriBuilder = new UriBuilder(Request.Scheme, Request.Host.Host, Request.Host.Port.Value, "homeModel");
                s.ModelUri = modelUriBuilder.ToString();
            }
            else
            {
                s.IsReady = false;
            }

            return s;
        }
    }
}
