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
using System.Threading;

namespace Terradue.Stars.Services.Translator
{
    [PluginPriority(1)]
    public class StacLinkTranslator : ITranslator
    {
        private ILogger logger;
        private readonly IResourceServiceProvider resourceServiceProvider;
        private readonly ICredentials credentials;

        public int Priority { get; set; }
        public string Key { get => "StacLinkTranslator"; set { } }

        public string Label => "STAC alternate link finder";

        public string Description => "This translator is able to find the STAC alternate link in a catalog or item and return the corresponding STAC node.";

        public StacLinkTranslator(ILogger<StacLinkTranslator> logger,
                                  IResourceServiceProvider resourceServiceProvider,
                                  ICredentials credentials)
        {
            this.logger = logger;
            this.resourceServiceProvider = resourceServiceProvider;
            this.credentials = credentials;
        }

        public async Task<T> TranslateAsync<T>(IResource route, CancellationToken ct) where T : IResource
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
                            var stacRoute = await resourceServiceProvider.CreateStreamResourceAsync(stacLink, ct);
                            var stacCatalog = StacConvert.Deserialize<IStacCatalog>(await stacRoute.ReadAsStringAsync(ct));
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
                            var stacRoute = await resourceServiceProvider.CreateStreamResourceAsync(stacLink, ct);
                            var stacItem = StacConvert.Deserialize<StacItem>(await stacRoute.ReadAsStringAsync(ct));
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
