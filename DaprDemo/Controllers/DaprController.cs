using Microsoft.AspNetCore.Mvc;

namespace DaprDemo.Controllers
{
    /// <summary>
    /// WIP: dapr doesn't seem to query this endpoint yet.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class DaprController : ControllerBase
    {
        [HttpGet("subscribe")]
        public IActionResult Subscribe() => Ok(new[]
        {
            new
            {
                pubsubname = "rabbitmq-pubsub",
                topic = "Demo",
                route = "demo"
            }
        });
    }
}