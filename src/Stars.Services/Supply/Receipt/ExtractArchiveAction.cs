using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Tar;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supply.Asset;
using Terradue.Stars.Interface.Supply.Destination;
using Terradue.Stars.Services.Router;
using Terradue.Stars.Services.Supply.Carrier;
using Terradue.Stars.Services.Supply.Destination;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Terradue.Stars.Services.Supply.Receipt
{
    public class ExtractArchiveAction : IReceiptAction
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

        public bool CanReceive(NodeInventory deliveryForm)
        {
            return deliveryForm.Assets != null && deliveryForm.Assets.Any(asset => IsArchive(asset.Value));
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

        public async Task<NodeInventory> Receive(NodeInventory nodeInventory)
        {
            Dictionary<string, IAsset> assetsExtracted = new Dictionary<string, IAsset>();
            foreach (var asset in nodeInventory.Assets)
            {
                if (!IsArchive(asset.Value)) continue;
                IDestination destination = nodeInventory.Destination;
                string subFolder = asset.Key;
                if ( nodeInventory.SupplierNode != null )
                    subFolder = nodeInventory.SupplierNode.Id + "." + subFolder;
                var newDestination = destination.To(subFolder);
                newDestination.Create();
                logger.LogInformation("Extracting asset {0}...", subFolder);
                var archiveAssets = await ExtractArchive(asset, newDestination);
                int i = 0;
                foreach (var archiveAsset in archiveAssets)
                {
                    assetsExtracted.Add(archiveAsset.Key, archiveAsset.Value);
                    i++;
                }
            }

            return new NodeInventory(nodeInventory.Node, assetsExtracted, nodeInventory.Destination);
        }

        private async Task<Dictionary<string, IAsset>> ExtractArchive(KeyValuePair<string, IAsset> asset, IDestination destination)
        {
            IDestination archiveFolder = CreateArchiveFolderDestination(asset.Value, destination);

            Archive archive = await Archive.Read(asset.Value);

            Dictionary<string, IAsset> assetsExtracted = new Dictionary<string, IAsset>();

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