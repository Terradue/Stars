using System.Threading.Tasks;

namespace Stars.Operations
{
    internal interface IOperation
    {
        Task ExecuteAsync();
    }
}