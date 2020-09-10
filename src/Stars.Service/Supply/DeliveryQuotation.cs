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
        private IDictionary<IRoute, IOrderedEnumerable<IDelivery>> deliveryRoutes;

        public DeliveryQuotation(IDictionary<IRoute, IOrderedEnumerable<IDelivery>> assetsQuote)
        {
            this.deliveryRoutes = assetsQuote;
        }

        public IEnumerable<ICarrier> Carriers => deliveryRoutes.SelectMany(q => q.Value.Select(q1 => q1.Carrier).Distinct()).Distinct();

        public IDictionary<IRoute, IOrderedEnumerable<IDelivery>> DeliveryQuotes => deliveryRoutes;
    }
}