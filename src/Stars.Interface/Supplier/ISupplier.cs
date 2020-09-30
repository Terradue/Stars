using System.Collections.Generic;
using System.Threading.Tasks;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier.Destination;

namespace Terradue.Stars.Interface.Supplier
{
    public interface ISupplier : IPlugin
    {
        string Id { get; }

        // TODO add assets filters
        Task<IRoute> SearchFor(IRoute item);

        IDeliveryQuotation QuoteDelivery(IRoute resource, IDestination destination);

        Task<IOrder> Order(IOrderable orderableRoute);
    }
}