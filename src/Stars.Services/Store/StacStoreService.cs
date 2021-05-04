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
using System.Linq;
using Terradue.Stars.Services.Router;

namespace Terradue.Stars.Services.Store
{
    public class StacStoreService : IStarsService
    {
        private readonly ILogger<StacStoreService> logger;
        private readonly DestinationManager destinationManager;
        private readonly TranslatorManager translatorManager;
        private readonly CarrierManager carrierManager;
        protected readonly ICredentials credentials;
        private readonly StacRouter _stacRouter;
        private readonly StacStoreConfiguration storeOptions;
        private static IDestination rootCatalogDestination;
        private static StacCatalogNode rootCatalogNode;

        public IDestination RootCatalogDestination
        {
            get
            {
                if (rootCatalogDestination == null)
                    throw new DriveNotFoundException("Stac store not ready. Initialization required");
                return rootCatalogDestination;
            }
        }
        public StacCatalogNode RootCatalogNode
        {
            get
            {
                if (rootCatalogNode == null)
                    throw new DriveNotFoundException("Stac store not ready. Initialization required"); return rootCatalogNode;
            }
        }

        public string Id => "Store";

        public int Priority { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Key { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public StacStoreService(IOptions<StacStoreConfiguration> options,
                            ILogger<StacStoreService> logger,
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
            this.credentials = credentials;
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

        public async Task<StacNode> Load(Uri uri)
        {
            var resource = WebRoute.Create(uri, credentials: credentials);
            return (await _stacRouter.Route(resource)) as StacNode;
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
            rootCatalogDestination.PrepareDestination();
            await StoreResourceAtDestination(catalogNode, rootCatalogDestination);
            logger.LogInformation("Root catalog {0} created at {1}", storeOptions.RootCatalogue.Identifier, storeOptions.RootCatalogue.Uri);
            await LoadRootCatalogNode();

        }

        public async Task<StacItemNode> StoreItemNodeAtDestination(StacItemNode stacItemNode, IDestination destination)
        {
            PrepareStacItemForDestination(stacItemNode, destination);
            return await StoreResourceAtDestination(stacItemNode, destination) as StacItemNode;
        }

        public async Task<StacCatalogNode> StoreCatalogNodeAtDestination(StacCatalogNode stacCatalogNode, IDestination destination)
        {
            PrepareStacCatalogueForDestination(stacCatalogNode, destination);
            return await StoreResourceAtDestination(stacCatalogNode, destination) as StacCatalogNode;
        }

        public async Task<IResource> StoreResourceAtDestination(IResource resource, IDestination destination)
        {
            var stacDeliveries = carrierManager.GetSingleDeliveryQuotations(resource, destination);
            IResource deliveredResource = null;
            foreach (var delivery in stacDeliveries)
            {
                deliveredResource = await delivery.Carrier.Deliver(delivery, true);
                if (deliveredResource != null) break;
            }

            if (deliveredResource == null)
                throw new InvalidDataException(string.Format("No carrier could store resource from {0} to {1}", resource.Uri, destination));

            // the returned resource should use the front interface if defined
            if (rootCatalogNode != null)
            {
                Uri relativeCatalogUri = RootCatalogDestination.Uri.MakeRelativeUri(deliveredResource.Uri);
                var publicResource = WebRoute.Create(new Uri(RootCatalogNode.Uri, relativeCatalogUri));

                return publicResource;
            }
            else
            {
                return deliveredResource;
            }
        }


        private void PrepareStacNodeForDestination(StacNode stacNode, IDestination destination)
        {
            IStacObject stacObject = stacNode.StacObject;
            if (stacObject == null) return;

            MakeAllLinksRelative(stacObject, destination);

            foreach (var link in stacObject.Links.Where(l => l.RelationshipType == "self").ToList())
                stacObject.Links.Remove(link);
            if (!storeOptions.AllRelative)
                stacObject.Links.Add(StacLink.CreateSelfLink(MapToFrontUri(destination.Uri), stacNode.ContentType.ToString()));

            foreach (var link in stacObject.Links.Where(l => l.RelationshipType == "root").ToList())
                stacObject.Links.Remove(link);
            if (!storeOptions.AllRelative)
                stacObject.Links.Add(StacLink.CreateRootLink(RootCatalogNode.Uri, RootCatalogNode.ContentType.ToString()));

            RemoveDuplicateLinks(stacNode);
        }

        private void MakeAllLinksRelative(IStacObject stacObject, IDestination destination)
        {
            foreach (var link in stacObject.Links.ToArray())
            {
                if (link == null || !link.Uri.IsAbsoluteUri) continue;
                // 1. Check the link uri can be relative to destination itself
                var relativeUri = destination.Uri.MakeRelativeUri(link.Uri);
                if (!relativeUri.IsAbsoluteUri)
                {
                    if (relativeUri.ToString() == "") relativeUri = new Uri(Path.GetFileName(link.Uri.ToString()), UriKind.Relative);
                    link.Uri = relativeUri;
                    continue;
                }
                // 2. Check the link uri can be relative to root catalog
                relativeUri = RootCatalogDestination.Uri.MakeRelativeUri(link.Uri);
                if (relativeUri.IsAbsoluteUri)
                {
                    relativeUri = RootCatalogNode.Uri.MakeRelativeUri(link.Uri);
                }
                if (relativeUri.IsAbsoluteUri) continue;
                Uri absoluteUri = new Uri(RootCatalogNode.Uri, relativeUri);
                relativeUri = MapToFrontUri(destination.Uri).MakeRelativeUri(absoluteUri);

                if (!relativeUri.IsAbsoluteUri)
                {
                    if (relativeUri.ToString() == "") relativeUri = new Uri(Path.GetFileName(link.Uri.ToString()), UriKind.Relative);
                    link.Uri = relativeUri;
                    continue;
                }
            }
        }

        private void RemoveDuplicateLinks(StacNode stacNode)
        {
            IStacObject stacObject = stacNode.StacObject;
            if (stacObject == null) return;

            var links = stacObject.Links.GroupBy(link => link.Uri).Select(grp => grp.First()).ToList();
            stacObject.Links.Clear();
            foreach (var link in links)
                stacObject.Links.Add(link);
        }

        public Uri MapToFrontUri(Uri uri)
        {
            if (!uri.IsAbsoluteUri) throw new InvalidDataException("Destination URI must be absolute");

            // Check the link uri can be relative to the backend root catalog
            var relativeUri = RootCatalogDestination.Uri.MakeRelativeUri(uri);
            // If yes, then make an absolute uri from frontend
            if (relativeUri.IsAbsoluteUri)
            {
                relativeUri = RootCatalogNode.Uri.MakeRelativeUri(uri);
            }
            if (relativeUri.IsAbsoluteUri) return uri;
            return new Uri(RootCatalogNode.Uri, relativeUri);
        }

        private void PrepareStacCatalogueForDestination(StacCatalogNode stacCatalogNode, IDestination destination)
        {
            PrepareStacNodeForDestination(stacCatalogNode, destination);
        }

        public void PrepareStacItemForDestination(StacItemNode stacItemNode, IDestination destination)
        {
            PrepareStacNodeForDestination(stacItemNode, destination);
            StacItem stacItem = stacItemNode.StacObject as StacItem;
            if (stacItem == null) return;

            if (storeOptions.AbsoluteAssetsUrl)
                MakeAssetUriAbsolute(stacItemNode, destination);
            else
                MakeAssetUriRelative(stacItemNode, destination);
        }

        private void MakeAssetUriAbsolute(StacItemNode stacItemNode, IDestination destination)
        {
            MakeAssetUriRelative(stacItemNode, destination);
            foreach (var asset in stacItemNode.StacItem.Assets)
            {
                if (asset.Value.Uri.IsAbsoluteUri) continue;
                asset.Value.Uri = new Uri(MapToFrontUri(destination.Uri), asset.Value.Uri);
            }
        }

        private void MakeAssetUriRelative(StacItemNode stacItemNode, IDestination destination)
        {
            foreach (var asset in stacItemNode.StacItem.Assets)
            {
                if (!asset.Value.Uri.IsAbsoluteUri) continue;
                // 0. make sure the uri is not outside of the root catalog
                var relativeUri = RootCatalogDestination.Uri.MakeRelativeUri(asset.Value.Uri);
                if (relativeUri.IsAbsoluteUri || relativeUri.ToString().StartsWith("../"))
                    continue;
                // 1. Check the asset uri can be relative to destination itself
                relativeUri = destination.Uri.MakeRelativeUri(asset.Value.Uri);
                if (!relativeUri.IsAbsoluteUri)
                {
                    asset.Value.Uri = relativeUri;
                    continue;
                }
                // 1. Check the asset uri can be relative to root catalog
                relativeUri = RootCatalogDestination.Uri.MakeRelativeUri(asset.Value.Uri);
                if (relativeUri.IsAbsoluteUri) continue;
                Uri absoluteUri = new Uri(RootCatalogNode.Uri, relativeUri);
                relativeUri = stacItemNode.Uri.MakeRelativeUri(asset.Value.Uri);
                if (!relativeUri.IsAbsoluteUri)
                {
                    asset.Value.Uri = relativeUri;
                    continue;
                }
            }
        }

        public async Task<IEnumerable<StacAssetAsset>> GetAssetsInFolder(string relPath)
        {
            throw new NotImplementedException();
        }
    }
}