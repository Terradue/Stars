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
using Stars.Interface;
using Stars.Interface.Router;
using Stars.Interface.Supply;
using Stars.Interface.Supply.Asset;
using Stars.Interface.Supply.Destination;
using Stars.Service.Model.Stac;
using Stars.Service.Router;
using Stars.Service.Router.Translator;
using Stars.Service.Supply;
using Stars.Service.Supply.Carrier;
using Stars.Service.Supply.Destination;

namespace Stars.Service.Catalog
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

        public async Task<IRoute> ExecuteAsync(IRoute parentRoute, IEnumerable<IRoute> childrenRoutes, IDictionary<string, IAsset> assets, IDestination destination)
        {
            INode parentNode = await parentRoute.GoToNode();
            if (parentNode is StacNode) return parentNode as StacNode;
            StacNode stacNode = await translatorManager.Translate<StacNode>(parentNode);
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

            return await RelinkAndDeliverStacNode(stacNode, childrenRoutes, assets, destination);
        }

        private async Task<IRoute> RelinkAndDeliverStacNode(StacNode stacNode, IEnumerable<IRoute> childrenRoutes, IDictionary<string, IAsset> assets, IDestination destination)
        {
            var stacDeliveries = carrierManager.GetSingleDeliveryQuotations(null, stacNode, destination);

            IRoute stacRoute = null;

            foreach (var delivery in stacDeliveries)
            {
                if (stacNode.IsCatalog)
                    await RelinkStacCatalog(stacNode, childrenRoutes, delivery);
                else
                    RelinkStacItem(stacNode, childrenRoutes, assets, delivery);
                stacRoute = await delivery.Carrier.Deliver(delivery);
                if (stacRoute != null) break;
            }

            return stacRoute;

        }

        private async Task RelinkStacCatalog(StacNode stacNode, IEnumerable<IRoute> childrenRoutes, IDelivery delivery)
        {
            StacCatalog stacCatalog = stacNode.StacObject as StacCatalog;
            if (stacCatalog == null) return;

            stacCatalog.Links.Clear();
            stacCatalog.Links.Add(StacLink.CreateSelfLink(delivery.TargetUri));

            foreach (var childRoute in childrenRoutes)
            {
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



        private void RelinkStacItem(StacNode stacNode, IEnumerable<IRoute> childrenRoutes, IDictionary<string, IAsset> assets, IDelivery delivery)
        {
            StacItem stacItem = stacNode.StacObject as StacItem;
            if (stacItem == null) return;

            stacItem.Links.Clear();
            stacItem.Links.Add(StacLink.CreateSelfLink(delivery.TargetUri));

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
            return new StacAsset(relativeUri, asset.Roles, asset.Label, asset.ContentType, asset.ContentLength);
        }
    }
}