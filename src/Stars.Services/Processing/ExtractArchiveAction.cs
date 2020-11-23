using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Tar;
using Terradue.Stars.Interface.Router;

using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Router;
using Terradue.Stars.Services.Supplier.Carrier;
using Terradue.Stars.Services.Supplier.Destination;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Terradue.Stars.Services.Supplier;
using Terradue.Stars.Interface.Processing;
using Stac;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Interface;
using Terradue.Stars.Services.Store;
using Microsoft.Extensions.Options;

namespace Terradue.Stars.Services.Processing
{
    public class ExtractArchiveAction : IProcessing
    {

        private readonly DestinationManager destinationManager;
        private readonly CarrierManager carrierManager;
        private readonly StacStoreService storeService;
        private readonly ILogger logger;
        private readonly IOptions<ExtractArchiveOptions> options;

        public int Priority { get; set; }
        public string Key { get; set; }


        public ExtractArchiveAction(IOptions<ExtractArchiveOptions> options, DestinationManager destinationManager, CarrierManager carrierManager, StacStoreService storeService, ILogger logger)
        {
            this.options = options;
            this.destinationManager = destinationManager;
            this.carrierManager = carrierManager;
            this.storeService = storeService;
            this.logger = logger;
            Key = "ExtractArchive";
            Priority = 1;
        }

        public bool CanProcess(IResource route, IDestination destination)
        {
            IAssetsContainer assetsContainer = route as IAssetsContainer;
            return assetsContainer != null && assetsContainer.GetAssets() != null && assetsContainer.GetAssets().Any(asset => IsArchive(asset.Value));
        }

        private bool IsArchive(IAsset asset)
        {
            return IsArchiveContentType(asset) || IsArchiveFileNameExtension(asset);
        }

        private bool IsArchiveFileNameExtension(IAsset asset)
        {
            return Archive.ArchiveFileExtensions.Keys.Contains(Path.GetExtension(asset.Uri.ToString()));
        }

        private bool IsArchiveContentType(IAsset asset)
        {
            return asset.ContentType != null && Archive.ArchiveContentTypes.Contains(asset.ContentType.MediaType);
        }

        public async Task<IResource> Process(IResource route, IDestination destination)
        {
            IItem item = route as IItem;
            if (item == null) return route;
            IAssetsContainer assetsContainer = route as IAssetsContainer;
            Dictionary<string, IAsset> newAssets = new Dictionary<string, IAsset>();
            foreach (var asset in assetsContainer.GetAssets())
            {
                if (!IsArchive(asset.Value))
                {
                    newAssets.Add(asset.Key, asset.Value);
                    continue;
                }
                logger.LogInformation("Extracting asset {0}...", asset.Value.Uri);
                Dictionary<string, GenericAsset> extractedAssets = await ExtractArchive(asset, destination);
                int i = 0;
                foreach (var extractedAsset in extractedAssets)
                {
                    newAssets.Add(extractedAsset.Key, extractedAsset.Value);
                    i++;
                }
                if (options.Value.KeepArchive)
                {
                    newAssets.Add(asset.Key, asset.Value);
                }
            }

            if (newAssets == null || newAssets.Count == 0) return route;

            return new ContainerNode(route as IItem, newAssets);

        }

        private async Task<Dictionary<string, GenericAsset>> ExtractArchive(KeyValuePair<string, IAsset> asset, IDestination destination)
        {
            Archive archive = await Archive.Read(asset.Value);

            Dictionary<string, GenericAsset> assetsExtracted = new Dictionary<string, GenericAsset>();
            string subFolder = archive.AutodetectSubfolder();

            foreach (var archiveAsset in archive.GetAssets())
            {
                var archiveAssetDestination = destination.To(archiveAsset.Value, subFolder);
                archiveAssetDestination.PrepareDestination();
                var assetDeliveries = carrierManager.GetSingleDeliveryQuotations(archiveAsset.Value, archiveAssetDestination);
                logger.LogDebug(archiveAsset.Key);
                foreach (var delivery in assetDeliveries)
                {
                    var assetExtracted = await delivery.Carrier.Deliver(delivery);
                    if (assetExtracted != null)
                    {
                        assetsExtracted.Add(asset.Key + "!" + archiveAsset.Key, new GenericAsset(assetExtracted, archiveAsset.Value.Label, archiveAsset.Value.Roles));
                        break;
                    }
                }
            }
            return assetsExtracted;
        }


        public void Configure(IConfigurationSection configurationSection, IServiceProvider serviceProvider)
        {

        }

        public string GetRelativePath(IResource route, IDestination destination)
        {
            return null;
        }
    }
}