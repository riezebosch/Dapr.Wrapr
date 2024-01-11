using System.Threading.Tasks;
using Dapr.Client;
using FluentAssertions.Extensions;
using Hypothesist;
using Hypothesist.AspNet;
using Hypothesist.AspNet.Endpoint;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Wrapr;
using Xunit.Abstractions;

namespace DaprDemo.Api.IntegrationTests;

public sealed class DemoTests
{
    private readonly ITestOutputHelper _output;
    private const int DaprHttpPort = 3001;
    private const int DaprGrpcPort = 3000;
    private const int AppPort = 5555;

    public DemoTests(ITestOutputHelper output) => 
        _output = output;

    [Fact]
    public async Task FromEvent()
    {
        var observer = Observer
            .For<Data>();
        
        await using var app = await App(observer);
        await using var sidecar = await Sidecar();

        using var client = new DaprClientBuilder()
            .UseGrpcEndpoint($"http://localhost:{DaprGrpcPort}")
            .Build();

        await client
            .PublishEventAsync("my-pubsub", "Demo", new
            {
                Value = 1234
            });

        await Hypothesis
            .On(observer)
            .Timebox(10.Seconds())
            .Any()
            .Match(new Data(1234))
            .Validate();
    }

    [Fact]
    public async Task UsingClientMethodsToManageSidecar()
    {
        var observer = Observer
            .For<Data>();
        
        await using var app = await App(observer);
        await using var sidecar = await Sidecar();

        using var client = new DaprClientBuilder()
            .UseHttpEndpoint($"http://localhost:{DaprHttpPort}")
            .UseGrpcEndpoint($"http://localhost:{DaprGrpcPort}")
            .Build();

        await client.WaitForSidecarAsync();
        await client
            .PublishEventAsync("my-pubsub", "Demo", new
            {
                Value = 1234
            });

        await Hypothesis
            .On(observer)
            .Timebox(10.Seconds())
            .Any()
            .Match(new Data(1234))
            .Validate();

        await client.ShutdownSidecarAsync();
        await Task.Delay(1.Seconds()); // this will interferes with the other tests otherwise, because the port is not available in time.
    }

    private async Task<WebApplication> App(Observer<Data> observer)
    {
        var builder = WebApplication.CreateBuilder(new[] { $"--urls=http://localhost:{AppPort}" });
        builder
            .Services
            .AddSingleton(_ =>
            {
                var mock = Substitute.For<Do>();
                mock.SomeMagic(1234).Returns(5);

                return mock;
            });

        builder.Logging.AddXunit(_output);

        var app = builder.Build();
        app.MapSubscribeHandler();
        app.UseDeveloperExceptionPage()
            .UseCloudEvents();
        
        app.Map().AddEndpointFilter(observer
            .FromEndpoint()
            .With(x => x.GetArgument<Data>(0))
            .When(x => x.HttpContext.Request.Path == "/orders"));

        await app.StartAsync();
        return app;
    }

    private async Task<Sidecar> Sidecar()
    {
        var sidecar = new Sidecar("demo-app", _output.BuildLogger());
        await sidecar.Start(with => with
            .ResourcesPath("components")
            .AppPort(AppPort)
            .DaprHttpPort(DaprHttpPort)
            .DaprGrpcPort(DaprGrpcPort)
            .Args("--log-level", "debug"));

        return sidecar;
    }
}