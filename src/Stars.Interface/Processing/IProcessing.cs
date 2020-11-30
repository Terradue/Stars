using System.Threading.Tasks;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier.Destination;

namespace Terradue.Stars.Interface.Processing
{
    public interface IProcessing : IPlugin
    {
        bool CanProcess(IResource resource, IDestination destination);

        Task<IResource> Process(IResource resource, IDestination destination, string suffix = null);

        string GetRelativePath(IResource resource, IDestination destination);
    }
}