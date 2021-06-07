using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Services.Supplier.Destination;
using Stac;
using System.Collections.Generic;
using Terradue.Stars.Services.Supplier.Carrier;
using Terradue.Stars.Interface;
using Terradue.Stars.Services.Translator;
using System.IO;
using System.Net;
using System.Linq;
using Terradue.Stars.Services.Router;
using Terradue.Stars.Interface.Persistence;
using Terradue.Stars.Services.Exceptions;

namespace Terradue.Stars.Services.Persistence.FileStore
{
    public class FileStoreService : IPersistenceService
    {
        private readonly IOptions<FileStoreConfiguration> options;
        private readonly ILogger<FileStoreService> logger;
        private readonly TranslatorManager translatorManager;
        private readonly RoutersManager routersManager;
        private readonly FileStorePersistenceMapper persistenceMapper;
        private ICatalog rootCatalog;

        public ICatalog Root
        {
            get
            {
                if (rootCatalog == null)
                    throw new DriveNotFoundException("File store not ready. Initialization required"); return rootCatalog;
            }
        }

        public string Id => "FileStore";

        public FileStoreService(IOptions<FileStoreConfiguration> options,
                            ILogger<FileStoreService> logger,
                            TranslatorManager translatorManager,
                            RoutersManager routersManager,
                            FileStorePersistenceMapper persistenceMapper
                            )
        {
            this.options = options;
            this.logger = logger;
            this.translatorManager = translatorManager;
            this.routersManager = routersManager;
            this.persistenceMapper = persistenceMapper;
        }


        public async Task Init(bool @new = false)
        {
            if (!options.Value.RootCatalog.Url.StartsWith("file://"))
                throw new InvalidConfigurationException("RootCatalogUrl must start with 'file://'");
            if (@new)
                await InitRootCatalogFile();
            else
                await LoadOrInitRootCatalogFile();

        }

        private async Task<ICatalog> LoadOrInitRootCatalogFile()
        {
            logger.LogInformation("Loading Root Catalog {0} at {1}...", options.Value.RootCatalog.Identifier, options.Value.RootCatalog.Url);
            try
            {
                await LoadRootCatalogNode();
            }
            catch (Exception e)
            {
                logger.LogInformation("No Root Catalog at {0}: {1}", options.Value.RootCatalog.Url, e.Message);
                await InitRootCatalogFile();
            }
        }

        private async Task<ICatalog> LoadRootCatalogNode()
        {
            var rootRoute = WebRoute.Create(new Uri(options.Value.RootCatalog.Url));
            var router = routersManager.GetRouter(rootRoute);
            if (router == null)
                throw new InvalidOperationException("No router found for file at " + rootRoute.Uri);
            IResource resource = await router.Route(rootRoute);
            if (resource.ResourceType != ResourceType.Catalog)
                throw new InvalidDataException(string.Format("No catalog at {0}",
                                                             options.Value.RootCatalog.Url));
            ICatalog catalog = resource as ICatalog;
            if (!catalog.Id.Equals(options.Value.RootCatalog.Identifier))
                throw new KeyNotFoundException(string.Format("No catalog with ID {0} at {1}",
                                                             options.Value.RootCatalog.Identifier,
                                                             options.Value.RootCatalog.Url));
            return catalog;
        }

        private async Task<ICatalog> InitRootCatalogFile()
        {
            StacCatalog stacRootCatalog = new StacCatalog(options.Value.RootCatalog.Identifier, options.Value.RootCatalog.Description);
            ICatalog _rootCatalog = new StacCatalogNode(stacRootCatalog, new Uri(options.Value.RootCatalog.Url));
            logger.LogInformation("Trying to init a root catalog {0} on {1}",
                                  options.Value.RootCatalog.Identifier,
                                  options.Value.RootCatalog.Url);
            _rootCatalog = await Commit<ICatalog>(_rootCatalog);
            logger.LogInformation("Root catalog {0} created at {1}", _rootCatalog.Id, _rootCatalog.Uri);
            return await LoadRootCatalogNode();
        }

        public async Task<IResource> Commit<T>(T resource) where T : IResource
        {
            // 1. Make a transaction clone of the resource
            ITransactableResource newResource = resource.CloneForTransaction();

            // 2. Define its Uri (backend)
            Uri resourceUri = BuildFileStoreUri(resource);
            newResource.SetTransactionState(FileTransactionState.Create(resourceUri));

            // 3. Load existing resource
            IResource existingResource = await LoadResourceAsync(resourceUri);
            // Update resource
            if (existingResource != null)
                newResource.SetExistingResource(existingResource);

            // 3. Update links
            if (options.Value.AbsoluteUris)
                newResource.UpdateUris(BuildFrontEndUri(resource));

            // 4. Save File
            await StreamToFile(newResource);

            return newResource;

        }

        

