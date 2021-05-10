using System.Threading.Tasks;
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
        public async Task<IActionResult> PostAsync([FromBody]Demo demo) => await Task.FromResult(Ok());

        public class Demo
        {
            public int Id { get; set; }
        }
    }
}