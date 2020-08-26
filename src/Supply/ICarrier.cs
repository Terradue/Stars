using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Stars.Router;
using Stars.Supply.Asset;
using Stars.Supply.Destination;

namespace Stars.Supply
{
    public interface ICarrier
    {
        string Id { get; }
        
        void Configure(IConfigurationSection configuration);

        bool CanDeliver(IRoute route, ISupplier supplier, IDestination destination);

        Task<IRoute> Deliver (IRoute route, ISupplier supplier, IDestination destination);
        
        Delivery QuoteDelivery(IRoute route, ISupplier supplier, IDestination destination);

    }
}