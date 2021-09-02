using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Dapr.Client;
using Divergic.Logging.Xunit;
using FluentAssertions.Extensions;
using Hypothesist;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;
using NSubstitute;
using Wrapr;
using Xunit.Abstractions;

namespace DaprDemo.Api.IntegrationTests
{
    public sealed class DemoControllerTests : IDisposable, IAsyncLifetime
    {
        private readonly IHost _host;
        private readonly IHandler<int, int> _service = Substitute.For<IHandler<int, int>>();
        private readonly ICacheLogger _logger;
        private readonly Sidecar _sidecar;

        public DemoControllerTests(ITestOutputHelper output)
        {
            _logger = output.BuildLogger(LogLevel.Debug);
            _host = new HostBuilder().ConfigureWebHost(app => app
                    .UseStartup<Startup>()
                    .ConfigureLogging(builder => builder.AddXunit(output))
                    .ConfigureServices(services => services.AddSingleton(_service))
                    .UseKestrel(options => options.ListenLocalhost(5555)))
                .Build();
            _sidecar = new Sidecar("demo-app", _logger);
        }

        [Fact]
        public async Task FromHttp()
        {
            using var client = new HttpClient();
            var response = await client
                .PostAsJsonAsync("http://localhost:5555/demo/", new
                {
                    data = new
                    {
                        value = 8374
                    }
                });

            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task FromEvent()
        {
            var hypothesis = Hypothesis
                .For<int>()
                .Any(x => x == 1234);

            _service
                .Handle(Arg.Any<int>())
                .Returns(5)
                .AndDoes(x => hypothesis.Test(x.Arg<int>()));

            using var client = new DaprClientBuilder()
                .UseGrpcEndpoint("http://localhost:3000")
                .Build();

            await client
                .PublishEventAsync("my-pubsub", "Demo", new
                {
                    Value = 1234
                });

            await hypothesis.Validate(10.Seconds());
        }

        void IDisposable.Dispose()
        {
            _host.Dispose();
            _logger.Dispose();
        }

        public async Task InitializeAsync()
        {
            await _host.StartAsync();
            await _sidecar.Start(args => args
                .ComponentsPath("components")
                .AppPort(5555)
                .DaprGrpcPort(3000)
                .Args("--log-level", "debug"));
        }

        public async Task DisposeAsync() => 
            await _sidecar.DisposeAsync();
    }
}