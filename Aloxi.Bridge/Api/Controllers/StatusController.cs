using System;
using Microsoft.AspNetCore.Mvc;
using ZoolWay.Aloxi.Bridge.Api.Models;

namespace ZoolWay.Aloxi.Bridge.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StatusController : ControllerBase
    {
        [HttpGet]
        public ActionResult<Status> Get()
        {
            return new Status();
        }
    }
}
