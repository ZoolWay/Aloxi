using System;
using Microsoft.AspNetCore.Mvc;

namespace ZoolWay.Aloxi.Bridge.Api.Controllers
{
    [ApiController]
    [Route("/")]
    public class RootController : ControllerBase
    {
        [HttpGet]
        public ActionResult Root()
        {
            return Redirect("/status");
        }
    }
}
