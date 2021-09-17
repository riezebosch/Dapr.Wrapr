using System;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.EventStream;
using Microsoft.Extensions.Logging;

namespace Wrapr
{
    internal static class CommandExtensions
    {
        public static async ValueTask Ready(this Command command, ILogger logger)
        {
            var ready = new TaskCompletionSource();
            _ = Task.Run(async () =>
            {
                await foreach (var line in command
                    .ListenAsync()
                    .Select(x => x.ToString()))
                {
                    logger.LogDebug(line);
                    if (Check(line, ready))
                    {
                        return;
                    }
                }
            });

            await ready.Task;
        }

        private static bool Check(string output, TaskCompletionSource ready) =>
            output.FirstOrDefault() switch
            {
                '✅' => ready.TrySetResult(),
                '❌' => ready.TrySetException(new WraprException(output)),
                _ => false
            };
    }
}