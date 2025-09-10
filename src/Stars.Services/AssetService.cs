// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: AssetService.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Stac;
using Stac.Extensions.File;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Supplier;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Services.Router;
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

        public async Task<AssetImportReport> ImportAssetsAsync(IAssetsContainer assetsContainer,
                                                               IDestination destination,
                                                               AssetFilters assetsFilters,
                                                               AssetChecks assetChecks,
                                                               CancellationToken ct)
        {
            // WARNING: PLEASE REVIEW ANY CHANGE TO THE FOLLOWING CODE WITH THE TEAM
            // There is a specific logic here to handle properly the assets delivery
            // with the follongin steps:
            // 1. Quote the assets delivery: this will return a list of possible deliveries by testing the assets access
            // 2. Check the delivery quotation: According to the asset quotation checks, some deliveries may be skipped with no error (no StopOnError)
            // 3. Deliver the assets: this will try to deliver the assets to the destination with the possible deliveries
            // 4. Check the delivery: According to the asset delivery checks, an error may be raised (StopOnError)

            FilteredAssetContainer filteredAssetContainer = new FilteredAssetContainer(assetsContainer, assetsFilters);



            IDeliveryQuotation deliveryQuotation = await QuoteAssetsDeliveryAsync(filteredAssetContainer, destination, assetsFilters, assetChecks, ct);
            AssetImportReport report = new AssetImportReport(deliveryQuotation, destination);

            logger.LogDebug("Delivery quotation for {0} assets ({1} exceptions)", deliveryQuotation.AssetsDeliveryQuotes.Count, deliveryQuotation.AssetsExceptions.Count);

            CheckDeliveryQuotation(deliveryQuotation);

            foreach (var assetDeliveries in deliveryQuotation.AssetsDeliveryQuotes)
            {
                if (assetDeliveries.Value.Count() == 0)
                {
                    string reason = "No possible delivery for asset";
                    if (deliveryQuotation.AssetsExceptions.ContainsKey(assetDeliveries.Key))
                    {
                        reason += " : " + deliveryQuotation.AssetsExceptions[assetDeliveries.Key].Message;
                        report.AssetsExceptions[assetDeliveries.Key] = new AssetImportException(reason);
                    }
                    else
                    {
                        report.AssetsExceptions.Add(assetDeliveries.Key, new AssetImportException(reason));
                    }
                    continue;
                }
                IResource importedResource = null;
                try
                {
                    importedResource = await ImportAsync(assetDeliveries.Key, assetDeliveries.Value, ct);
                    if (importedResource == null)
                        throw new AssetImportException("Imported asset is null");
                }
                catch (AggregateException ae)
                {
                    report.AssetsExceptions.Add(assetDeliveries.Key, ae);
                }
                // OK
                if (importedResource != null)
                {
                    IAsset importedAsset = MakeAsset(importedResource, assetsContainer.Assets[assetDeliveries.Key]);
                    report.ImportedAssets.Add(assetDeliveries.Key, importedAsset);
                }
            }

            return report;
        }

        private void CheckDeliveryQuotation(IDeliveryQuotation deliveryQuotation)
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
            if (assetOrigin is StacAssetAsset)
            {
                var clonedAsset = new StacAsset((assetOrigin as StacAssetAsset).StacAsset, null);
                clonedAsset.Uri = route.Uri;
                clonedAsset.FileExtension().Size = route.ContentLength > 0 ? route.ContentLength : clonedAsset.FileExtension().Size;
                clonedAsset.MediaType = assetOrigin.ContentType?.MediaType != "application/octet-stream" ? assetOrigin.ContentType : route.ContentType;
                return new StacAssetAsset(clonedAsset, null);
            }
            var genericAsset = new GenericAsset(route, assetOrigin.Title, assetOrigin.Roles);
            genericAsset.MergeProperties(assetOrigin.Properties);
            return genericAsset;
        }

        private async Task<IResource> ImportAsync(string key, IOrderedEnumerable<IDelivery> deliveries, CancellationToken ct)
        {
            logger.LogInformation("Starting delivery of assets for {0}", key);
            List<Exception> exceptions = new List<Exception>();
            foreach (var delivery in deliveries)
            {
                logger.LogInformation("Delivering asset {0} {1} {2} ({3}) to {4}...", key, delivery.Resource.ResourceType, delivery.Resource.Uri, delivery.Carrier.Id, delivery.Destination);
                try
                {
                    delivery.Destination.PrepareDestination();
                    IResource delivered = await delivery.Carrier.DeliverAsync(delivery, ct);

                    if (delivered != null)
                    {
                        logger.LogInformation("Delivery asset complete to {0} ({1})", delivered.Uri, Humanizer.ByteSizeExtensions.Humanize(Humanizer.ByteSizeExtensions.Bytes(delivered.ContentLength)));
                        return delivered;
                    }
                }
                catch (Exception e)
                {
                    logger.LogError("Error delivering asset {0} ({1}) : {2}", key, delivery.Carrier.Id, e.Message);
                    logger.LogDebug(e.StackTrace);
                    exceptions.Add(e);
                }
            }
            throw new AggregateException(exceptions);
        }

        public async Task<AssetDeleteReport> DeleteAssetsAsync(IAssetsContainer assetsContainer, AssetFilters assetsFilters, CancellationToken ct)
        {
            AssetDeleteReport assetDeleteReport = new AssetDeleteReport();

            logger.LogDebug("Deleting {0} assets of {1}", assetsContainer.Assets.Count(), assetsContainer.Uri.ToString());

            foreach (var asset in assetsContainer.Assets)
            {
                try
                {
                    await resourceServiceProvider.DeleteAsync(asset.Value, ct);
                }
                catch (Exception e)
                {
                    logger.LogWarning("Cannot delete asset {0}({2}) : {1}", asset.Key, e.Message, asset.Value.Uri);
                    assetDeleteReport.AssetsExceptions.Add(asset.Key, e);
                }
            }

            return assetDeleteReport;
        }

        private async Task<IDeliveryQuotation> QuoteAssetsDeliveryAsync(IAssetsContainer assetsContainer, IDestination destination, AssetFilters assetFilters, AssetChecks assetChecks, CancellationToken ct)
        {
            // Here we use a try/catch because some implementation of IAssetsContainer
            // perform an important work in the assets listing (e.g. DataHubResultItemRoutable)
            try
            {
                if (assetsContainer.Assets.Count() == 0)
                {
                    logger.LogDebug("No asset to quote. Applied filters: " + assetFilters.ToString());
                    return new DeliveryQuotation(new Dictionary<string, IOrderedEnumerable<IDelivery>>(),
                                                 new Dictionary<string, Exception>());
                }
            }
            catch (Exception e)
            {
                logger.LogWarning("Cannot quote delivery for assets container {0} : {1}", assetsContainer.Uri, e.Message);
                logger.LogDebug(e.StackTrace);
                return new DeliveryQuotation(new Dictionary<string, IOrderedEnumerable<IDelivery>>(),
                                             new Dictionary<string, Exception>(){
                                                    { assetsContainer.Uri.ToString(), e }
                                             });
            }

            logger.LogDebug("Quoting delivery for {0} assets to {1}", assetsContainer.Assets.Count(), destination);
            return await carrierManager.GetAssetsDeliveryQuotationsAsync(assetsContainer, destination, assetChecks, true, ct);
        }
    }
}
