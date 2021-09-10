using System.Threading.Tasks;

namespace Dapr.Wrapr.Example.Api
{
    public interface IHandler
    {
        Task<int> Handle(int input);
    }
}