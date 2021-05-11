using System.Threading.Tasks;
using DaprDemo.Events;
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
        public IActionResult PostAsync([FromBody]CloudEvent<Demo> demo, [FromServices]IService service)
        {
            service.Do(demo.Data);
            return Ok();
        }
    }
}