using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier.Destination;

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

        public async Task<IResource> ExecuteAsync(IResource route, IDestination destination)
        {
            IResource newRoute = route;
            foreach (var processing in processingManager.Plugins)
            {
                if (!processing.Value.CanProcess(newRoute, destination)) continue;
                // Create a new destination for each processing
                IDestination procDestination = destination.To(route, processing.Value.GetRelativePath(route, destination));
                newRoute = await processing.Value.Process(newRoute, procDestination);
            }
            return newRoute;
        }
    }
}