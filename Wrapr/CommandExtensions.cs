using System.Linq;
using System.Text.Json;
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
                    if (line.StartsWith("{"))
                    {
                        var output = JsonSerializer.Deserialize<Output>(line);
                        logger.LogInformation(output.Message.Trim());

                        Check(output, ready);
                    }
                    else
                    {
                        logger.LogDebug(line); 
                    }
                }
            });

            await ready.Task.ConfigureAwait(false);
        }

        private static void Check(Output output, TaskCompletionSource ready)
        {
            switch (output.Status)
            {
                case "success":
                    ready.TrySetResult();
                    break;
                case "failure":
                    ready.TrySetException(new WraprException(output.Message));
                    break;
            }
        }

        private struct Output
        {
            [JsonPropertyName("msg")]
            public string Message { get; set; }
            [JsonPropertyName("status")]
            public string Status { get; set; }
        }
    }
}