using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Dapr.Wrapr.Example.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExampleController : ControllerBase
    {
        [HttpPost]
        [Topic("my-pubsub", "ExampleTopic")]
        public async Task<IActionResult> PostAsync(Data data, [FromServices] IHandler handler)
        {
            return Ok(await handler.Handle(data.Value));
        }
    
        public record Data(int Value);
    }
}