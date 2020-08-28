using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stars.Router;
using Stars.Supply.Destination;

namespace Stars.Supply
{
    public class DeliveryQuotation
    {
        private IDictionary<IRoute, IOrderedEnumerable<Delivery>> deliveryRoutes;

        public DeliveryQuotation(IDictionary<IRoute, IOrderedEnumerable<Delivery>> assetsQuote)
        {
            this.deliveryRoutes = assetsQuote;
        }

        public IEnumerable<ICarrier> Carriers => deliveryRoutes.SelectMany(q => q.Value.Select(q1 => q1.Carrier).Distinct()).Distinct();

        public IDictionary<IRoute, IOrderedEnumerable<Delivery>> DeliveryQuotes => deliveryRoutes;
    }
}