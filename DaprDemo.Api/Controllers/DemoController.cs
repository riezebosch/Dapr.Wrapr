using System.Threading.Tasks;
using Dapr;
using Microsoft.AspNetCore.Mvc;

namespace DaprDemo.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DemoController : ControllerBase
    {
        [HttpPost]
        [Topic("rabbitmq-pubsub", "Demo")]
        public async Task<IActionResult> PostAsync(Data data, [FromServices]IHandler<int, int> handler) => 
            Ok(await handler.Handle(data.Value));
        
        public record Data(int Value);
    }
}