using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Dapr.Client;
using DaprDemo.Events;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using Xunit;

namespace DaprDemo.IntegrationTests
{
    public sealed class DemoControllerTests : IDisposable
    {
        private readonly AutoResetEvent _received = new(false);
        private readonly IService _service = Substitute.For<IService>();
        private readonly IHost _host; 

        public DemoControllerTests()
        {
            _host = new HostBuilder().ConfigureWebHost(app => app
                    .UseStartup<Startup>()
                    .ConfigureServices(services => services.AddSingleton(_service))
                    .UseKestrel(options => options.ListenLocalhost(5000)))
                .Build();
            _host.Start();
            
            _service
                .When(async x => await x.Do(Arg.Any<Demo>()))
                .Do(_ => _received.Set());
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
                        value = 1234
                    }
                });
            
            response.EnsureSuccessStatusCode();
            await _service
                .Received()
                .Do(new Demo { Value = 1234 });
        }
        
        [Fact]
        public async Task FromEvent()
        {
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
            
            _received
                .WaitOne(TimeSpan.FromSeconds(20))
                .Should()
                .BeTrue();

            await _service
                .Received()
                .Do(new Demo { Value = 1234 });
        }

        void IDisposable.Dispose()
        {
             _received.Dispose();
            _host.Dispose();
        }
    }
}