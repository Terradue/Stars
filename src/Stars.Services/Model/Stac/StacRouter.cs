using System;
using System.IO;
using System.Net;
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
        private ICredentials credentials;
        private readonly ILogger logger;

        public StacRouter(ICredentials credentials, ILogger logger = null)
        {
            this.credentials = credentials;
            this.logger = logger;
        }

        public int Priority { get; set; }
        public string Key { get => "Stac"; set { } }

        public string Label => "Stac Native Router";

        public ICredentials Credentials => credentials;

        public bool CanRoute(IResource route)
        {
            var routeFound = AffineRoute(route);
            if (routeFound is StacNode) return true;
            if (!(routeFound is IStreamable)) return false;
            if (routeFound.ContentType.MediaType.Contains("application/json") || Path.GetExtension(routeFound.Uri.ToString()) == ".json")
            {
                try
                {
                    StacConvert.Deserialize<IStacObject>((routeFound as IStreamable).ReadAsString().Result);
                    return true;
                }
                catch (Exception e)
                {
                    logger?.LogDebug(e, "Cannot read STAC object from {0}", routeFound.Uri);
                }
            }
            return false;
        }

        private IResource AffineRoute(IResource route)
        {
            try
            {
                if (route.ContentType.MediaType.Contains("application/json"))
                {
                    return route;
                }
            }
            catch (Exception)
            {
                // maybe the route is a folder
                if (string.IsNullOrEmpty(Path.GetExtension(route.Uri.ToString())) && route is WebRoute)
                {
                    try
                    {
                        WebRoute newRoute = WebRoute.Create(new Uri(route.Uri.ToString() + "/catalog.json"));
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
            }
            return route;
        }

        public async Task<IResource> Route(IResource route)
        {
            var routeFound = AffineRoute(route);
            if (routeFound is StacCatalogNode)
                return routeFound as StacCatalogNode;
            if (routeFound is StacItemNode)
                return routeFound as StacItemNode;
            if (!(routeFound is IStreamable)) return null;
            if (routeFound.ContentType.MediaType.Contains("application/json") || Path.GetExtension(routeFound.Uri.ToString()) == ".json")
            {
                IStacObject stacObject = StacConvert.Deserialize<IStacObject>(await (routeFound as IStreamable).ReadAsString());
                if (stacObject is IStacCatalog)
                    return new StacCatalogNode(stacObject as IStacCatalog, routeFound.Uri, credentials);
                else
                    return new StacItemNode(stacObject as StacItem, routeFound.Uri, credentials);
            }
            throw new NotSupportedException(routeFound.ContentType.ToString());
        }

    }
}