        private async Task<IResource> LoadResourceAsync(Uri resourceUri)
        {
            WebRoute resourceRoute = null;
            try
            {
                resourceRoute = WebRoute.Create(resourceUri);
            }
            catch
            {
                return null;
            }
            var resourceRouter = routersManager.GetRouter(resourceRoute);
            if (resourceRouter == null)
                throw new InvalidOperationException("No router found for existing file at " + resourceRoute.Uri);
            return await resourceRouter.Route(resourceRoute);
        }

        private Uri BuildFileStoreUri(IResource resource)
        {
            Uri baseUri = new Uri(Path.GetDirectoryName(rootCatalog.Uri.ToString()));
            return new Uri(baseUri, persistenceMapper.GetPath(resource));
        }

        private object BuildFrontEndUri<T>(T resource) where T : IResource
        {
            Uri baseUri = new Uri(options.Value.RootCatalog.FrontEndUrl);
            return new Uri(baseUri, persistenceMapper.GetPath(resource));
        }

        private async Task StreamToFile(ITransactableResource resource)
        {
            FileInfo file = new FileInfo(resource.Uri.ToString().Replace("file://", ""));
            Stream stream = await resource.GetStreamAsync();
            using (FileStream fileStream = new FileStream(file.FullName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                await stream.CopyToAsync(fileStream, 131072);
                await fileStream.FlushAsync();
            }
        }

        public async Task<StacItemNode> StoreStacNodeAtDestination(StacNode stacNode, IDestination destination)
        {
            if (stacNode is StacItemNode)
                PrepareStacItemForDestination(stacNode as StacItemNode, destination);
            if (stacNode is StacCatalogNode)
                PrepareStacCatalogueForDestination(stacNode as StacCatalogNode, destination);
            return await _stacRouter.Route(await StoreResourceAtDestination(stacNode, destination)) as StacItemNode;
        }

        public async Task<StacItemNode> StoreItemNodeAtDestination(StacItemNode stacItemNode, IDestination destination)
        {
            PrepareStacItemForDestination(stacItemNode, destination);
            return await _stacRouter.Route(await StoreResourceAtDestination(stacItemNode, destination)) as StacItemNode;
        }

        public async Task<StacCatalogNode> StoreCatalogNodeAtDestination(StacCatalogNode stacCatalogNode, IDestination destination)
        {
            PrepareStacCatalogueForDestination(stacCatalogNode, destination);
            return await _stacRouter.Route(await StoreResourceAtDestination(stacCatalogNode, destination)) as StacCatalogNode;
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
            if (rootCatalog != null)
            {
                Uri relativeCatalogUri = RootCatalogDestination.Uri.MakeRelativeUri(deliveredResource.Uri);
                var publicResource = WebRoute.Create(new Uri(Root.Uri, relativeCatalogUri));

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
                stacObject.Links.Add(StacLink.CreateRootLink(Root.Uri, Root.ContentType.ToString()));

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
                    relativeUri = Root.Uri.MakeRelativeUri(link.Uri);
                }
                if (relativeUri.IsAbsoluteUri) continue;
                Uri absoluteUri = new Uri(Root.Uri, relativeUri);
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

            Uri frontUri = uri;

            // Check the link uri can be relative to the backend root catalog
            var relativeUri = RootCatalogDestination.Uri.MakeRelativeUri(uri);
            // If yes, then make an absolute uri from frontend
            if (relativeUri.IsAbsoluteUri)
            {
                relativeUri = Root.Uri.MakeRelativeUri(uri);
            }
            if (!relativeUri.IsAbsoluteUri)
                frontUri = new Uri(Root.Uri, relativeUri);

            return frontUri;
        }

        public Uri MapToBackendUri(Uri uri)
        {
            if (!uri.IsAbsoluteUri) throw new InvalidDataException("Destination URI must be absolute");

            // Check the link uri can be relative to the backend root catalog
            var relativeUri = Root.Uri.MakeRelativeUri(uri);
            // If yes, then make an absolute uri from frontend
            if (relativeUri.IsAbsoluteUri)
            {
                relativeUri = RootCatalogDestination.Uri.MakeRelativeUri(uri);
            }
            if (relativeUri.IsAbsoluteUri) return uri;
            return new Uri(RootCatalogDestination.Uri, relativeUri);
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
                Uri absoluteUri = new Uri(Root.Uri, relativeUri);
                relativeUri = stacItemNode.Uri.MakeRelativeUri(asset.Value.Uri);
                if (!relativeUri.IsAbsoluteUri)
                {
                    asset.Value.Uri = relativeUri;
                    continue;
                }
            }
        }

        public async Task<IEnumerable<IStreamable>> GetAssetsInFolder(string relPath)
        {
            var assetsFolder = WebRoute.Create(new Uri(RootCatalogDestination.Uri, relPath));
            return assetsFolder.ListFolder().Select(a => WebRoute.Create(MapToFrontUri(a.Uri)));
        }


    }
}