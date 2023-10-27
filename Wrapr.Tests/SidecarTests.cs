using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Wrapr.Tests;

public class SidecarTests
{
    private readonly ITestOutputHelper _output;

    public SidecarTests(ITestOutputHelper output) => 
        _output = output;

    [Fact]
    public async Task StartStop()
    {
        using var logger = _output.BuildLogger(LogLevel.Debug);
        await using var sidecar = new Sidecar("test-start-stop", logger);
            
        await sidecar.Start(with => with
            .ResourcesPath(Directory.CreateDirectory("resources-path").FullName)
            .AppPort(3015)
            .DaprGrpcPort(0)
            .DaprHttpPort(0)
            .Args("--log-level", "warn"));
        await sidecar.Stop();
    }
        
    [Fact]
    public async Task Error()
    {
        var act = async () =>
        {
            using var logger = _output.BuildLogger(LogLevel.Debug);
            await using var sidecar = new Sidecar("test-error", logger);
            await sidecar.Start(with => with.ResourcesPath("non-existing-components"));
        };
            
        await act
            .Should()
            .ThrowAsync<WraprException>()
            .WithMessage("*non-existing-components*");
    }
        
    [Fact]
    public async Task ErrorInput()
    {
        var act = async () =>
        {
            using var logger = _output.BuildLogger(LogLevel.Debug);
            await using var sidecar = new Sidecar("test-error-input", logger);
            await sidecar.Start(with => with.Args("--metrics-port", "x"));
        };
            
        await act
            .Should()
            .ThrowAsync<WraprException>()
            .WithMessage("Error: invalid argument \"x\" for \"-M, --metrics-port\"*");
    }
}