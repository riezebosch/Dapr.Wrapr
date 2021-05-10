using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace DaprDemo.IntegrationTests
{
    public class UnitTest1 : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;

        public UnitTest1(WebApplicationFactory<Startup> factory) => _factory = factory.WithWebHostBuilder(builder  => builder.UseUrls("http://localhost:1234").UseKestrel(options => options.ListenLocalhost(1234)));

        [Fact]
        public async Task Get()
        {
            using var client = _factory.CreateClient();
            var response = await client.GetAsync("/demo/");

            response.EnsureSuccessStatusCode();
        }
        
        [Fact]
        public async Task Post()
        {
            using var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync("/demo/", new { id = 1234 });

            response.EnsureSuccessStatusCode();
        }
        
        [Fact]
        public async Task Test()
        {
            var host = new HostBuilder().ConfigureWebHost(app => app
                .UseStartup<Startup>()
                .UseKestrel(options => options.ListenLocalhost(1234)));

            await host.StartAsync();
            
            using var client = new HttpClient();
            var response = await client.PostAsJsonAsync("http://localhost:1234/demo/", new { id = 1234 });
            response.EnsureSuccessStatusCode();
        }
    }
}