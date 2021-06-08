using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Terradue.Stars.Interface.Router.Translator;
using Terradue.Stars.Services.Model.Stac;
using Stac;
using Terradue.Stars.Interface;
using Terradue.Stars.Services.Plugins;
using Terradue.Stars.Services.Router;
using System.Net;

namespace Terradue.Stars.Services.Translator
{
    [PluginPriority(1)]
    public class StacLinkTranslator : ITranslator
    {
        private ILogger logger;
        private readonly ICredentials credentials;

        public int Priority { get; set; }
        public string Key { get => "StacLinkTranslator"; set { } }

        public StacLinkTranslator(ILogger<StacLinkTranslator> logger, ICredentials credentials)
        {
            this.logger = logger;
            this.credentials = credentials;
        }

        public async Task<T> Translate<T>(IResource route) where T : IResource
        {
            if (typeof(T) == typeof(StacNode) || typeof(T) == typeof(StacCatalogNode))
            {
                ICatalog catalogNode = route as ICatalog;
                if (catalogNode != null)
                {
                    foreach (IResourceLink stacLink in catalogNode.GetLinks().Where(l => l.Relationship == "alternate" && l.ContentType.MediaType == "application/json"))
                    {
                        try
                        {
                            var stacRoute = WebRoute.Create(stacLink.Uri, credentials: credentials);
                            var stacCatalog = StacConvert.Deserialize<IStacCatalog>(await stacRoute.ReadAsString());
                            if (stacCatalog != null)
                                return (T)(new StacCatalogNode(stacCatalog, stacRoute.Uri) as IResource);
                        }
                        catch { }
                    }
                }
            }

            if (typeof(T) == typeof(StacNode) || typeof(T) == typeof(StacItemNode))
            {
                IItem itemNode = route as IItem;
                if (itemNode != null)
                {
                    var links = itemNode.GetLinks();
                    foreach (IResourceLink stacLink in links.Where(l => l.Relationship == "alternate" && 
                                (l.ContentType?.MediaType == "application/json" || l.ContentType?.MediaType == "application/geo+json")))
                    {
                        try
                        {
                            var stacRoute = WebRoute.Create(stacLink.Uri, credentials: credentials);
                            var stacItem = StacConvert.Deserialize<StacItem>(await stacRoute.ReadAsString());
                            if (stacItem != null)
                                return (T)(new StacItemNode(stacItem, stacRoute.Uri) as IResource);
                        }
                        catch { }
                    }
                }
            }

            return default(T);
        }
    }
}
