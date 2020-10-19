using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Router;

using Terradue.Stars.Services.Supplier.Destination;

namespace Terradue.Stars.Services.Supplier.Carrier
{
    public class CarrierManager : AbstractManager<ICarrier>
    {
        private readonly ILogger logger;

        public CarrierManager(ILogger logger, IServiceProvider serviceProvider) : base(logger, serviceProvider)
        {
            this.logger = logger;
        }

        internal IEnumerable<ICarrier> GetCarriers(IRoute route, ISupplier supplier, IDestination destination)
        {
            return Plugins.Where(r => r.Value.CanDeliver(route, supplier, destination)).Select(r => r.Value);
        }

        public Dictionary<string, IOrderedEnumerable<IDelivery>> GetAssetsDeliveryQuotations(ISupplier supplier, IAssetsContainer assetsContainer, IDestination destination)
        {
            Dictionary<string, IOrderedEnumerable<IDelivery>> assetsQuotes = new Dictionary<string, IOrderedEnumerable<IDelivery>>();

            foreach (var asset in assetsContainer.GetAssets())
            {
                var assetsDeliveryQuotations = GetSingleDeliveryQuotations(supplier, asset.Value, destination);
                assetsQuotes.Add(asset.Key, assetsDeliveryQuotations);
            }
            return assetsQuotes;
        }

        public IOrderedEnumerable<IDelivery> GetSingleDeliveryQuotations(ISupplier supplier, IRoute route, IDestination destination)
        {
            List<IDelivery> quotes = new List<IDelivery>();
            foreach (var carrier in Plugins.Values)
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
        public IDeliveryQuotation QuoteDeliveryFromCarriers((ISupplier, IRoute) supply, IDestination destination)
        {
            List<(ISupplier, DeliveryQuotation)> resourceSupplyQuotations = new List<(ISupplier, DeliveryQuotation)>();

            Dictionary<string, IOrderedEnumerable<IDelivery>> assetsDeliveryQuotation = new Dictionary<string, IOrderedEnumerable<IDelivery>>();
            if (supply.Item2 is IAssetsContainer)
            {
                assetsDeliveryQuotation = GetAssetsDeliveryQuotations(supply.Item1, supply.Item2 as IAssetsContainer, destination);
            }

            var resourceDeliveryQuotations = GetSingleDeliveryQuotations(supply.Item1, supply.Item2, destination);

            return new DeliveryQuotation(supply.Item1, (supply.Item2,resourceDeliveryQuotations), assetsDeliveryQuotation, destination);

        }
    }
}