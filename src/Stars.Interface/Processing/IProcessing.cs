using System.Threading;
using System.Threading.Tasks;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier.Destination;

namespace Terradue.Stars.Interface.Processing
{
    public interface IProcessing : IPlugin
    {
        ProcessingType ProcessingType { get; }
        string Label { get; }

        bool CanProcess(IResource resource, IDestination destination);

        Task<IResource> ProcessAsync(IResource resource, IDestination destination, CancellationToken ct, string suffix = null);

        string GetRelativePath(IResource resource, IDestination destination);
    }
}