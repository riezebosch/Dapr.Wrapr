using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DaprDemo.Handlers.Slow;

public class Handler : IHandler<int, int>
{
    private readonly ILogger _logger;

    public Handler(ILogger logger) => 
        _logger = logger;

    public async Task<int> Handle(int input) => 
        await Slow(input);

    private async Task<int> Slow(int n)
    {
        _logger.LogInformation(n.ToString());
        if (n is 0 or 1)
            return n;

        var a = Slow(n - 1);
        var b = Slow(n - 2);

        return  await a + await b;
    }
}