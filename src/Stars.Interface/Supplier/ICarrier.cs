using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier.Destination;

namespace Terradue.Stars.Interface.Supplier
{
    public interface ICarrier : IPlugin
    {
        string Id { get; }
        
        bool CanDeliver(IRoute route, ISupplier supplier, IDestination destination);

        Task<IRoute> Deliver (IDelivery delivery);
        
        IDelivery QuoteDelivery(IRoute route, ISupplier supplier, IDestination destination);

    }
}