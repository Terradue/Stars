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
using Medallion.Threading.FileSystem;
using Terradue.Stars.Interface.Model.Stac;

namespace Terradue.Stars.Services.Persistence.Stac.FileStore
{
    public class StacFileStoreService : IStacPersistenceService
    {
        private readonly IOptions<StacFileStoreConfiguration> options;
        private readonly ILogger<StacFileStoreService> logger;
        private readonly TranslatorManager translatorManager;
        private readonly StacRouter stacRouter;
        private readonly StacFileStorePersistenceMapper persistenceMapper;
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

        public StacFileStoreService(IOptions<StacFileStoreConfiguration> options,
                            ILogger<StacFileStoreService> logger,
                            TranslatorManager translatorManager,
                            StacRouter stacRouter,
                            StacFileStorePersistenceMapper persistenceMapper
                            )
        {
            this.options = options;
            this.logger = logger;
            this.translatorManager = translatorManager;
            this.stacRouter = stacRouter;
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
                return await LoadRootCatalogNode();
            }
            catch (Exception e)
            {
                logger.LogInformation("No Root Catalog at {0}: {1}", options.Value.RootCatalog.Url, e.Message);
                return await InitRootCatalogFile();
            }
        }

        private async Task<ICatalog> LoadRootCatalogNode()
        {
            var rootRoute = WebRoute.Create(new Uri(options.Value.RootCatalog.Url));
            IResource resource = await stacRouter.Route(rootRoute);
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
            StacCatalogNode _rootCatalog = new StacCatalogNode(stacRootCatalog, new Uri(options.Value.RootCatalog.Url));
            logger.LogInformation("Trying to init a root catalog {0} on {1}",
                                  options.Value.RootCatalog.Identifier,
                                  options.Value.RootCatalog.Url);
            _rootCatalog = await Commit<StacCatalogNode>(_rootCatalog);
            logger.LogInformation("Root catalog {0} created at {1}", _rootCatalog.Id, _rootCatalog.Uri);
            return await LoadRootCatalogNode();
        }

        public async Task<T> Commit<T>(ITransactableResource resource)
        {
            // 1. Make a transaction clone of the resource
            StacNode newResource = (StacNode)resource.Clone();

            // 2. Create the transaction
            StacTransaction stacTransaction = StacTransaction.Create(newResource, this);

            // 3. Define its FS destination Uri (backend)
            Uri stacNodeFileSystemUri = BuildFileStoreUri(resource);
            stacTransaction.SetDestinationUri(stacNodeFileSystemUri);

            // 4. START transaction with file locking
            var @lock = new FileDistributedLock(new FileInfo(stacNodeFileSystemUri.ToString().Replace("file://", "") + ".lock_"));
            await using (await @lock.AcquireAsync(options.Value.LockTimeout))
            {
                // 4.1. Load existing resource
                IResource existingResource = await LoadResourceAsync(stacNodeFileSystemUri);

                // 4.2. Update links
                UpdateLinks(newResource);

                // 4.3. Apply the "Before Transaction" method of the Transaction
                stacTransaction.BeforeWrite(existingResource);

                // 4.3. Save File
                await StreamToFile(newResource);


            }

            // 4.5. Apply the "After Commit" method of the Transaction
            stacTransaction.AfterCommit(newResource);

            return (T)newResource;

        }

        private void UpdateLinks(StacNode stacNode)
        {
            IStacObject stacObject = stacNode.StacObject;
            if (stacObject == null) return;

            // Self link
            foreach (var link in stacObject.Links.Where(l => l.RelationshipType == "self").ToList())
                stacObject.Links.Remove(link);
            stacObject.Links.Add(StacLink.CreateSelfLink(BuildFrontEndUri(stacNode), stacNode.ContentType.ToString()));

            // Root link
            foreach (var link in stacObject.Links.Where(l => l.RelationshipType == "root").ToList())
                stacObject.Links.Remove(link);
            stacObject.Links.Add(StacLink.CreateRootLink(new Uri(options.Value.RootCatalog.FrontEndUrl), Root.ContentType.ToString()));

            if (options.Value.RelativeReference)
            {
                MakeAllLinksRelative(stacNode);
                if (stacNode is IStacAssetsContainer)
                    MakeAssetUriRelative(stacNode as IStacAssetsContainer);
            }
            else
            {
                if (stacNode is IStacAssetsContainer)
                    MakeAssetUriAbsolute(stacNode as IStacAssetsContainer);
            }
            RemoveDuplicateLinks(stacNode);
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
            return await stacRouter.Route(resourceRoute);
        }

