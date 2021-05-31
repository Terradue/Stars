using System.Collections.Generic;
using System.Linq;
using Stac;
using Stac.Extensions.File;
using Terradue.Stars.Interface;
using Terradue.Stars.Services.Model.Stac;

namespace Terradue.Stars.Services
{
    public static class StacResourceExtensions
    {

        public static void MergeAssets(this StacItem stacItem, IAssetsContainer assetContainer)
        {
            foreach (var asset in assetContainer.Assets)
            {
                if (stacItem.Assets.ContainsKey(asset.Key))
                {
                    stacItem.Assets.Remove(asset.Key);
                }
                stacItem.Assets.Add(asset.Key, asset.Value.CreateStacAsset(stacItem));
            }
        }

        public static StacAsset CreateStacAsset(this IAsset asset, StacItem stacItem)
        {
            if (asset is StacAssetAsset) return new StacAsset((asset as StacAssetAsset).StacAsset, stacItem);
            var stacAsset = new StacAsset(stacItem, asset.Uri, asset.Roles, asset.Title, asset.ContentType);
            stacAsset.FileExtension().Size = asset.ContentLength;
            return stacAsset;
        }

        public static void UpdateLinks(this IStacCatalog catalogNode, IEnumerable<IResource> resources)
        {
            foreach (var resource in resources)
            {
                if (resource == null) continue;
                foreach (var link in catalogNode.Links.Where(a => a != null && a.Uri.Equals(resource.Uri)).ToArray())
                    catalogNode.Links.Remove(link);

                if (resource is ICatalog)
                    catalogNode.Links.Add(StacLink.CreateChildLink(resource.Uri, resource.ContentType.ToString()));
                if (resource is IItem)
                    catalogNode.Links.Add(StacLink.CreateItemLink(resource.Uri, resource.ContentType.ToString()));
            }
        }

    }

}