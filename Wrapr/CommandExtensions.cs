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
                    .OfType<StandardOutputCommandEvent>()
                    .Select(x => x.Text))
                {
                    logger.LogDebug(line); 
                    if (Check(line, ready))
                    {
                        return;
                    }
                }
                
                // when reaching this point the sidecar is already stopped before we recognized a success or fail from the output.
                ready.TrySetException(new WraprException("Sidecar stopped prematurely, check the logger output for details."));
            });

            await ready.Task;
        }

        private static bool Check(string output, TaskCompletionSource ready) =>
            output.First() switch
            {
                '✅' => ready.TrySetResult(),
                '❌' => ready.TrySetException(new WraprException(output)),
                _ => false
            };
    }
}