        private Uri BuildFileStoreUri(ITransactableResource resource)
        {
            Uri baseUri = new Uri(Path.GetDirectoryName(rootCatalog.Uri.ToString()));
            return new Uri(baseUri, persistenceMapper.GetPath(resource));
        }

        private Uri BuildFrontEndUri(ITransactableResource resource)
        {
            Uri baseUri = new Uri(options.Value.RootCatalog.FrontEndUrl);
            return new Uri(baseUri, persistenceMapper.GetPath(resource));
        }

        private async Task StreamToFile(ITransactableResource resource)
        {
            FileInfo file = new FileInfo(resource.Uri.ToString().Replace("file://", ""));
            file.Directory.Create();
            Stream stream = await resource.GetStreamAsync();
            using (FileStream fileStream = new FileStream(file.FullName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                await stream.CopyToAsync(fileStream, 131072);
                await fileStream.FlushAsync();
            }
        }

        private void MakeAllLinksRelative(StacNode stacNode)
        {
            foreach (var link in stacNode.StacObject.Links.ToArray())
            {
                if (link == null || !link.Uri.IsAbsoluteUri) continue;
                // 1. Check the link uri can be relative to destination itself
                var relativeUri = stacNode.Uri.MakeRelativeUri(link.Uri);
                if (!relativeUri.IsAbsoluteUri)
                {
                    if (relativeUri.ToString() == "") relativeUri = new Uri(Path.GetFileName(link.Uri.ToString()), UriKind.Relative);
                    link.Uri = relativeUri;
                    continue;
                }
                // 2. Check the link uri can be relative to root catalog
                relativeUri = Root.Uri.MakeRelativeUri(link.Uri);
                if (relativeUri.IsAbsoluteUri)
                {
                    relativeUri = Root.Uri.MakeRelativeUri(link.Uri);
                }
                if (relativeUri.IsAbsoluteUri) continue;
                Uri absoluteUri = new Uri(Root.Uri, relativeUri);
                relativeUri = BuildFrontEndUri(stacNode).MakeRelativeUri(absoluteUri);

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

        private void MakeAssetUriAbsolute(IStacAssetsContainer stacNode)
        {
            MakeAssetUriRelative(stacNode);
            foreach (var asset in stacNode.Assets)
            {
                if (asset.Value.Uri.IsAbsoluteUri) continue;
                asset.Value.Uri = new Uri(BuildFrontEndUri(stacNode), asset.Value.Uri);
            }
        }

        private void MakeAssetUriRelative(IStacAssetsContainer stacNode)
        {
            foreach (var asset in stacNode.Assets)
            {
                if (!asset.Value.Uri.IsAbsoluteUri) continue;
                // 0. make sure the uri is not outside of the root catalog
                var relativeUri = Root.Uri.MakeRelativeUri(asset.Value.Uri);
                if (relativeUri.IsAbsoluteUri || relativeUri.ToString().StartsWith("../"))
                    continue;
                // 1. Check the asset uri can be relative to destination itself
                relativeUri = stacNode.Uri.MakeRelativeUri(asset.Value.Uri);
                if (!relativeUri.IsAbsoluteUri)
                {
                    asset.Value.Uri = relativeUri;
                    continue;
                }
                // 1. Check the asset uri can be relative to root catalog
                relativeUri = Root.Uri.MakeRelativeUri(asset.Value.Uri);
                if (relativeUri.IsAbsoluteUri) continue;
                Uri absoluteUri = new Uri(Root.Uri, relativeUri);
                relativeUri = stacNode.Uri.MakeRelativeUri(asset.Value.Uri);
                if (!relativeUri.IsAbsoluteUri)
                {
                    asset.Value.Uri = relativeUri;
                    continue;
                }
            }
        }

    }
}