using System;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Wrapr
{
    public class Sidecar : IAsyncDisposable
    {
        private readonly string _appId;
        private readonly ILogger _logger;

        public Sidecar(string appId, ILogger logger = null)
        {
            _appId = appId;
            _logger = logger ?? NullLogger.Instance;
        }

        public ValueTask Start(Func<Run, Run> with) => 
            Cli.Wrap("dapr")
                .WithArguments(with(Run.Create(_appId)).Arguments)
                .Ready(_logger);

        public async ValueTask Stop()
        {
            if (await IsRunning())
            {
                await Cli.Wrap("dapr")
                    .WithArguments(new [] { "stop", "--app-id", _appId })
                    .Ready(_logger)
                    .ConfigureAwait(false);
            }
        }

        private async Task<bool> IsRunning()
        {
            var result = await Cli
                .Wrap("dapr")
                .WithArguments("list")
                .ExecuteBufferedAsync();
            _logger.LogDebug(result.StandardOutput);

            return result.StandardOutput.Contains(_appId);
        }

        public ValueTask DisposeAsync() =>
            Stop();
    }
}