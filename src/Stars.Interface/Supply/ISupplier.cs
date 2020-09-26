using System.Collections.Generic;
using System.Threading.Tasks;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supply.Destination;

namespace Terradue.Stars.Interface.Supply
{
    public interface ISupplier : IPlugin
    {
        string Id { get; }

        // TODO add assets filters
        Task<INode> SearchFor(INode resource);
        IDeliveryQuotation QuoteDelivery(INode resource, IDestination destination);
        Task<IOrder> Order(IOrderable orderableRoute);
    }
}