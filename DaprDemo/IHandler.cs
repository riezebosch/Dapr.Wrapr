using System.Threading.Tasks;

namespace DaprDemo
{
    public interface IHandler<in TIn, TResult>
    {
        Task<TResult> Handle(TIn input);
    }
}