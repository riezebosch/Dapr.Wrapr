using System.Threading.Tasks;
using Hypothesist;

namespace DaprDemo.Api.IntegrationTests
{
    public class TestHandler<TIn, TResult> : IHandler<TIn,TResult>
    {
        private readonly IHypothesis<TIn> _hypothesis;
        private readonly TResult _result;

        public TestHandler(IHypothesis<TIn> hypothesis, TResult result)
        {
            _hypothesis = hypothesis;
            _result = result;
        }

        public async Task<TResult> Handle(TIn input)
        {
            await _hypothesis.Test(input);
            return _result;
        }
    }
}