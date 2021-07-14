using Dapr;
using DaprDemo.Events;
using DaprDemo.Services;
using Microsoft.AspNetCore.Mvc;

namespace DaprDemo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DemoController : ControllerBase
    {
        [HttpPost]
        [Topic("rabbitmq-pubsub", "Demo")]
        public IActionResult PostAsync(Data data, [FromServices]IDemoService service)
        {
            service.Demo(data);
            return Ok();
        }
    }
}