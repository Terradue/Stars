using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stac;
using Stac.Catalog;
using Stac.Item;

namespace Stars.Router
{
    public class StacRouter : IResourceRouter
    {
        public StacRouter()
        {
        }

        public bool CanRoute(IResource resource)
        {
            if (resource.ContentType.MediaType == "application/json" || Path.GetExtension(resource.Uri.LocalPath) == ".json")
            {
                try
                {
                    StacCatalog.LoadJToken(JsonConvert.DeserializeObject<JToken>(resource.ReadAsString()), resource.Uri);
                    return true;
                }
                catch (Exception)
                {
                    try
                    {
                        StacItem.LoadJToken(JsonConvert.DeserializeObject<JToken>(resource.ReadAsString()), resource.Uri);
                        return true;
                    }
                    catch{}
                }
            }
            return false;
        }

        public IRoutable Route(IResource resource)
        {
            if (resource.ContentType.MediaType == "application/json" || Path.GetExtension(resource.Uri.LocalPath) == ".json")
            {
                try
                {
                    return new StacCatalogRoutable(StacCatalog.LoadJToken(JsonConvert.DeserializeObject<JToken>(resource.ReadAsString()), resource.Uri));
                }
                catch (Exception)
                {
                    try
                    {
                        return new StacItemRoutable(StacItem.LoadJToken(JsonConvert.DeserializeObject<JToken>(resource.ReadAsString()), resource.Uri));
                    }
                    catch{}
                }
            }
            return null;
        }

    }
}
