# Dapr Wrapr

[![nuget](https://img.shields.io/nuget/v/Wrapr.svg)](https://www.nuget.org/packages/Wrapr/)
[![codecov](https://codecov.io/gh/riezebosch/Dapr.Wrapr/branch/main/graph/badge.svg)](https://codecov.io/gh/riezebosch/Dapr.Wrapr)
[![stryker](https://img.shields.io/endpoint?style=flat&label=stryker&url=https%3A%2F%2Fbadge-api.stryker-mutator.io%2Fgithub.com%2Friezebosch%2FDapr.Wrapr%2Fmain)](https://dashboard.stryker-mutator.io/reports/github.com/riezebosch/Dapr.Wrapr/main)
[![build status](https://ci.appveyor.com/api/projects/status/hylup50uv4x0lvta/branch/main?svg=true)](https://ci.appveyor.com/project/riezebosch/dapr-wrapr)

Wrapr is a library for [Dapr](https://dapr.io) to start and stop a sidecar to support integration testing.
It works particularly well with the [In Memory](https://docs.dapr.io/reference/components-reference/supported-pubsub/setup-inmemory/) pubsub component
but can also be used with other components.

## E2E pub/sub example

### Arrange

```c#
await using var sidecar = new Sidecar("integration-test");
await sidecar.Start(with => with
    .ResourcesPath("components-path")
    .DaprGrpcPort(1234) 
    .Args("--log-level", "warn"));

await sidecar.Stop();
```

Use it in combination with the [WebApplicationFactory](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-5.0#basic-tests-with-the-default-webapplicationfactory) or the [HostBuilder](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-5.0) to also host your web
application from the test.

```c#
new HostBuilder().ConfigureWebHost(app => app
    .UseStartup<Startup>()
    .ConfigureServices(services => services.AddSingleton(service))
    .UseKestrel(options => options.ListenLocalhost(<mark>5555</mark>)))
.Build();
```

### Act

```c#
using var client = new DaprClientBuilder()
    .UseGrpcEndpoint("http://localhost:1234")
    .Build();

await client
    .PublishEventAsync("my-pubsub", "Demo", new
    {
        Value = 1111
    });
```

### Assert

If you really want to validate the message arrival on the controller action
you probably need to stub an underlying service. Since this is an asynchronous
operation by default you will need some mechanism for future completion.

I've had great success with my own "future assertion" library [hypothesist](https://github.com/riezebosch/hypothesist).

```c#
var hypothesis = Hypothesis
    .For<int>()
    .Any(x => x == 1111);
```

After that you only need a stub that tests the hypothesis before you can validate it.
For example using [NSubstitute](https://nsubstitute.github.io/):

```c#
service
    .SomeMagic(Arg.Any<int>())
    .Returns(x => hypothesis.Test(x.Arg<int>()));
```

or with a very slim hand-rolled implementation that does exactly that:

```c#
private class TestAdapter : Do
{
    private readonly IHypothesis<int> _hypothesis;

    public TestAdapter(IHypothesis<int> hypothesis) => 
        _hypothesis = hypothesis;

    Task<int> Do.SomeMagic(int input) =>
        _hypothesis.Test(input);
}
```

Inject that stub into the service collection used by the WebApplicationFactory or HostBuilder.
After that you validate the hypothesis of having received a message with specified shape:

```c#
await hypothesis
    .Validate(10.Seconds());
```

You should checkout the [example](Example) for the full picture.