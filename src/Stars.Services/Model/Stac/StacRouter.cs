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
        public string Key { get => "Stac"; set {} }

        public string Label => "Stac";

        public bool CanRoute(IResource route)
        {
            if (route is StacNode) return true;
            if (!(route is IStreamable)) return false;
            if (route.ContentType.MediaType == "application/json" || Path.GetExtension(route.Uri.ToString()) == ".json")
            {
                try
                {
                    new StacCatalogNode(StacCatalog.LoadJToken(JsonConvert.DeserializeObject<JToken>((route as IStreamable).ReadAsString().Result), route.Uri), credentials);
                    return true;
                }
                catch
                {
                    try
                    {
                        new StacItemNode(StacItem.LoadJToken(JsonConvert.DeserializeObject<JToken>((route as IStreamable).ReadAsString().Result), route.Uri), credentials);
                        return true;
                    }
                    catch { }
                }
            }
            return false;
        }

        public void Configure(IConfigurationSection configurationSection, IServiceProvider serviceProvider)
        {
            credentials = serviceProvider.GetService<ICredentials>();
        }

        public async Task<IResource> Route(IResource route)
        {
            if (route is StacCatalogNode)
                return route as StacCatalogNode;
            if (!(route is IStreamable)) return null;
            if (route.ContentType.MediaType == "application/json" || Path.GetExtension(route.Uri.ToString()) == ".json")
            {
                try
                {
                    return new StacCatalogNode(StacCatalog.LoadJToken(JsonConvert.DeserializeObject<JToken>(await (route as IStreamable).ReadAsString()), route.Uri), credentials);
                }
                catch
                {
                    try
                    {
                        return new StacItemNode(StacItem.LoadJToken(JsonConvert.DeserializeObject<JToken>(await (route as IStreamable).ReadAsString()), route.Uri), credentials);
                    }
                    catch { }
                }
            }
            throw new NotSupportedException(route.ContentType.ToString());
        }

    }
}
