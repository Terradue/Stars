using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Router.Translator;
using Terradue.Stars.Services.Model.Atom;
using Stac.Item;
using Terradue.Stars.Services.Model.Stac;
using Stac.Catalog;
using System.Collections.Generic;
using Stac;
using System.IO;
using Terradue.Stars.Interface;
using Terradue.Stars.Services.Plugins;

namespace Terradue.Stars.Services.Translator
{
    [PluginPriority(1)]
    public class StacLinkTranslator : ITranslator
    {
        private ILogger logger;

        public int Priority { get; set; }
        public string Key { get => "StacLinkTranslator"; set { } }

        public StacLinkTranslator(ILogger<StacLinkTranslator> logger)
        {
            this.logger = logger;
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
                            var stacCatalog = await StacCatalog.LoadUri(stacLink.Uri);
                            if (stacCatalog != null)
                                return (T)(new StacCatalogNode(stacCatalog) as IResource);
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
                    foreach (IResourceLink stacLink in links.Where(l => l.Relationship == "alternate" && l.ContentType?.MediaType == "application/json"))
                    {
                        try
                        {
                            var stacItem = await StacItem.LoadUri(stacLink.Uri);
                            if (stacItem != null)
                                return (T)(new StacItemNode(stacItem) as IResource);
                        }
                        catch { }
                    }
                }
            }

            return default(T);
        }
    }
}
