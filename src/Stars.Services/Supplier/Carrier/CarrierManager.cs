using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Stac;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Supplier;
using Terradue.Stars.Interface.Supplier.Destination;

namespace Terradue.Stars.Services.Supplier.Carrier
{
    public class CarrierManager : AbstractManager<ICarrier>
    {
        private readonly ILogger logger;
        private readonly IResourceServiceProvider resourceServiceProvider;

        public CarrierManager(ILogger<CarrierManager> logger,
                              IServiceProvider serviceProvider,
                              IResourceServiceProvider resourceServiceProvider) : base(logger, serviceProvider)
        {
            this.resourceServiceProvider = resourceServiceProvider;
            this.logger = logger;
        }

        internal IEnumerable<ICarrier> GetCarriers(IResource route, IDestination destination)
        {
            return GetPlugins().Where(r => r.Value.CanDeliver(route, destination)).Select(r => r.Value);
        }

        public async Task<IDeliveryQuotation> GetAssetsDeliveryQuotationsAsync(IAssetsContainer assetsContainer,
                                                                               IDestination destination,
                                                                               AssetChecks assetChecks,
                                                                               bool includeAlternates = true,
                                                                               CancellationToken ct = default(CancellationToken))
        {
            Dictionary<string, IOrderedEnumerable<IDelivery>> assetsQuotes = new Dictionary<string, IOrderedEnumerable<IDelivery>>();
            Dictionary<string, Exception> assetsExceptions = new Dictionary<string, Exception>();

            foreach (var asset in assetsContainer.Assets)
            {
                List<IDelivery> assetsDeliveryQuotations = new List<IDelivery>();
                IList<IAsset> possibleAssets = new List<IAsset>();
                possibleAssets.Add(asset.Value);
                // Add the alternates
                if (includeAlternates && asset.Value.Alternates != null)
                {
                    possibleAssets.AddRange(asset.Value.Alternates);
                }
                foreach (var possibleAsset in possibleAssets)
                {
                    string relPath = null;
                    if (assetsContainer.Uri != null && assetsContainer.Uri.IsAbsoluteUri)
                    {
                        try
                        {
                            logger.LogDebug("Quoting delivery for asset {0} with url {1}", asset.Key, possibleAsset.Uri);
                            var possibleAssetStreamResource = await resourceServiceProvider.GetStreamResourceAsync(possibleAsset, ct);
                            var length = possibleAssetStreamResource.ContentLength > 0 ? possibleAssetStreamResource.ContentLength : possibleAsset.ContentLength;
                            if (assetsContainer.Uri != null && assetsContainer.Uri.IsAbsoluteUri)
                            {
                                var relUri = assetsContainer.Uri.MakeRelativeUri(possibleAsset.Uri);
                                // Use the relative path only if a sub-directory
                                if (!relUri.IsAbsoluteUri && !relUri.ToString().StartsWith(".."))
                                    relPath = Path.GetDirectoryName(relUri.ToString());
                            }
                            IResource assetForDestination = null;
                            // If the remote asset contains a content disposition, use it as the filename
                            if (possibleAssetStreamResource.ContentDisposition != null && !string.IsNullOrEmpty(possibleAssetStreamResource.ContentDisposition.FileName))
                            {
                                assetForDestination = possibleAssetStreamResource;
                                if (possibleAssetStreamResource.ContentDisposition.FileName.Contains("/"))
                                    relPath = "";
                            }
                            // If the asset contains a content disposition, use it as the filename
                            if (assetForDestination == null && possibleAsset.ContentDisposition != null && !string.IsNullOrEmpty(possibleAsset.ContentDisposition.FileName))
                            {
                                assetForDestination = possibleAsset;
                                if (possibleAsset.ContentDisposition.FileName.Contains("/"))
                                    relPath = "";
                            }
                            IEnumerable<IDelivery> deliveries = GetSingleDeliveryQuotations(possibleAssetStreamResource, destination.To(assetForDestination, relPath));
                            deliveries = await FilterDeliveriesByAssetChecksAsync(deliveries, possibleAsset, assetChecks);
                            assetsDeliveryQuotations.AddRange(deliveries);
                        }
                        catch (Exception e)
                        {
                            logger.LogWarning("Cannot quote delivery for asset {0} with url {2} : {1}", asset.Key, e.Message, possibleAsset.Uri);
                            logger.LogDebug(e.StackTrace);
                            assetsExceptions.Add(asset.Key, e);
                        }
                    }
                }
                assetsQuotes.Add(asset.Key, assetsDeliveryQuotations.OrderBy(d => d.Cost));
            }
            return new DeliveryQuotation(assetsQuotes, assetsExceptions);
        }

        private async Task<IEnumerable<IDelivery>> FilterDeliveriesByAssetChecksAsync(IEnumerable<IDelivery> deliveries, IAsset reference, AssetChecks assetChecks)
        {
            List<IDelivery> filteredDeliveries = new List<IDelivery>(deliveries);
            if (assetChecks != null)
            {
                foreach (var delivery in deliveries)
                {
                    foreach (var check in assetChecks)
                    {
                        try
                        {
                            await check.Check(reference, delivery.Resource);
                        }
                        catch (Exception e)
                        {
                            logger.LogWarning("Excluding delivery option {0}: {1}", delivery, e.Message);
                            filteredDeliveries.Remove(delivery);
                        }
                    }
                }
            }
            return filteredDeliveries;
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
