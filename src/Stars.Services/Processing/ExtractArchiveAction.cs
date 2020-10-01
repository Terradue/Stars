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

namespace Terradue.Stars.Services.Processing
{
    public class ExtractArchiveAction : IProcessing
    {

        private readonly DestinationManager destinationManager;
        private readonly CarrierManager carrierManager;
        private readonly ILogger logger;

        public ExtractArchiveAction(DestinationManager destinationManager, CarrierManager carrierManager, ILogger logger)
        {
            this.destinationManager = destinationManager;
            this.carrierManager = carrierManager;
            this.logger = logger;
        }

        public bool CanProcess(IRoute route)
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

        public async Task<IRoute> Process(IRoute route)
        {
            IItem item = route as IItem;
            if (item == null) return route;
            IAssetsContainer assetsContainer = route as IAssetsContainer;
            Dictionary<string, IAsset> newAssets = new Dictionary<string, IAsset>();
            string dest = Path.GetDirectoryName(route.Uri.ToString());
            IDestination destination = await destinationManager.CreateDestination(dest);
            destination = destination.To(item.Id);
            destination.Create();
            foreach (var asset in assetsContainer.GetAssets())
            {
                if (!IsArchive(asset.Value))
                {
                    newAssets.Add(asset.Key, asset.Value);
                    continue;
                }
                string subFolder = asset.Key;
                var newDestination = destination.To(subFolder);
                newDestination.Create();
                logger.LogInformation("Extracting asset {0}...", subFolder);
                Dictionary<string, GenericAsset> extractedAssets = await ExtractArchive(asset, newDestination);
                int i = 0;
                foreach (var extractedAsset in extractedAssets)
                {
                    extractedAsset.Value.SetUriRelativeTo(item.Uri);
                    newAssets.Add(extractedAsset.Key, extractedAsset.Value);
                    i++;
                }
            }

            return new ContainerNode(route as IItem, newAssets);
        }

        private async Task<Dictionary<string, GenericAsset>> ExtractArchive(KeyValuePair<string, IAsset> asset, IDestination destination)
        {
            IDestination archiveFolder = CreateArchiveFolderDestination(asset.Value, destination);

            Archive archive = await Archive.Read(asset.Value);

            Dictionary<string, GenericAsset> assetsExtracted = new Dictionary<string, GenericAsset>();

            foreach (var archiveAsset in archive.GetAssets())
            {
                var archiveAssetDestination = archiveFolder.To(archiveAsset.Value);
                archiveAssetDestination.Create();
                var assetDeliveries = carrierManager.GetSingleDeliveryQuotations(null, archiveAsset.Value, archiveAssetDestination);
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


        private IDestination CreateArchiveFolderDestination(IAsset archiveAsset, IDestination destination)
        {
            FolderRoute folderRoute = new FolderRoute(archiveAsset);
            return destination.To(folderRoute);
        }

        public void Configure(IConfigurationSection configurationSection, IServiceProvider serviceProvider)
        {

        }
    }
}