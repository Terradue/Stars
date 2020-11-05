using System.Threading.Tasks;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier.Destination;

namespace Terradue.Stars.Interface.Processing
{
    public interface IProcessing : IPlugin
    {
        bool CanProcess(IResource route, IDestination destination);

        Task<IResource> Process(IResource route, IDestination destination);

        string GetRelativePath(IResource route, IDestination destination);
    }
}