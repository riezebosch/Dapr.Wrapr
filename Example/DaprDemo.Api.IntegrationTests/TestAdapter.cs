using System.Threading.Tasks;
using Hypothesist;

namespace DaprDemo.Api.IntegrationTests;

internal class TestAdapter : Do
{
    private readonly IHypothesis<int> _hypothesis;

    public TestAdapter(IHypothesis<int> hypothesis) => 
        _hypothesis = hypothesis;

    async Task<int> Do.SomeMagic(int input)
    {
        await _hypothesis.Test(input);
        return 5;
    }
}