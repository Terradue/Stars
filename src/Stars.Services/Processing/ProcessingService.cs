using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Processing;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Services.Store;
using Terradue.Stars.Services.Translator;

namespace Terradue.Stars.Services.Processing
{
    public class ProcessingService : IStarsService
    {
        public ProcessingServiceParameters Parameters { get; set; }
        private readonly ILogger logger;
        private readonly ProcessingManager processingManager;
        private readonly TranslatorManager translatorManager;

        public ProcessingService(ILogger<ProcessingService> logger, ProcessingManager processingManager, TranslatorManager translatorManager)
        {
            this.logger = logger;
            this.processingManager = processingManager;
            this.translatorManager = translatorManager;
            Parameters = new ProcessingServiceParameters();
        }

        public async Task<StacNode> ExtractArchive(StacItemNode stacItemNode, IDestination destination, StacStoreService storeService)
        {
            StacNode newItemNode = stacItemNode;
            foreach (var processing in processingManager.GetProcessings(ProcessingType.ArchiveExtractor))
            {
                if (!processing.CanProcess(newItemNode, destination)) continue;
                // Create a new destination for each processing
                IDestination procDestination = destination.To(stacItemNode, processing.GetRelativePath(stacItemNode, destination));
                StacItemNode newStacItemNode = null;
                try
                {
                    var processedResource = await processing.Process(newItemNode, procDestination);
                    if (processedResource == null) continue;
                    newStacItemNode = processedResource as StacItemNode;

                    // Maybe the node is already a stac node
                    if (newStacItemNode == null)
                    {
                        // No? Let's try to translate it to Stac
                        newStacItemNode = await translatorManager.Translate<StacItemNode>(processedResource);
                        if (newStacItemNode == null)
                            throw new InvalidDataException(string.Format("Impossible to translate node {0} into STAC.", processedResource.Uri));
                    }
                }
                catch (Exception e)
                {
                    logger.LogWarning("Exception extracting archive assets in {0} : {1}", newItemNode.Uri, e.Message);
                    logger.LogDebug(e.StackTrace);
                    continue;
                }
                newItemNode = await storeService.StoreItemNodeAtDestination(newStacItemNode, destination);
                break;
            }
            return newItemNode;
        }

        public async Task<StacNode> ExtractMetadata(StacItemNode itemNode, IDestination destination, StacStoreService storeService)
        {
            StacNode newItemNode = itemNode;
            foreach (var processing in processingManager.GetProcessings(ProcessingType.MetadataExtractor))
            {
                try
                {
                    if (!processing.CanProcess(newItemNode, destination)) continue;
                }
                catch
                {
                    continue;
                }
                // Create a new destination for each processing
                IDestination procDestination = destination.To(itemNode, processing.GetRelativePath(itemNode, destination));
                var processedResource = await processing.Process(newItemNode, procDestination);
                StacItemNode stacItemNode = processedResource as StacItemNode;
                // Maybe the node is already a stac node
                if (stacItemNode == null)
                {
                    // No? Let's try to translate it to Stac
                    stacItemNode = await translatorManager.Translate<StacItemNode>(processedResource);
                    if (stacItemNode == null)
                        throw new InvalidDataException(string.Format("Impossible to translate node {0} into STAC.", processedResource.Uri));
                }
                if (Parameters.KeepOriginalAssets)
                {
                    stacItemNode.StacItem.MergeAssets(itemNode);
                }
                newItemNode = await storeService.StoreItemNodeAtDestination(stacItemNode, destination);
                break;
            }
            return newItemNode;
        }
    }
}