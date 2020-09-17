using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stars.Interface;
using Stars.Interface.Router;
using Stars.Interface.Supply;
using Stars.Interface.Supply.Destination;
using Stars.Service.Model.Stac;
using Stars.Service.Router;
using Stars.Service.Router.Translator;
using Stars.Service.Supply;
using Stars.Service.Supply.Destination;

namespace Stars.Service.Catalog
{
    public class CatalogingTask : IStarsTask
    {
        public CatalogTaskParameters Parameters { get; set; }
        private readonly ILogger logger;
        private readonly TranslatorManager translatorManager;
        private readonly RoutersManager routersManager;

        public CatalogingTask(ILogger logger, TranslatorManager translatorManager, RoutersManager routersManager)
        {
            this.logger = logger;
            this.translatorManager = translatorManager;
            this.routersManager = routersManager;
            Parameters = new CatalogTaskParameters();
        }

        public async Task<StacNode> ExecuteAsync(IRoute parentRoute, IEnumerable<IRoute> childrenRoute)
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

            return stacNode;

        }

    }

}