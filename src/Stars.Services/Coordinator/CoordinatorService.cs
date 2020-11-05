using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stac;
using Stac.Catalog;
using Stac.Item;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier;

using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Services.Router;
using Terradue.Stars.Services.Translator;
using Terradue.Stars.Services.Processing;
using Terradue.Stars.Services.Supplier.Carrier;
using Terradue.Stars.Services.Supplier.Destination;

namespace Terradue.Stars.Services.Catalog
{
    public class CoordinatorService : IStarsService
    {
        public CoordinatorServiceParameters Parameters { get; set; }
        private readonly ILogger logger;
        private readonly TranslatorManager translatorManager;
        private readonly RoutersManager routersManager;
        private readonly CarrierManager carrierManager;

        public CoordinatorService(ILogger logger, TranslatorManager translatorManager, RoutersManager routersManager, CarrierManager carrierManager)
        {
            this.logger = logger;
            this.translatorManager = translatorManager;
            this.routersManager = routersManager;
            this.carrierManager = carrierManager;
            Parameters = new CoordinatorServiceParameters();
        }

        public async Task<IResource> ExecuteAsync(IResource route, IEnumerable<IResource> childrenRoutes, IDestination destination, int depth)
        {
            StacNode stacNode = route as StacNode;
            if (stacNode == null)
            {
                stacNode = await translatorManager.Translate<StacNode>(route);
                if (stacNode == null)
                {
                    logger.LogDebug("Impossible to translate node as STAC. Trying other routers.");

                    foreach (var router in routersManager.Plugins)
                    {
                        if (!router.Value.CanRoute(route)) continue;
                        var routableNode = await router.Value.Route(route);
                        if (routableNode == null) continue;
                        stacNode = await translatorManager.Translate<StacNode>(routableNode);
                        if (stacNode != null) break;
                    }
                }
            }

            var deliveredStacNode = await RelinkAndDeliverStacNode(stacNode, childrenRoutes, destination.To(stacNode), depth);

            if (depth == 1 && !stacNode.IsCatalog)
            {
                childrenRoutes = new List<IResource>() { deliveredStacNode };
                stacNode = CreateStacCatalogNode(stacNode.StacObject as StacItem, "root", new Uri("catalog.json", UriKind.Relative));
                deliveredStacNode = await RelinkAndDeliverStacNode(stacNode, childrenRoutes, destination.To(stacNode), depth);
            }

            return deliveredStacNode;
        }

        private StacCatalogNode CreateStacCatalogNode(StacItem stacItem, string id, Uri uri)
        {
            StacCatalog catalog = new StacCatalog(id, null);
            catalog.Links.Add(StacLink.CreateItemLink(stacItem.Uri, stacItem.ToString()));
            catalog.Uri = uri;
            return new StacCatalogNode(catalog);
        }

        private async Task<IResource> RelinkAndDeliverStacNode(StacNode stacNode, IEnumerable<IResource> childrenRoutes, IDestination destination, int depth)
        {
            if (depth == 1 && stacNode.IsCatalog) stacNode.IsRoot = true;
            var stacDeliveries = carrierManager.GetSingleDeliveryQuotations(stacNode, destination);

            IResource stacRoute = null;

            foreach (var delivery in stacDeliveries)
            {
                if (stacNode.IsCatalog)
                {
                    RelinkStacCatalog(stacNode, childrenRoutes, delivery, destination);
                }
                else
                    RelinkStacItem(stacNode, childrenRoutes, delivery, destination);
                stacRoute = await delivery.Carrier.Deliver(delivery);
                if (stacRoute != null) break;
            }

            return stacRoute;

        }

        private void RelinkStacCatalog(StacNode stacNode, IEnumerable<IResource> childrenRoutes, IDelivery delivery, IDestination destination)
        {
            StacCatalog stacCatalog = stacNode.StacObject as StacCatalog;
            if (stacCatalog == null) return;

            stacCatalog.Links.Clear();
            if (stacNode.IsRoot)
                stacCatalog.Links.Add(StacLink.CreateRootLink(destination.Uri.MakeRelativeUri(delivery.Destination.Uri), "application/json"));
            else
                stacCatalog.Links.Add(StacLink.CreateSelfLink(destination.Uri.MakeRelativeUri(delivery.Destination.Uri), stacNode.ContentType.ToString()));

            foreach (var childRoute in childrenRoutes)
            {
                if (childRoute == null) continue;
                var relativeUri = childRoute.Uri.IsAbsoluteUri ? delivery.Destination.Uri.MakeRelativeUri(childRoute.Uri) : childRoute.Uri;
                logger.LogDebug("Link to {0}", relativeUri.ToString());
                switch (childRoute.ResourceType)
                {
                    case ResourceType.Catalog:
                    case ResourceType.Collection:
                        stacCatalog.Links.Add(StacLink.CreateChildLink(relativeUri, childRoute.ContentType.ToString()));
                        break;
                    case ResourceType.Item:
                        stacCatalog.Links.Add(StacLink.CreateItemLink(relativeUri, childRoute.ContentType.ToString()));
                        break;
                }
            }
        }

        private void RelinkStacItem(StacNode stacNode, IEnumerable<IResource> childrenRoutes, IDelivery delivery, IDestination destination)
        {
            StacItem stacItem = stacNode.StacObject as StacItem;
            if (stacItem == null) return;

            stacItem.Links.Clear();
            stacItem.Links.Add(StacLink.CreateSelfLink(destination.Uri.MakeRelativeUri(delivery.Destination.Uri)));

            // foreach (var assetKey in assets.Keys)
            // {
            //     IAsset asset = assets[assetKey];
            //     var relativeUri = delivery.TargetUri.MakeRelativeUri(asset.Uri);
            //     logger.LogDebug("Asset [{0}] {1}", assetKey, relativeUri.ToString());
            //     stacItem.Assets.Add(assetKey, CreateAsset(asset, relativeUri, stacItem));
            // }
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