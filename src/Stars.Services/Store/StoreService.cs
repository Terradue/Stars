using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Services.Supplier.Destination;
using Stac.Catalog;
using Stac;
using Terradue.Stars.Interface.Supplier;
using System.Collections.Generic;
using Terradue.Stars.Services.Supplier.Carrier;
using Microsoft.Extensions.Configuration;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Services.Store
{
    public class StoreService : IStarsService
    {
        private readonly ILogger<StoreService> logger;
        private readonly DestinationManager destinationManager;
        private readonly CarrierManager carrierManager;
        private readonly StoreOptions storeOptions;
        private IDestination rootCatalogDestination;
        private StacCatalogNode rootCatalogNode;

        public IDestination RootCatalogDestination { get => rootCatalogDestination; }
        public StacCatalogNode RootCatalogNode { get => rootCatalogNode; }

        public string Id => throw new NotImplementedException();

        public int Priority { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Key { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public StoreService(IOptions<StoreOptions> options, ILogger<StoreService> logger, DestinationManager destinationManager, CarrierManager carrierManager)
        {
            this.storeOptions = options.Value;
            this.logger = logger;
            this.destinationManager = destinationManager;
            this.carrierManager = carrierManager;
        }


        public async Task Init()
        {
            await LoadOrInitRootCatalogNode();
            rootCatalogDestination = await destinationManager.CreateDestination(storeOptions.RootCatalogue.DestinationUrl, RootCatalogNode);
            CheckSynchronization();
        }

        private void CheckSynchronization()
        {
        }

        private async Task LoadOrInitRootCatalogNode()
        {
            logger.LogInformation("Loading Root Catalog {0} at {1}", storeOptions.RootCatalogue.Identifier, storeOptions.RootCatalogue.Uri);
            try
            {
                await LoadRootCatalogNode();
            }
            catch (Exception e)
            {
                logger.LogInformation("No Root Catalog at {0}: {1}", storeOptions.RootCatalogue.Uri, e.Message);
                await InitRootCatalogNode();
            }
        }

        private async Task LoadRootCatalogNode()
        {
            IStacCatalog rootCatalog = await StacCatalog.LoadUri(storeOptions.RootCatalogue.Uri);
            if (!rootCatalog.Id.Equals(storeOptions.RootCatalogue.Identifier))
                throw new KeyNotFoundException(string.Format("No catalog with ID {0} at {1}", storeOptions.RootCatalogue.Identifier, storeOptions.RootCatalogue.Uri));
            rootCatalogNode = new StacCatalogNode(rootCatalog);
        }

        private async Task InitRootCatalogNode()
        {
            StacCatalog stacCatalog = new StacCatalog(storeOptions.RootCatalogue.Identifier, storeOptions.RootCatalogue.Description);
            StacCatalogNode catalogNode = new StacCatalogNode(stacCatalog);
            logger.LogInformation("Trying to init a root catalog {0} on {1}", storeOptions.RootCatalogue.Identifier, storeOptions.RootCatalogue.DestinationUri);
            rootCatalogDestination = await destinationManager.CreateDestination(storeOptions.RootCatalogue.DestinationUrl, catalogNode);

            await CreateNode(catalogNode, rootCatalogDestination);
            logger.LogInformation("Root catalog {0} created at {1}", storeOptions.RootCatalogue.Identifier, storeOptions.RootCatalogue.Uri);
            await LoadRootCatalogNode();

        }

        public async Task<IResource> CreateNode(IResource node, IDestination destination)
        {
            var deliveries = carrierManager.GetSingleDeliveryQuotations(node, destination);
            destination.PrepareDestination();

            IResource delivered = null;
            foreach (var delivery in deliveries)
            {
                delivered = await delivery.Carrier.Deliver(delivery);
                if (delivered != null) break;
            }
            if (delivered == null)
                throw new InvalidOperationException(string.Format("Cannot save node {0} to {1}.", node.Uri, destination));

            return delivered;
        }
    }
}