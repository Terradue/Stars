using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Stac;
using Stac.Catalog;
using Stac.Item;
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
                if (!stacItem.Assets.ContainsKey(asset.Key))
                {
                    stacItem.Assets.Add(asset.Key, asset.Value.CreateStacAsset());
                    continue;
                }
                stacItem.Assets[asset.Key].Uri = asset.Value.Uri;
            }
        }

        public static StacAsset CreateStacAsset(this IAsset asset)
        {
            return new StacAsset(asset.Uri, asset.Roles, asset.Title, asset.ContentType, asset.ContentLength);
        }

        public static void UpdateLinks(this StacCatalog catalogNode, IEnumerable<IResource> resources)
        {
            foreach (var resource in resources)
            {
                if (resource == null) continue;
                foreach (var link in catalogNode.Links.Where(a => a.Uri.Equals(resource.Uri)))
                    catalogNode.Links.Remove(link);

                if (resource is ICatalog)
                    catalogNode.Links.Add(StacLink.CreateChildLink(resource.Uri, resource.ContentType.ToString()));
                if (resource is IItem)
                    catalogNode.Links.Add(StacLink.CreateItemLink(resource.Uri, resource.ContentType.ToString()));
            }
        }

    }

}