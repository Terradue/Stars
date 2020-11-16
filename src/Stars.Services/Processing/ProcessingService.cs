using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Model.Stac;

namespace Terradue.Stars.Services.Processing
{
    public class ProcessingService : IStarsService
    {
        public ProcessingServiceParameters Parameters { get; set; }
        private readonly ILogger logger;
        private readonly ProcessingManager processingManager;

        public ProcessingService(ILogger logger, ProcessingManager processingManager)
        {
            this.logger = logger;
            this.processingManager = processingManager;
            Parameters = new ProcessingServiceParameters();
        }

        public async Task<StacNode> ExecuteAsync(StacNode node, IDestination destination)
        {
            StacNode newNode = node;
            foreach (var processing in processingManager.Plugins)
            {
                if (!processing.Value.CanProcess(newNode, destination)) continue;
                // Create a new destination for each processing
                IDestination procDestination = destination.To(node, processing.Value.GetRelativePath(node, destination));
                var processedResource = await processing.Value.Process(newNode, procDestination);
            }
            return newNode;
        }
    }
}