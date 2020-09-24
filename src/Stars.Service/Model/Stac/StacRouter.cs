using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stac.Catalog;
using Stac.Item;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Service.Router;

namespace Terradue.Stars.Service.Model.Stac
{
    [PluginPriority(1)]
    public class StacRouter : IRouter
    {

        public StacRouter()
        {
        }

        public string Label => "Stac";

        public bool CanRoute(INode node)
        {
            if (node is StacNode) return true;
            if (!(node is IStreamable)) return false;
            if (node.ContentType.MediaType == "application/json" || Path.GetExtension(node.Uri.ToString()) == ".json")
            {
                try
                {
                    new StacCatalogNode(StacCatalog.LoadJToken(JsonConvert.DeserializeObject<JToken>((node as IStreamable).ReadAsString()), node.Uri));
                    return true;
                }
                catch
                {
                    try
                    {
                        new StacItemNode(StacItem.LoadJToken(JsonConvert.DeserializeObject<JToken>((node as IStreamable).ReadAsString()), node.Uri));
                        return true;
                    }
                    catch { }
                }
            }
            return false;
        }

        public async Task<IRoutable> Route(INode node)
        {
            if (node is StacNode)
                return node as StacNode;
            if (!(node is IStreamable)) return null;
            if (node.ContentType.MediaType == "application/json" || Path.GetExtension(node.Uri.ToString()) == ".json")
            {
                try
                {
                    return new StacCatalogNode(StacCatalog.LoadJToken(JsonConvert.DeserializeObject<JToken>((node as IStreamable).ReadAsString()), node.Uri));
                }
                catch
                {
                    try
                    {
                        return new StacItemNode(StacItem.LoadJToken(JsonConvert.DeserializeObject<JToken>((node as IStreamable).ReadAsString()), node.Uri));
                    }
                    catch { }
                }
            }
            throw new NotSupportedException(node.ContentType.ToString());
        }

    }
}
