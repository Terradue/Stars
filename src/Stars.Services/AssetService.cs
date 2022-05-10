using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Stac;
using Stac.Extensions.File;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Services.Router;
using Terradue.Stars.Services.Store;
using Terradue.Stars.Services.Supplier;
using Terradue.Stars.Services.Supplier.Carrier;
using Terradue.Stars.Services.Translator;

namespace Terradue.Stars.Services
{
    public class AssetService : IStarsService
    {
        protected readonly ILogger logger;
        private readonly RoutersManager routersManager;
        protected readonly SupplierManager suppliersManager;
        protected readonly TranslatorManager translatorManager;
        private readonly CarrierManager carrierManager;
        private readonly IResourceServiceProvider resourceServiceProvider;
        private readonly ICredentials credentials;

        public AssetService(ILogger<AssetService> logger,
                            RoutersManager routersManager,
                            SupplierManager suppliersManager,
                            TranslatorManager translatorManager,
                            CarrierManager carrierManager,
                            IResourceServiceProvider resourceServiceProvider,
                            ICredentials credentials)
        {
            this.logger = logger;
            this.routersManager = routersManager;
            this.suppliersManager = suppliersManager;
            this.translatorManager = translatorManager;
            this.carrierManager = carrierManager;
            this.resourceServiceProvider = resourceServiceProvider;
            this.credentials = credentials;
        }

        public async Task<AssetImportReport> ImportAssets(IAssetsContainer assetsContainer, IDestination destination, AssetFilters assetsFilters)
        {
            FilteredAssetContainer filteredAssetContainer = new FilteredAssetContainer(assetsContainer, assetsFilters);

            if (filteredAssetContainer.Assets.Count() == 0) {
                logger.LogDebug("No asset to import. Check filters: " + assetsFilters.ToString());
                return new AssetImportReport(null, destination);
            }

            logger.LogDebug("Importing {0} assets to {1}", filteredAssetContainer.Assets.Count(), destination);

            IDeliveryQuotation deliveryQuotation = await QuoteAssetsDeliveryAsync(filteredAssetContainer, destination, assetsFilters);
            AssetImportReport report = new AssetImportReport(deliveryQuotation, destination);

            logger.LogDebug("Delivery quotation for {0} assets ({1} exceptions)", deliveryQuotation.AssetsDeliveryQuotes.Count, deliveryQuotation.AssetsExceptions.Count);

            CheckDelivery(deliveryQuotation);

            foreach (var assetDeliveries in deliveryQuotation.AssetsDeliveryQuotes)
            {
                if (assetDeliveries.Value.Count() == 0)
                {
                    report.AssetsExceptions.Add(assetDeliveries.Key, new AssetImportException("No delivery possible"));
                    continue;
                }
                IResource importedResource = null;
                try
                {
                    importedResource = await Import(assetDeliveries.Key, assetDeliveries.Value);
                    if ( importedResource == null )
                        throw new AssetImportException("Imported asset is null");
                }
                catch (AggregateException ae)
                {
                    report.AssetsExceptions.Add(assetDeliveries.Key, ae);
                }
                // OK
                if (importedResource != null)
                {
                    IAsset importedAsset = MakeAsset(importedResource, (IAsset)assetDeliveries.Value.First().Resource);
                    report.ImportedAssets.Add(assetDeliveries.Key, importedAsset);
                }
            }

            return report;
        }

        private void CheckDelivery(IDeliveryQuotation deliveryQuotation)
        {
            foreach (var item in deliveryQuotation.AssetsDeliveryQuotes)
            {
                if (item.Value.Count() == 0)
                {
                    logger.LogDebug("No delivery for asset {0}", item.Key);
                }

                logger.LogDebug("Asset {0} : {1} possible deliveries", item.Key, item.Value.Count());
                int j = 1;
                foreach (var delivery in item.Value)
                {
                    logger.LogDebug("  #{0} {1}", j, delivery);
                    j++;
                }
            }
        }

        private IAsset MakeAsset(IResource route, IAsset assetOrigin)
        {
            if (route is IAsset) return route as IAsset;
            if (assetOrigin is StacAssetAsset){
                var clonedAsset = new StacAsset((assetOrigin as StacAssetAsset).StacAsset, null);
                clonedAsset.Uri = route.Uri;
                clonedAsset.FileExtension().Size = route.ContentLength > 0 ? route.ContentLength : clonedAsset.FileExtension().Size;
                clonedAsset.MediaType = assetOrigin.ContentType?.MediaType != "application/octet-stream" ? assetOrigin.ContentType : route.ContentType;
                return new StacAssetAsset(clonedAsset, null);
            }
            var genericAsset = new GenericAsset(route, assetOrigin.Title, assetOrigin.Roles);
            return genericAsset;
        }

        private async Task<IResource> Import(string key, IOrderedEnumerable<IDelivery> deliveries)
        {
            logger.LogInformation("Starting delivery of assets for {0}", key);
            List<Exception> exceptions = new List<Exception>();
            foreach (var delivery in deliveries)
            {
                logger.LogInformation("Delivering asset {0} {1} {2} ({3}) to {4}...", key, delivery.Resource.ResourceType, delivery.Resource.Uri, delivery.Carrier.Id, delivery.Destination);
                try
                {
                    delivery.Destination.PrepareDestination();
                    IResource delivered = await delivery.Carrier.Deliver(delivery);
                    if (delivered != null)
                    {
                        logger.LogInformation("Delivery asset complete to {0}", delivered.Uri);
                        return delivered;
                    }
                }
                catch (Exception e)
                {
                    logger.LogError("Error delivering asset {0} ({1}) : {2}", key, delivery.Carrier.Id, e.Message);
                    exceptions.Add(e);
                }
            }
            throw new AggregateException(exceptions);
        }

        public async Task<AssetDeleteReport> DeleteAssets(IAssetsContainer assetsContainer, AssetFilters assetsFilters)
        {
            AssetDeleteReport assetDeleteReport = new AssetDeleteReport();

            logger.LogDebug("Deleting {0} assets of {1}", assetsContainer.Assets.Count(), assetsContainer.Uri.ToString());

            foreach (var asset in assetsContainer.Assets)
            {
                try {
                    await resourceServiceProvider.Delete(asset.Value);
                }
                catch(Exception e)
                {
                    logger.LogWarning("Cannot delete asset {0}({2}) : {1}", asset.Key, e.Message, asset.Value.Uri);
                    assetDeleteReport.AssetsExceptions.Add(asset.Key, e);
                }
            }

            return assetDeleteReport;
        }

        private async Task<IDeliveryQuotation> QuoteAssetsDeliveryAsync(IAssetsContainer assetsContainer, IDestination destination, AssetFilters assetFilters)
        {
            return await carrierManager.GetAssetsDeliveryQuotationsAsync(assetsContainer, destination);
        }
    }
}