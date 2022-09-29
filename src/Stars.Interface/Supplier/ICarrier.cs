using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier.Destination;

namespace Terradue.Stars.Interface.Supplier
{
    public interface ICarrier : IPlugin
    {
        string Id { get; }
        
        bool CanDeliver(IResource route, IDestination destination);

        Task<IResource> DeliverAsync (IDelivery delivery, CancellationToken ct, bool overwrite = false);
        
        IDelivery QuoteDelivery(IResource route, IDestination destination);

    }
}