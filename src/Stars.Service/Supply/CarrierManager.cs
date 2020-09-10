using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stars.Interface.Router;
using Stars.Interface.Supply;
using Stars.Interface.Supply.Destination;
using Stars.Service.Router;
using Stars.Interface.Supply.Asset;
using Stars.Service.Supply.Destination;

namespace Stars.Service.Supply
{
    public class CarrierManager : AbstractManager<ICarrier>
    {
        private readonly ILogger logger;
        private readonly IConfiguration configuration;

        public CarrierManager(ILogger logger, IConfiguration configuration, IServiceProvider serviceProvider) : base(logger, serviceProvider)
        {
            this.logger = logger;
            this.configuration = configuration;
        }

        internal IEnumerable<ICarrier> GetCarriers(IRoute route, ISupplier supplier, IDestination destination)
        {
            return Plugins.Where(r => r.CanDeliver(route, supplier, destination));
        }

        private Dictionary<IRoute, IOrderedEnumerable<IDelivery>> GetAssetsDeliveryQuotations(ISupplier supplier, IAssetsContainer assetsContainer, IDestination destination)
        {
            Dictionary<IRoute, IOrderedEnumerable<IDelivery>> routesQuotes = new Dictionary<IRoute, IOrderedEnumerable<IDelivery>>();

            foreach (var route in assetsContainer.GetAssets())
            {
                var assetsDeliveryQuotations = GetSingleDeliveryQuotations(supplier, route, destination);
                routesQuotes.Add(route, assetsDeliveryQuotations);
            }
            return routesQuotes;
        }

        private IOrderedEnumerable<IDelivery> GetSingleDeliveryQuotations(ISupplier supplier, IRoute route, IDestination destination)
        {
            List<IDelivery> quotes = new List<IDelivery>();
            foreach (var carrier in Plugins)
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
                    logger.LogWarning("Cannot quote delivery for {0} with carrier {1}: {2}", route.Uri, carrier.Id, e.Message);
                }

            }
            return quotes.OrderBy(q => q.Cost);
        }

        /// <summary>
        /// Make a set of quotation for each supplier
        /// </summary>
        /// <returns></returns>
        public IDeliveryQuotation QuoteDeliveryFromCarriers((ISupplier, INode) supply, IDestination destination)
        {
            List<(ISupplier, DeliveryQuotation)> resourceSupplyQuotations = new List<(ISupplier, DeliveryQuotation)>();

            Dictionary<IRoute, IOrderedEnumerable<IDelivery>> supplierQuotation = new Dictionary<IRoute, IOrderedEnumerable<IDelivery>>();
            if (supply.Item2 is IAssetsContainer)
                supplierQuotation = GetAssetsDeliveryQuotations(supply.Item1, supply.Item2 as IAssetsContainer, destination);

            var resourceDeliveryQuotations = GetSingleDeliveryQuotations(supply.Item1, supply.Item2, destination);
            if ( resourceDeliveryQuotations.Count() > 0 )
                supplierQuotation.Add(supply.Item2, resourceDeliveryQuotations);

            if ( supplierQuotation.Count > 0 )
                return new DeliveryQuotation(supplierQuotation);

            return null;
        }
    }
}