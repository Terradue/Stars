using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Services.Supplier.Destination;
using Stac.Catalog;
using Stac;
using System.Collections.Generic;
using Terradue.Stars.Services.Supplier.Carrier;
using Terradue.Stars.Interface;
using Terradue.Stars.Services.Translator;
using System.IO;
using Stac.Item;
using System.Net;

namespace Terradue.Stars.Services.Store
{
    public class StoreService : IStarsService
    {
        private readonly ILogger<StoreService> logger;
        private readonly DestinationManager destinationManager;
        private readonly TranslatorManager translatorManager;
        private readonly CarrierManager carrierManager;
        private readonly StacRouter _stacRouter;
        private readonly StoreOptions storeOptions;
        private IDestination rootCatalogDestination;
        private StacCatalogNode rootCatalogNode;

        public IDestination RootCatalogDestination { get => rootCatalogDestination; }
        public StacCatalogNode RootCatalogNode { get => rootCatalogNode; }

        public string Id => "Store";

        public int Priority { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Key { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public StoreService(IOptions<StoreOptions> options,
                            ILogger<StoreService> logger,
                            DestinationManager destinationManager,
                            TranslatorManager translatorManager,
                            CarrierManager carrierManager,
                            ICredentials credentials
                            )
        {
            this.storeOptions = options.Value;
            this.logger = logger;
            this.destinationManager = destinationManager;
            this.translatorManager = translatorManager;
            this.carrierManager = carrierManager;
            this._stacRouter = new StacRouter(credentials);
        }


        public async Task Init(bool @new = false)
        {
            if (@new)
                await InitRootCatalogNode();
            else
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

            await StoreNodeAtDestination(catalogNode, rootCatalogDestination);
            logger.LogInformation("Root catalog {0} created at {1}", storeOptions.RootCatalogue.Identifier, storeOptions.RootCatalogue.Uri);
            await LoadRootCatalogNode();

        }

        public async Task<StacNode> StoreNodeAtDestination(IResource node, IDictionary<string, IAsset> assets, IDestination destination, IEnumerable<StacNode> childrenNodes = null)
        {
            // Maybe the node is already a stac node
            StacNode stacNode = node as StacNode;
            if (stacNode == null)
            {
                // No? Let's try to translate it to Stac
                stacNode = await translatorManager.Translate<StacNode>(node);
                if (stacNode == null)
                    throw new InvalidDataException(string.Format("Impossible to translate node {0} into STAC.", node.Uri));
            }

            IDestination stacDestination = destination.To(stacNode);

            // Now we make the proper links between items and assets and store the node
            LinkStacNode(stacNode, assets, stacDestination, childrenNodes);

            return await StoreNodeAtDestination(stacNode, stacDestination);

        }

        public async Task<StacNode> StoreNodeAtDestination(StacNode stacNode, IDestination destination)
        {
            var stacDeliveries = carrierManager.GetSingleDeliveryQuotations(stacNode, destination);
            IResource deliveredResource = null;
            foreach (var delivery in stacDeliveries)
            {
                deliveredResource = await delivery.Carrier.Deliver(delivery);
                if (deliveredResource != null) break;
            }

            if (deliveredResource == null)
                throw new InvalidDataException(string.Format("No carrier could store node {0} at {1}", stacNode.Id, destination));

            return (await _stacRouter.Route(deliveredResource)) as StacNode;
        }

        private void LinkStacNode(StacNode stacNode, IDictionary<string, IAsset> assets, IDestination destination, IEnumerable<StacNode> childrenNodes)
        {
            LinkStacNode(stacNode, destination);
            if (stacNode.IsCatalog)
                LinkStacCatalog(stacNode as StacCatalogNode, childrenNodes, destination);
            else
                LinkStacItem(stacNode as StacItemNode, assets, destination);
        }

        private void LinkStacNode(StacNode stacNode, IDestination destination)
        {
            IStacObject stacObject = stacNode.StacObject;

            if (stacObject == null) return;

            stacObject.Links.Clear();
            stacObject.Links.Add(StacLink.CreateRootLink(destination.Uri.MakeRelativeUri(RootCatalogNode.Uri), RootCatalogNode.ContentType.ToString()));
            stacObject.Links.Add(StacLink.CreateSelfLink(new Uri(Path.GetFileName(destination.Uri.ToString()), UriKind.Relative), stacNode.ContentType.ToString()));

        }

        private void LinkStacItem(StacItemNode stacNode, IDictionary<string, IAsset> assets, IDestination destination)
        {
            StacItem stacItem = stacNode.StacObject as StacItem;
            if (stacItem == null || assets == null) return;

            stacItem.Assets.Clear();
            foreach (var assetKey in assets.Keys)
            {
                IAsset asset = assets[assetKey];
                var relativeUri = destination.Uri.MakeRelativeUri(asset.Uri);
                stacItem.Assets.Add(assetKey, CreateAsset(asset, relativeUri, stacItem));
            }
        }

        private void LinkStacCatalog(StacCatalogNode stacCatalogNode, IEnumerable<StacNode> childrenNodes, IDestination destination)
        {
            StacCatalog stacCatalog = stacCatalogNode.StacObject as StacCatalog;
            if (childrenNodes == null) return;
            foreach (var childNode in childrenNodes)
            {
                if (childNode == null) continue;
                var relativeUri = childNode.Uri.IsAbsoluteUri ? destination.Uri.MakeRelativeUri(childNode.Uri) : childNode.Uri;
                switch (childNode.ResourceType)
                {
                    case ResourceType.Catalog:
                    case ResourceType.Collection:
                        stacCatalog.Links.Add(StacLink.CreateChildLink(relativeUri, childNode.ContentType.ToString()));
                        break;
                    case ResourceType.Item:
                        stacCatalog.Links.Add(StacLink.CreateItemLink(relativeUri, childNode.ContentType.ToString()));
                        break;
                }
            }
        }

        public async Task AddChildrenToRootCatalog(IEnumerable<StacNode> stacNodes)
        {
            foreach (var stacNode in stacNodes)
            {
                if (stacNode.IsCatalog)
                    RootCatalogNode.StacCatalog.Links.Add(StacLink.CreateChildLink(RootCatalogDestination.Uri.MakeRelativeUri(stacNode.Uri), stacNode.ContentType.MediaType));
                else
                    RootCatalogNode.StacCatalog.Links.Add(StacLink.CreateItemLink(RootCatalogDestination.Uri.MakeRelativeUri(stacNode.Uri), stacNode.ContentType.MediaType));
            }
            await StoreNodeAtDestination(RootCatalogNode, RootCatalogDestination);
        }

        private StacAsset CreateAsset(IAsset asset, Uri relativeUri, IStacObject stacObject)
        {
            StacAssetAsset stacAssetAsset = asset as StacAssetAsset;
            StacAsset stacAsset = null;
            if (stacAssetAsset == null)
                stacAsset = new StacAsset(relativeUri, asset.Roles, asset.Label, asset.ContentType, asset.ContentLength);
            else
            {
                stacAsset = stacAssetAsset.StacAsset;
                stacAsset.Uri = relativeUri;
            }

            return stacAsset;
        }
    }
}