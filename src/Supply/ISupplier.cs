using System.Collections.Generic;
using System.Threading.Tasks;
using Stars.Router;
using Stars.Supply.Asset;
using Stars.Supply.Destination;

namespace Stars.Supply
{
    public interface ISupplier
    {
        string Id { get; }

        bool CanSupply(IResource resource);

        // TODO add assets filters
        Task<IEnumerable<IResource>> LocalizeResource(IResource resource);
        DeliveryQuotation QuoteDelivery(IResource resource, IDestination destination);
    }
}