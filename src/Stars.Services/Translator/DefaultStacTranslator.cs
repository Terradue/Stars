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

namespace Terradue.Stars.Services.Translator
{
    public class DefaultStacTranslator : ITranslator
    {
        private ILogger logger;

        public int Priority { get; set; }
        public string Key { get => "StacTranslator"; set {} }

        public DefaultStacTranslator(ILogger logger)
        {
            this.logger = logger;
        }

        public Task<T> Translate<T>(IResource route) where T : IResource
        {
            ICatalog catalogRoute = route as ICatalog;
            if (catalogRoute != null)
            {
                return Task.FromResult<T>((T)CreateStacCatalogNode(catalogRoute));
            }
            IItem itemRoute = route as IItem;
            if (itemRoute != null)
            {
                return Task.FromResult<T>((T)CreateStacItemNode(itemRoute));
            }
            return Task.FromResult<T>(default(T));
        }

        private IResource CreateStacCatalogNode(ICatalog node)
        {
            StacCatalog catalog = new StacCatalog(node.Id, node.Label, CreateStacLinks(node));
            return new StacCatalogNode(catalog);
        }

        private IEnumerable<StacLink> CreateStacLinks(ICatalog node)
        {
            return new List<StacLink>();
        }

        private IItem CreateStacItemNode(IItem node)
        {
            StacItem stacItem = new StacItem(node.Geometry, node.Properties, node.Id);
            foreach(var kvp in node.GetAssets()){
                Uri relativeUri = kvp.Value.Uri;
                if (kvp.Value.Uri.IsAbsoluteUri)
                    relativeUri = node.Uri.MakeRelativeUri(kvp.Value.Uri);
                stacItem.Assets.Add(kvp.Key, CreateStacAsset(kvp.Value, relativeUri)); 
            }
            return new StacItemNode(stacItem);
        }

        private StacAsset CreateStacAsset(IAsset asset, Uri relativeUri)
        {
            StacAssetAsset stacAssetAsset = asset as StacAssetAsset;
            StacAsset stacAsset = null;
            if (stacAssetAsset == null)
                stacAsset = new StacAsset(relativeUri, asset.Roles, asset.Label, asset.ContentType, asset.ContentLength);
            else
            {
                stacAsset = stacAssetAsset.StacAsset;
                stacAsset.Uri = relativeUri;
            }

            return stacAsset;
        }

        public static DefaultStacTranslator Create(IConfigurationSection configurationSection, IServiceProvider serviceProvider)
        {
            return new DefaultStacTranslator((ILogger)serviceProvider.GetService(typeof(ILogger)));
        }

        public void Configure(IConfigurationSection configurationSection, IServiceProvider serviceProvider)
        {
        
        }
    }
}
