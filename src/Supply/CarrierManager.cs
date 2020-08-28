using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Stars.Router;
using Stars.Supply.Asset;
using Stars.Supply.Destination;

namespace Stars.Supply
{
    public class CarrierManager : AbstractManager<ICarrier>
    {
        private readonly IReporter reporter;
        private readonly IConfiguration configuration;

        public CarrierManager(IEnumerable<ICarrier> carriers, IReporter reporter, IConfiguration configuration) : base(carriers)
        {
            this.reporter = reporter;
            this.configuration = configuration;
        }

        internal IEnumerable<ICarrier> GetCarriers(IRoute route, ISupplier supplier, IDestination destination)
        {
            return _items.Where(r => r.CanDeliver(route, supplier, destination));
        }

        private Dictionary<IRoute, IOrderedEnumerable<Delivery>> GetAssetsDeliveryQuotations(ISupplier supplier, IAssetsContainer assetsContainer, IDestination destination)
        {
            Dictionary<IRoute, IOrderedEnumerable<Delivery>> routesQuotes = new Dictionary<IRoute, IOrderedEnumerable<Delivery>>();

            foreach (var route in assetsContainer.GetAssets())
            {
                var assetsDeliveryQuotations = GetSingleDeliveryQuotations(supplier, route, destination);
                routesQuotes.Add(route, assetsDeliveryQuotations);
            }
            return routesQuotes;
        }

        private IOrderedEnumerable<Delivery> GetSingleDeliveryQuotations(ISupplier supplier, IRoute route, IDestination destination)
        {
            List<Delivery> quotes = new List<Delivery>();
            foreach (var carrier in _items)
            {
                // Check that carrier can deliver
                if (!carrier.CanDeliver(route, supplier, destination)) continue;

                // Quote cost of the item
                try
                {
                    var qDelivery = carrier.QuoteDelivery(route, supplier, destination);
                    if (qDelivery == null) continue;
                    quotes.Add(qDelivery);
                }
                catch (Exception e)
                {
                    reporter.Warn(string.Format("Cannot quote delivery for {0} with carrier {1}: {2}", route.Uri, carrier.Id, e.Message));
                }

            }
            return quotes.OrderBy(q => q.Cost);
        }

        /// <summary>
        /// Make a set of quotation for each supplier
        /// </summary>
        /// <returns></returns>
        public DeliveryQuotation QuoteDeliveryFromCarriers((ISupplier, IResource) supply, IDestination destination)
        {
            List<(ISupplier, DeliveryQuotation)> resourceSupplyQuotations = new List<(ISupplier, DeliveryQuotation)>();

            Dictionary<IRoute, IOrderedEnumerable<Delivery>> supplierQuotation = new Dictionary<IRoute, IOrderedEnumerable<Delivery>>();
            if (supply.Item2 is IAssetsContainer)
                supplierQuotation = GetAssetsDeliveryQuotations(supply.Item1, supply.Item2 as IAssetsContainer, destination);

            var resourceDeliveryQuotations = GetSingleDeliveryQuotations(supply.Item1, supply.Item2, destination);
            supplierQuotation.Add(supply.Item2, resourceDeliveryQuotations);

            return new DeliveryQuotation(supplierQuotation);
        }
    }
}