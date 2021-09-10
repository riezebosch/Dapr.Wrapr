using System.Threading.Tasks;
using Dapr.Client;
using FluentAssertions.Extensions;
using Hypothesist;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Wrapr;
using Xunit;
using Xunit.Abstractions;

namespace Dapr.Wrapr.Example.Api.IntegrationTests
{
    public sealed class DaprWraprIntegrationTests
    {
        private readonly ITestOutputHelper _output;

        public DaprWraprIntegrationTests(ITestOutputHelper output)
        {
            _output = output;
        }
        
        [Fact]
        public async Task Receive_CloudEvent_WithInmemoryDaprSidecar()
        {
            // Add a hypothesis to expect that 1234 is received somewhere in the future
            var hypothesis = Hypothesis
                .For<int>()
                .Any(x => x == 1234);
            
            // Mock the handler and test the hypothesis
            IHandler handlerMock = Substitute.For<IHandler>();
            handlerMock
                .Handle(Arg.Any<int>())
                .Returns(5)
                .AndDoes(x => hypothesis.Test(x.Arg<int>()));
            
            // Host an inmemory dotnet core api and register the handler mock
            var logger = _output.BuildLogger(LogLevel.Debug);
            var host = new HostBuilder()
                .ConfigureWebHost(app => app
                    .UseStartup<Startup>()
                    .ConfigureServices(services => services.AddSingleton(handlerMock))
                    .UseKestrel(options => options.ListenLocalhost(5555)))
                .Build();
            
            await host.StartAsync();

            // Start the Dapr sidecar with the in-memory servicebus using the sidecar configuration pubsub.yml found in components/* folder
            var sidecar = new Sidecar("demo-app", logger);
            await sidecar.Stop();
            
            await sidecar.Start(args => args
                .ComponentsPath("components")
                .AppPort(5555)
                .DaprGrpcPort(3000)
                .Args("--log-level", "debug"));

            // Publish to topic ExampleTopic in the my-pubsub Dapr sidecar 
            using var client = new DaprClientBuilder()
                .UseGrpcEndpoint("http://localhost:3000")
                .Build();

            await client.PublishEventAsync("my-pubsub", "ExampleTopic", new
            {
                Value = 1234
            });
            
            // Assert that 1234 is received in the past or within the next 10 seconds
            await hypothesis.Validate(10.Seconds());
            
            // Dispose
            await sidecar.DisposeAsync();
            host.Dispose();
            client.Dispose();
        }
    }
}