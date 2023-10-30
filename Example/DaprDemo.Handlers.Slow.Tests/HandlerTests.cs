using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace DaprDemo.Handlers.Slow.Tests;

public class HandlerTests
{
    private readonly Adapter _do = new(NullLogger.Instance);

    [Theory]
    [InlineData(0,0)]
    [InlineData(1,1)]
    [InlineData(2,1)]
    [InlineData(3,2)]
    [InlineData(4,3)]
    [InlineData(5,5)]
    [InlineData(6,8)]
    public async Task Test(int input, int expected)
    {
        var result = await _do.SomeMagic(input);
        result
            .Should()
            .Be(expected);
    }
}