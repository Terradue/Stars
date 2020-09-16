using System.Collections.Generic;
using System.Threading.Tasks;
using Stars.Interface.Model;
using Stars.Interface.Router;
using Stars.Interface.Supply.Destination;

namespace Stars.Interface.Supply
{
    public interface ISupplier 
    {
        string Id { get; }

        // TODO add assets filters
        Task<INode> SearchFor(INode resource);
        IDeliveryQuotation QuoteDelivery(INode resource, IDestination destination);
        Task<IOrder> Order(IOrderable orderableRoute);
    }
}