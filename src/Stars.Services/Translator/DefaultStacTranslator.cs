using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Terradue.Stars.Interface.Router.Translator;
using Terradue.Stars.Services.Model.Stac;
using System.Collections.Generic;
using Stac;
using Terradue.Stars.Interface;
using Terradue.Stars.Services.Plugins;
using Stac.Extensions.File;

namespace Terradue.Stars.Services.Translator
{
    [PluginPriority(10)]
    public class DefaultStacTranslator : ITranslator
    {
        private ILogger logger;

        public int Priority { get; set; }
        public string Key { get => "DefaultStacTranslator"; set { } }

        public DefaultStacTranslator(ILogger<DefaultStacTranslator> logger)
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
            return new StacCatalogNode(catalog, node.Uri);
        }

        private IEnumerable<StacLink> CreateStacLinks(ICatalog node)
        {
            return new List<StacLink>();
        }

        private IItem CreateStacItemNode(IItem node)
        {
            StacItem stacItem = new StacItem(node.Id, node.Geometry, node.Properties);
            foreach (var asset in node.Assets)
            {

                Uri relativeUri = asset.Value.Uri;
                if (asset.Value.Uri.IsAbsoluteUri)
                    relativeUri = node.Uri.MakeRelativeUri(asset.Value.Uri);
                stacItem.Assets.Add(asset.Key, CreateStacAsset(asset.Value, stacItem, relativeUri));
            }
            return new StacItemNode(stacItem, node.Uri);
        }

        private StacAsset CreateStacAsset(IAsset asset, StacItem stacItem, Uri uri)
        {
            Preconditions.CheckNotNull(asset);
            StacAssetAsset stacAssetAsset = asset as StacAssetAsset;
            StacAsset stacAsset = null;
            if (stacAssetAsset == null)
            {
                stacAsset = new StacAsset(stacItem, uri, asset.Roles, asset.Title, asset.ContentType);
                stacAsset.FileExtension().Size = asset.ContentLength;
            }
            else
            {
                stacAsset = stacAssetAsset.StacAsset;
                stacAsset.Uri = uri;
            }

            return stacAsset;
        }
    }
}
