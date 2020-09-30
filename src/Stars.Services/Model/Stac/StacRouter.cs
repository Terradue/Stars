using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stac.Catalog;
using Stac.Item;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Services.Router;

namespace Terradue.Stars.Services.Model.Stac
{
    [PluginPriority(1)]
    public class StacRouter : IRouter
    {

        public StacRouter()
        {
        }

        public string Label => "Stac";

        public bool CanRoute(IRoute route)
        {
            if (route is StacNode) return true;
            if (!(route is IStreamable)) return false;
            if (route.ContentType.MediaType == "application/json" || Path.GetExtension(route.Uri.ToString()) == ".json")
            {
                try
                {
                    new StacCatalogNode(StacCatalog.LoadJToken(JsonConvert.DeserializeObject<JToken>((route as IStreamable).ReadAsString().Result), route.Uri));
                    return true;
                }
                catch
                {
                    try
                    {
                        new StacItemNode(StacItem.LoadJToken(JsonConvert.DeserializeObject<JToken>((route as IStreamable).ReadAsString().Result), route.Uri));
                        return true;
                    }
                    catch { }
                }
            }
            return false;
        }

        public void Configure(IConfigurationSection configurationSection, IServiceProvider serviceProvider)
        {
        }

        public async Task<IRoute> Route(IRoute route)
        {
            if (route is StacCatalogNode)
                return route as StacCatalogNode;
            if (!(route is IStreamable)) return null;
            if (route.ContentType.MediaType == "application/json" || Path.GetExtension(route.Uri.ToString()) == ".json")
            {
                try
                {
                    return new StacCatalogNode(StacCatalog.LoadJToken(JsonConvert.DeserializeObject<JToken>(await (route as IStreamable).ReadAsString()), route.Uri));
                }
                catch
                {
                    try
                    {
                        return new StacItemNode(StacItem.LoadJToken(JsonConvert.DeserializeObject<JToken>(await (route as IStreamable).ReadAsString()), route.Uri));
                    }
                    catch { }
                }
            }
            throw new NotSupportedException(route.ContentType.ToString());
        }

    }
}
