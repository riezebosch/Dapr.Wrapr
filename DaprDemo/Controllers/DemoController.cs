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
        public async Task<IActionResult> GetAsync() => await Task.FromResult(Ok());
        
        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody]CloudEvent<Demo> demo, [FromServices]IService service) => Ok(await service.Do(demo.Data));
    }
}