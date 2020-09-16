using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stars.Interface.Router;
using Stars.Interface.Supply;
using Stars.Service.Router;
using Stars.Service.Supply.Destination;

namespace Stars.Service.Supply
{
    public class DeliveryQuotation : IDeliveryQuotation
    {
        private readonly ISupplier supplier;
        private readonly (IRoute, IOrderedEnumerable<IDelivery>) nodeQuotes;
        private IDictionary<IRoute, IOrderedEnumerable<IDelivery>> assetsDeliveryQuotes;

        public DeliveryQuotation(ISupplier supplier, (IRoute, IOrderedEnumerable<IDelivery>) nodeQuotes, IDictionary<IRoute, IOrderedEnumerable<IDelivery>> assetsQuotes)
        {
            this.supplier = supplier;
            this.nodeQuotes = nodeQuotes;
            this.assetsDeliveryQuotes = assetsQuotes;
        }

        public IEnumerable<ICarrier> Carriers => assetsDeliveryQuotes.SelectMany(q => q.Value.Select(q1 => q1.Carrier).Distinct()).Distinct();

        public IDictionary<IRoute, IOrderedEnumerable<IDelivery>> AssetsDeliveryQuotes => assetsDeliveryQuotes;

        public (IRoute, IOrderedEnumerable<IDelivery>) NodeDeliveryQuotes => nodeQuotes;

        public ISupplier Supplier => supplier;
    }
}