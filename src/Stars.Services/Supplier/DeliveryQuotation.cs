using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Router;
using Terradue.Stars.Services.Supplier.Destination;

namespace Terradue.Stars.Services.Supplier
{
    public class DeliveryQuotation : IDeliveryQuotation
    {
        private readonly (IRoute, IOrderedEnumerable<IDelivery>) nodeQuotes;
        private IDictionary<string, IOrderedEnumerable<IDelivery>> assetsDeliveryQuotes;
        private readonly IDestination destination;

        public DeliveryQuotation((IRoute, IOrderedEnumerable<IDelivery>) nodeQuotes, IDictionary<string, IOrderedEnumerable<IDelivery>> assetsQuotes, IDestination destination)
        {
            this.nodeQuotes = nodeQuotes;
            this.assetsDeliveryQuotes = assetsQuotes;
            this.destination = destination;
        }

        public IEnumerable<ICarrier> Carriers => assetsDeliveryQuotes.SelectMany(q => q.Value.Select(q1 => q1.Carrier).Distinct()).Distinct();

        public IDictionary<string, IOrderedEnumerable<IDelivery>> AssetsDeliveryQuotes => assetsDeliveryQuotes;

        public (IRoute, IOrderedEnumerable<IDelivery>) NodeDeliveryQuotes => nodeQuotes;

        public IDestination Destination => destination;
    }
}