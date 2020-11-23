using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Terradue.Stars.Interface;
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
        private readonly StacStoreService storeService;
        private readonly TranslatorManager translatorManager;

        public ProcessingService(ILogger logger, ProcessingManager processingManager, StacStoreService storeService, TranslatorManager translatorManager)
        {
            this.logger = logger;
            this.processingManager = processingManager;
            this.storeService = storeService;
            this.translatorManager = translatorManager;
            Parameters = new ProcessingServiceParameters();
        }

        public async Task<StacNode> ExecuteAsync(StacNode node, IDestination destination)
        {
            StacNode newNode = node;
            foreach (var processing in processingManager.Plugins.Values)
            {
                if (!processing.CanProcess(newNode, destination)) continue;
                // Create a new destination for each processing
                IDestination procDestination = destination.To(node, processing.GetRelativePath(node, destination));
                var processedResource = await processing.Process(newNode, procDestination);
                IDictionary<string, IAsset> assets = null;
                if (processedResource is IItem)
                {
                    assets = (processedResource as IItem).GetAssets();
                }
                StacNode stacNode = processedResource as StacNode;
                // Maybe the node is already a stac node
                if (stacNode == null)
                {
                    // No? Let's try to translate it to Stac
                    stacNode = await translatorManager.Translate<StacNode>(processedResource);
                    if (stacNode == null)
                        throw new InvalidDataException(string.Format("Impossible to translate node {0} into STAC.", processedResource.Uri));
                }
                newNode = await storeService.StoreNodeAtDestination(stacNode, assets, destination, null);
            }
            return newNode;
        }
    }
}