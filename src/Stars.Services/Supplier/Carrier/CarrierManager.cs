using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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
            return GetPlugins().Where(r => r.Value.CanDeliver(route, destination)).Select(r => r.Value);
        }

        public async Task<IDeliveryQuotation> GetAssetsDeliveryQuotationsAsync(IAssetsContainer assetsContainer, IDestination destination)
        {
            Dictionary<string, IOrderedEnumerable<IDelivery>> assetsQuotes = new Dictionary<string, IOrderedEnumerable<IDelivery>>();
            Dictionary<string, Exception> assetsExceptions = new Dictionary<string, Exception>();

            foreach (var asset in assetsContainer.Assets)
            {
                try
                {
                    var length = asset.Value.ContentLength;
                    await asset.Value.CacheHeaders();
                    length = asset.Value.ContentLength;
                    string relPath = null;
                    if (assetsContainer.Uri != null && assetsContainer.Uri.IsAbsoluteUri)
                    {
                        var relUri = assetsContainer.Uri.MakeRelativeUri(asset.Value.Uri);
                        // Use the relative path only if a sub-directory
                        if (!relUri.IsAbsoluteUri && !relUri.ToString().StartsWith(".."))
                            relPath = Path.GetDirectoryName(relUri.ToString());
                    }
                    if (asset.Value.ContentDisposition != null && !string.IsNullOrEmpty(asset.Value.ContentDisposition.FileName))
                    {
                        if ( asset.Value.ContentDisposition.FileName.Contains("/"))
                            relPath = "";
                    }
                    var assetsDeliveryQuotations = GetSingleDeliveryQuotations(asset.Value, destination.To(asset.Value, relPath));
                    assetsQuotes.Add(asset.Key, assetsDeliveryQuotations);
                }
                catch (WebException we)
                {
                    logger.LogWarning("Cannot quote delivery for {0}: {1}", asset.Value.Uri, we.Message);
                    if ( we.InnerException != null )
                        logger.LogWarning(we.InnerException.Message);
                    assetsExceptions.Add(asset.Key, we);
                }
                catch (Exception e)
                {
                    logger.LogWarning("Cannot quote delivery for {0}: {1}", asset.Value.Uri, e.Message);
                    logger.LogDebug(e.StackTrace);
                    assetsExceptions.Add(asset.Key, e);
                }
            }
            return new DeliveryQuotation(assetsQuotes, assetsExceptions);
        }

        public IOrderedEnumerable<IDelivery> GetSingleDeliveryQuotations(IResource route, IDestination destination)
        {
            List<IDelivery> quotes = new List<IDelivery>();

            foreach (var carrier in GetPlugins().Values)
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
    }
}