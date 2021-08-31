using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Wrapr.Tests
{
    public class SidecarTests
    {
        private readonly ITestOutputHelper _output;

        public SidecarTests(ITestOutputHelper output) => 
            _output = output;

        [Fact]
        public async Task StartStop()
        {
            using var logger = _output.BuildLogger(LogLevel.Debug);
            await using var sidecar = new Sidecar("asdf", logger);
            
            await sidecar.Start(with => with
                .ComponentsPath(Directory.CreateDirectory("components-path").FullName)
                .AppPort(3000)
                .DaprGrpcPort(1234)
                .Args("--log-level", "warn"));
            await sidecar.Stop();
        }
        
        [Fact]
        public async Task Error()
        {
            Func<Task> act = async () =>
            {
                await using var sidecar = new Sidecar("asdf");
                await sidecar.Start(with => with.ComponentsPath("non-existing-components"));
            };
            
            await act
                .Should()
                .ThrowAsync<WraprException>()
                .WithMessage("** non-existing-components: no such file or directory");
        }
    }
}