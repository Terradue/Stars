using System.Threading.Tasks;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier.Destination;

namespace Terradue.Stars.Interface.Processing
{
    public interface IProcessing : IPlugin
    {
        bool CanProcess(IRoute route, IDestination destination);

        Task<IRoute> Process(IRoute route, IDestination destination);

        string GetRelativePath(IRoute route, IDestination destination);
    }
}