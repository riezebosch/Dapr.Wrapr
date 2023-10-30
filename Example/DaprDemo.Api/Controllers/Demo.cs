using System.Threading.Tasks;
using Dapr;
using Microsoft.AspNetCore.Mvc;

namespace DaprDemo.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class Demo : ControllerBase
{
    [HttpPost]
    [Topic("my-pubsub", "Demo")]
    public async Task<IActionResult> PostAsync(Data data, [FromServices]Do @do) => 
        Ok(await @do.SomeMagic(data.Value));
        
    public record Data(int Value);
}