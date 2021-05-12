using DaprDemo.Events;
using DaprDemo.Services;
using Microsoft.AspNetCore.Mvc;

namespace DaprDemo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DemoController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetAsync() => 
            Ok();
        
        [HttpPost]
        public IActionResult PostAsync([FromBody]CloudEvent<Data> @event, [FromServices]IDemoService service)
        {
            service.Demo(@event.Data);
            return Ok();
        }
    }
}