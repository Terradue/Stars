using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stac.Catalog;
using Stac.Item;
using Stars.Interface.Router;
using Stars.Router;

namespace Stars.Model.Stac
{
    [PluginPriority(1)]
    public class StacRouter : IRouter
    {

        public StacRouter()
        {
        }

        public string Label => "Stac";

        public bool CanRoute(INode resource)
        {
            if (resource.ContentType.MediaType == "application/json" || Path.GetExtension(resource.Uri.ToString()) == ".json")
            {
                try
                {
                    new StacCatalogResource(StacCatalog.LoadJToken(JsonConvert.DeserializeObject<JToken>(resource.ReadAsString()), resource.Uri));
                    return true;
                }
                catch
                {
                    try
                    {
                        new StacItemResource(StacItem.LoadJToken(JsonConvert.DeserializeObject<JToken>(resource.ReadAsString()), resource.Uri));
                        return true;
                    }
                    catch { }
                }
            }
            return false;
        }

        public async Task<IRoutable> Go(INode resource)
        {
            if (resource.ContentType.MediaType == "application/json" || Path.GetExtension(resource.Uri.ToString()) == ".json")
            {
                try
                {
                    return new StacCatalogResource(StacCatalog.LoadJToken(JsonConvert.DeserializeObject<JToken>(resource.ReadAsString()), resource.Uri));
                }
                catch
                {
                    try
                    {
                        return new StacItemResource(StacItem.LoadJToken(JsonConvert.DeserializeObject<JToken>(resource.ReadAsString()), resource.Uri));
                    }
                    catch { }
                }
            }
            throw new NotSupportedException(resource.ContentType.ToString());
        }

    }
}
