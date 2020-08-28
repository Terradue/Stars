using System.Collections.Generic;
using System.Threading.Tasks;
using Stars.Interface.Router;
using Stars.Interface.Supply.Destination;

namespace Stars.Interface.Supply
{
    public interface ISupplier
    {
        string Id { get; }

        bool CanSupply(IResource resource);

        // TODO add assets filters
        Task<IEnumerable<IResource>> LocalizeResource(IResource resource);
        IDeliveryQuotation QuoteDelivery(IResource resource, IDestination destination);
    }
}