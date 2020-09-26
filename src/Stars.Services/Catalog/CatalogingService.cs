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
using Terradue.Stars.Interface.Supply;
using Terradue.Stars.Interface.Supply.Asset;
using Terradue.Stars.Interface.Supply.Destination;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Services.Router;
using Terradue.Stars.Services.Router.Translator;
using Terradue.Stars.Services.Supply;
using Terradue.Stars.Services.Supply.Carrier;
using Terradue.Stars.Services.Supply.Destination;

namespace Terradue.Stars.Services.Catalog
{
    public class CatalogingService : IStarsService
    {
        public CatalogTaskParameters Parameters { get; set; }
        private readonly ILogger logger;
        private readonly TranslatorManager translatorManager;
        private readonly RoutersManager routersManager;
        private readonly CarrierManager carrierManager;

        public CatalogingService(ILogger logger, TranslatorManager translatorManager, RoutersManager routersManager, CarrierManager carrierManager)
        {
            this.logger = logger;
            this.translatorManager = translatorManager;
            this.routersManager = routersManager;
            this.carrierManager = carrierManager;
            Parameters = new CatalogTaskParameters();
        }

        public async Task<IRoute> ExecuteAsync(IRoute parentRoute, IEnumerable<IRoute> childrenRoutes, IDictionary<string, IAsset> assets, IDestination destination, int depth)
        {
            INode parentNode = await parentRoute.GoToNode();
            StacNode stacNode = parentNode as StacNode;
            if (stacNode == null)
            {
                stacNode = await translatorManager.Translate<StacNode>(parentNode);
                if (stacNode == null)
                {
                    logger.LogDebug("Impossible to translate node as STAC. Trying other routers.");

                    foreach (var router in routersManager.Plugins)
                    {
                        if (!router.CanRoute(parentNode)) continue;
                        var routableNode = await router.Route(parentNode);
                        if (routableNode == null) continue;
                        stacNode = await translatorManager.Translate<StacNode>(routableNode);
                        if (stacNode != null) break;
                    }
                }
            }

            return await RelinkAndDeliverStacNode(stacNode, childrenRoutes, assets, destination, depth);
        }

        private async Task<IRoute> RelinkAndDeliverStacNode(StacNode stacNode, IEnumerable<IRoute> childrenRoutes, IDictionary<string, IAsset> assets, IDestination destination, int depth)
        {
            if (depth == 1) stacNode.IsRoot = true;
            var stacDeliveries = carrierManager.GetSingleDeliveryQuotations(null, stacNode, destination);

            IRoute stacRoute = null;

            foreach (var delivery in stacDeliveries)
            {
                if (stacNode.IsCatalog)
                {
                    await RelinkStacCatalog(stacNode, childrenRoutes, delivery, destination);
                }
                else
                    RelinkStacItem(stacNode, childrenRoutes, assets, delivery, destination);
                stacRoute = await delivery.Carrier.Deliver(delivery);
                if (stacRoute != null) break;
            }

            return stacRoute;

        }

        private async Task RelinkStacCatalog(StacNode stacNode, IEnumerable<IRoute> childrenRoutes, IDelivery delivery, IDestination destination)
        {
            StacCatalog stacCatalog = stacNode.StacObject as StacCatalog;
            if (stacCatalog == null) return;

            stacCatalog.Links.Clear();
            stacCatalog.Links.Add(StacLink.CreateSelfLink(destination.Uri.MakeRelativeUri(delivery.TargetUri)));

            foreach (var childRoute in childrenRoutes)
            {
                if (childRoute == null) continue;
                INode childNode = await childRoute.GoToNode();
                var relativeUri = delivery.TargetUri.MakeRelativeUri(childNode.Uri);
                logger.LogDebug("Link to {0}", relativeUri.ToString());
                switch (childRoute.ResourceType)
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



        private void RelinkStacItem(StacNode stacNode, IEnumerable<IRoute> childrenRoutes, IDictionary<string, IAsset> assets, IDelivery delivery, IDestination destination)
        {
            StacItem stacItem = stacNode.StacObject as StacItem;
            if (stacItem == null) return;

            stacItem.Links.Clear();
            stacItem.Links.Add(StacLink.CreateSelfLink(destination.Uri.MakeRelativeUri(delivery.TargetUri)));

            foreach (var assetKey in assets.Keys)
            {
                IAsset asset = assets[assetKey];
                var relativeUri = delivery.TargetUri.MakeRelativeUri(asset.Uri);
                logger.LogDebug("Asset [{0}] {1}", assetKey, relativeUri.ToString());
                stacItem.Assets.Add(assetKey, CreateAsset(asset, relativeUri, stacItem));
            }
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