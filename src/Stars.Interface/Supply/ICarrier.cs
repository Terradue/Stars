using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Stars.Interface.Router;
using Stars.Interface.Supply.Destination;

namespace Stars.Interface.Supply
{
    public interface ICarrier
    {
        string Id { get; }
        
        void Configure(IConfigurationSection configuration);

        bool CanDeliver(IRoute route, ISupplier supplier, IDestination destination);

        Task<IRoute> Deliver (IDelivery delivery);
        
        IDelivery QuoteDelivery(IRoute route, ISupplier supplier, IDestination destination);

    }
}