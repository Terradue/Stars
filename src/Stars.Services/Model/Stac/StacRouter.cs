using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stac;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Services.Plugins;
using Terradue.Stars.Services.Router;

namespace Terradue.Stars.Services.Model.Stac
{
    [PluginPriority(1)]
    public class StacRouter : IRouter
    {
        private readonly IResourceServiceProvider resourceServiceProvider;
        private readonly ILogger<StacRouter> logger;

        public StacRouter(IResourceServiceProvider resourceServiceProvider,
                          ILogger<StacRouter> logger)
        {
            this.resourceServiceProvider = resourceServiceProvider;
            this.logger = logger;
        }

        public int Priority { get; set; }
        public string Key { get => "Stac"; set { } }

        public string Label => "Stac Native Router";

        public bool CanRoute(IResource route)
        {
            var routeFound = AffineRouteAsync(route, CancellationToken.None).Result;
            if (routeFound is StacNode) return true;
            if (!(routeFound is IStreamResource)) return false;
            if ((routeFound.ContentType.MediaType.Contains("application/json")
                    || routeFound.ContentType.MediaType.Contains("application/geo+json")) || Path.GetExtension(routeFound.Uri.ToString()) == ".json")
            {
                try
                {
                    StacConvert.Deserialize<IStacObject>((routeFound as IStreamResource).ReadAsStringAsync(CancellationToken.None).Result);
                    return true;
                }
                catch (Exception e)
                {
                    logger?.LogDebug(e, "Cannot read STAC object from {0}", routeFound.Uri);
                }
            }
            return false;
        }

        public Task<IResource> RouteLinkAsync(IResource resource, IResourceLink childLink, CancellationToken ct)
        {
            if (!(resource is StacNode))
            {
                throw new Exception("Cannot route link from non-stac resource");
            }
            var link = resourceServiceProvider.ComposeLinkUri(childLink, resource);
            return RouteAsync(new GenericResource(link), ct);
        }

        private async Task<IResource> AffineRouteAsync(IResource route, CancellationToken ct)
        {
            if ((route.ContentType.MediaType.Contains("application/json")
                    || route.ContentType.MediaType.Contains("application/geo+json"))
                && route is IStreamResource)
            {
                return route;
            }
            IResource newRoute = await resourceServiceProvider.CreateStreamResourceAsync(new GenericResource(new Uri(route.Uri.ToString())), ct);
            // maybe the route is a folder
            if (string.IsNullOrEmpty(Path.GetExtension(route.Uri.ToString())))
            {
                try
                {
                    newRoute = await resourceServiceProvider.CreateStreamResourceAsync(new GenericResource(new Uri(route.Uri.ToString() + "/catalog.json")), ct);
                    if (newRoute.ContentType.MediaType.Contains("application/json"))
                    {
                        return newRoute;
                    }
                }
                catch
                {
                    return route;
                }
            }
            return newRoute;
        }

        public async Task<IResource> RouteAsync(IResource route, CancellationToken ct)
        {
            var routeFound = await AffineRouteAsync(route, ct);
            if (routeFound is StacCatalogNode)
                return routeFound as StacCatalogNode;
            if (routeFound is StacItemNode)
                return routeFound as StacItemNode;
            if (!(routeFound is IStreamResource))
                return null;
            if (routeFound.ContentType.MediaType.Contains("application/json") || routeFound.ContentType.MediaType.Contains("application/geo+json") || Path.GetExtension(routeFound.Uri.ToString()) == ".json")
            {
                IStacObject stacObject = StacConvert.Deserialize<IStacObject>(await (routeFound as IStreamResource).ReadAsStringAsync(ct));
                if (stacObject is IStacCatalog)
                    return new StacCatalogNode(stacObject as IStacCatalog, routeFound.Uri);
                else
                    return new StacItemNode(stacObject as StacItem, routeFound.Uri);
            }
            throw new NotSupportedException(routeFound.ContentType.ToString());
        }

    }
}
