using System.Threading.Tasks;
using DaprDemo.Events;

namespace DaprDemo
{
    public interface IService
    {
        Task<object> Do(Demo demo);
    }
}