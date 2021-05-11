using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace DaprDemo.IntegrationTests
{
    public sealed class DaprControllerTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;

        public DaprControllerTests(WebApplicationFactory<Startup> factory) => _factory = factory;

        [Fact]
        public async Task Test()
        {
            using var client = _factory.CreateClient();
            var response = await client.GetFromJsonAsync<Subscription[]>("dapr/subscribe");

            response
                .Should()
                .BeEquivalentTo(new[] { new Subscription("rabbitmq-pubsub", "Demo", "demo")});
        }

        private record Subscription(string pubsubname, string topic, string route);
    }
}