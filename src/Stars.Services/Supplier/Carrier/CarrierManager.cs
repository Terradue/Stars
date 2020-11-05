using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier;
using Terradue.Stars.Interface.Supplier.Destination;

namespace Terradue.Stars.Services.Supplier.Carrier
{
    public class CarrierManager : AbstractManager<ICarrier>
    {
        private readonly ILogger logger;

        public CarrierManager(ILogger<CarrierManager> logger, IServiceProvider serviceProvider) : base(logger, serviceProvider)
        {
            this.logger = logger;
        }

        internal IEnumerable<ICarrier> GetCarriers(IResource route, IDestination destination)
        {
            return Plugins.Where(r => r.Value.CanDeliver(route, destination)).Select(r => r.Value);
        }

        public Dictionary<string, IOrderedEnumerable<IDelivery>> GetAssetsDeliveryQuotations(IAssetsContainer assetsContainer, IDestination destination)
        {
            Dictionary<string, IOrderedEnumerable<IDelivery>> assetsQuotes = new Dictionary<string, IOrderedEnumerable<IDelivery>>();

            foreach (var asset in assetsContainer.GetAssets())
            {
                try
                {
                    var assetsDeliveryQuotations = GetSingleDeliveryQuotations(asset.Value, destination.To(asset.Value));
                    assetsQuotes.Add(asset.Key, assetsDeliveryQuotations);
                }
                catch (Exception e)
                {
                    logger.LogDebug("Cannot quote delivery for {0}: {1}", asset.Value.Uri, e.Message);
                }
            }
            return assetsQuotes;
        }

        public IOrderedEnumerable<IDelivery> GetSingleDeliveryQuotations(IResource route, IDestination destination)
        {
            List<IDelivery> quotes = new List<IDelivery>();
            foreach (var carrier in Plugins.Values)
            {
                // Check that carrier can deliver
                if (!carrier.CanDeliver(route, destination)) continue;

                // Quote cost of the item
                try
                {
                    var qDelivery = carrier.QuoteDelivery(route, destination);
                    if (qDelivery == null) continue;
                    quotes.Add(qDelivery);
                }
                catch (Exception e)
                {
                    logger.LogDebug("Cannot quote delivery for {0} with carrier {1}: {2}", route.Uri, carrier.Id, e.Message);
                }

            }
            return quotes.OrderBy(q => q.Cost);
        }

        /// <summary>
        /// Make a set of quotation for each supplier
        /// </summary>
        /// <returns></returns>
        public IDeliveryQuotation QuoteDeliveryFromCarriers(IResource supply, IDestination destination)
        {
            List<(ISupplier, DeliveryQuotation)> resourceSupplyQuotations = new List<(ISupplier, DeliveryQuotation)>();

            Dictionary<string, IOrderedEnumerable<IDelivery>> assetsDeliveryQuotation = new Dictionary<string, IOrderedEnumerable<IDelivery>>();
            if (supply is IAssetsContainer)
            {
                assetsDeliveryQuotation = GetAssetsDeliveryQuotations(supply as IAssetsContainer, destination);
            }

            var resourceDeliveryQuotations = GetSingleDeliveryQuotations(supply, destination);

            return new DeliveryQuotation((supply, resourceDeliveryQuotations), assetsDeliveryQuotation, destination);

        }
    }
}