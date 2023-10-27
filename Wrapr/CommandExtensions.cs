using System.Linq;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.EventStream;
using Microsoft.Extensions.Logging;

namespace Wrapr;

internal static class CommandExtensions
{
    public static async ValueTask Ready(this Command command, string expected, ILogger logger) =>
        await command
            .WithStandardErrorPipe(PipeTarget.ToDelegate(s => logger.LogError(s)))
            .WithStandardOutputPipe(PipeTarget.ToDelegate(s => logger.LogInformation(s)))
            .ListenAsync()
            .Select(x => x)
            .AnyAsync(line => Check(line, expected));

    private static bool Check(CommandEvent command, string ready) =>
        command switch
        {
            ExitedCommandEvent => true,
            StandardOutputCommandEvent output when output.Text.Contains(ready) => true,
            StandardErrorCommandEvent error =>  throw new WraprException(error.Text),
            _ => false
        };
}