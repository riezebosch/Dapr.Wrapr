using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Dapr.Client;
using DaprDemo.Events;
using DaprDemo.Services;
using FluentAssertions.Extensions;
using Hypothesist;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;
using NSubstitute;

namespace DaprDemo.IntegrationTests
{
    public sealed class DemoControllerTests : IDisposable
    {
        private readonly IHost _host;
        private readonly IDemoService _service = Substitute.For<IDemoService>();

        public DemoControllerTests()
        {
            _host = new HostBuilder().ConfigureWebHost(app => app
                    .UseStartup<Startup>()
                    .ConfigureServices(services => services.AddSingleton(_service))
                    .UseKestrel(options => options.ListenLocalhost(5000)))
                .Build();
            _host.Start();
        }
        
        [Fact]
        public async Task FromHttp()
        {
            using var client = new HttpClient();
            var response = await client
                .PostAsJsonAsync("http://localhost:5000/demo/", new
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
                .For<Data>()
                .Any(x => x.Value == 1234);
            
            _service
                .When(x => x.Demo(Arg.Any<Data>()))
                .Do(x => hypothesis.Test(x.Arg<Data>()));

            // Make sure the sidecar is running before executing this test! See README.md
            var client = new DaprClientBuilder()
                .UseGrpcEndpoint("http://localhost:3000")
                .UseHttpEndpoint("http://localhost:5000")
                .Build();
            
            await client
                .PublishEventAsync("rabbitmq-pubsub", "Demo", new
                {
                    Value = 1234
                });

            await hypothesis.Validate(10.Seconds());
        }

        void IDisposable.Dispose() => 
            _host.Dispose();
    }
}