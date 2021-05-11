using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Dapr.Client;
using DaprDemo.Events;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace DaprDemo.IntegrationTests
{
    public sealed class DemoControllerTests : IDisposable
    {
        private readonly DummyService _service = new();
        private readonly IHost _host; 

        public DemoControllerTests()
        {
            _host = new HostBuilder().ConfigureWebHost(app => app
                    .UseStartup<Startup>()
                    .ConfigureServices(services => services.AddSingleton<IService>(_service))
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
                        value = 1234
                    }
                });
            
            response.EnsureSuccessStatusCode();
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

            var result = await _service.Received();
            result.Should().Be(new Demo { Value = 1234 });
        }

        void IDisposable.Dispose() => 
            _host.Dispose();

        private class DummyService : IService
        {
            private readonly TaskCompletionSource<Demo> _received = new ();
            public Task<Demo> Received() => 
                _received.Task;

            void IService.Do(Demo demo) => 
                _received.SetResult(demo);
        }
    }
}