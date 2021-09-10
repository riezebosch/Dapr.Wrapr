using System.Threading.Tasks;

namespace DaprDemo.Api
{
    public interface IHandler<in TIn, TResult>
    {
        Task<TResult> Handle(TIn input);
    }
}