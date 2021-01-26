using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stac.Catalog;
using Stac.Item;
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

        public StacRouter(ICredentials credentials)
        {
            this.credentials = credentials;
        }

        public int Priority { get; set; }
        public string Key { get => "Stac"; set { } }

        public string Label => "Stac";

        public bool CanRoute(IResource route)
        {
            var routeFound = AffineRoute(route);
            if (routeFound is StacNode) return true;
            if (!(routeFound is IStreamable)) return false;
            if (routeFound.ContentType.MediaType.Contains("application/json") || Path.GetExtension(routeFound.Uri.ToString()) == ".json")
            {
                try
                {
                    new StacCatalogNode(StacCatalog.LoadJToken(JsonConvert.DeserializeObject<JToken>((routeFound as IStreamable).ReadAsString().Result), routeFound.Uri), credentials);
                    return true;
                }
                catch
                {
                    try
                    {
                        new StacItemNode(StacItem.LoadJToken(JsonConvert.DeserializeObject<JToken>((routeFound as IStreamable).ReadAsString().Result), routeFound.Uri), credentials);
                        return true;
                    }
                    catch { }
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
                        if ( newRoute.ContentType.MediaType.Contains("application/json"))
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
                try
                {
                    return new StacCatalogNode(StacCatalog.LoadJToken(JsonConvert.DeserializeObject<JToken>(await (routeFound as IStreamable).ReadAsString()), routeFound.Uri), credentials);
                }
                catch
                {
                    return new StacItemNode(StacItem.LoadJToken(JsonConvert.DeserializeObject<JToken>(await (routeFound as IStreamable).ReadAsString()), routeFound.Uri), credentials);
                }
            }
            throw new NotSupportedException(routeFound.ContentType.ToString());
        }

    }
}
