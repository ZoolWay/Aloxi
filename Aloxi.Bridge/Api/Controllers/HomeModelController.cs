using System;
using System.Threading.Tasks;

using Akka.Actor;

using Microsoft.AspNetCore.Mvc;

using ZoolWay.Aloxi.Bridge.Api.Models;
using ZoolWay.Aloxi.Bridge.Loxone;

namespace ZoolWay.Aloxi.Bridge.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeModelController : ControllerBase
    {
        private static readonly TimeSpan MAX_ACTOR_WAIT = TimeSpan.FromSeconds(5);
        private readonly IActorRef loxoneAdapterActor;

        public HomeModelController(ActorSystemProvider actorSystemProvider)
        {
            this.loxoneAdapterActor = actorSystemProvider.LoxoneAdapter;
        }

        [HttpGet]
        public async Task<ActionResult<HomeModel>> Get()
        {
            var hm = new HomeModel();
            var response = await this.loxoneAdapterActor.Ask<LoxoneMessage.PublishModel>(new LoxoneMessage.RequestModel(), MAX_ACTOR_WAIT);
            hm.Home = response.Model;
            hm.UpdateTimestamp = response.UpdateTimestamp;
            return hm;
        }
    }
}
