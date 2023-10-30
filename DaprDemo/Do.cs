using System.Threading.Tasks;

namespace DaprDemo
{
    public interface Do
    {
        Task<int> SomeMagic(int input);
    }
}