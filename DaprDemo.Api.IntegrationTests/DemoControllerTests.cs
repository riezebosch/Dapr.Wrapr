using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Dapr.Client;
using Divergic.Logging.Xunit;
using FluentAssertions;
using FluentAssertions.Extensions;
using Hypothesist;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;
using Wrapr;
using Xunit.Abstractions;

namespace DaprDemo.Api.IntegrationTests
{
    public sealed class DemoControllerTests : IDisposable, IAsyncLifetime
    {
        private readonly IHost _host;
        private readonly ICacheLogger _logger;
        private readonly Sidecar _sidecar;
        private readonly IHypothesis<int> _hypothesis= Hypothesis
            .For<int>()
            .Any(x => x == 1234);

        public DemoControllerTests(ITestOutputHelper output)
        {
            _logger = output.BuildLogger(LogLevel.Debug);
            _host = new HostBuilder().ConfigureWebHost(app => app
                    .UseStartup<Startup>()
                    .ConfigureLogging(builder => builder.AddXunit(output))
                    .ConfigureServices(services => services.AddSingleton<IHandler<int, int>>(new TestHandler<int,int>(_hypothesis, 5)))
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
                    value = 1234
                });

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Be("5");

            await _hypothesis
                .Validate(10.Seconds());
        }

        [Fact]
        public async Task FromEvent()
        {
            using var client = new DaprClientBuilder()
                .UseHttpEndpoint("http://localhost:3001")
                .UseGrpcEndpoint("http://localhost:3000")
                .Build();

            // await client.WaitForSidecarAsync();
            await client
                .PublishEventAsync("my-pubsub", "Demo", new
                {
                    Value = 1234
                });

            await _hypothesis
                .Validate(10.Seconds());
        }

        void IDisposable.Dispose()
        {
            _host.Dispose();
            _logger.Dispose();
        }

        public async Task InitializeAsync()
        {
            await _host.StartAsync();
            await _sidecar.Start(with => with
                .ResourcesPath("components")
                .AppPort(5555)
                // .DaprHttpPort(3001)
                .DaprGrpcPort(3000)
                .Args("--log-level", "debug"));
        }

        public async Task DisposeAsync() => 
            await _sidecar.DisposeAsync();
    }
}