// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: StacResourceExtensions.cs

using System;
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

        public static void MergeAssets(this StacItem stacItem, IAssetsContainer assetContainer, bool removeIfNotInContainer = false)
        {
            if (removeIfNotInContainer)
                stacItem.Assets.Clear();
            foreach (var asset in assetContainer.Assets)
            {
                if (stacItem.Assets.ContainsKey(asset.Key))
                {
                    stacItem.Assets.Remove(asset.Key);
                }
                // we must pass the previous parent url to make the asset uri absolute
                stacItem.Assets.Add(asset.Key, asset.Value.CreateAbsoluteStacAsset(stacItem, assetContainer.Uri));
            }
        }

        public static void MergeAssets(this StacCollection stacCollection, IAssetsContainer assetContainer, bool removeIfNotInContainer = false)
        {
            if (removeIfNotInContainer)
                stacCollection.Assets.Clear();
            foreach (var asset in assetContainer.Assets)
            {
                if (stacCollection.Assets.ContainsKey(asset.Key))
                {
                    stacCollection.Assets.Remove(asset.Key);
                }
                // we must pass the previous parent url to make the asset uri absolute
                stacCollection.Assets.Add(asset.Key, asset.Value.CreateAbsoluteStacAsset(stacCollection, assetContainer.Uri));
            }
        }

        public static void MergeAssets(this IDictionary<string, IAsset> assets, IReadOnlyDictionary<string, IAsset> assets2, bool removeIfNotInContainer = false)
        {
            if (removeIfNotInContainer)
                assets.Clear();
            foreach (var asset in assets2)
            {
                if (assets.ContainsKey(asset.Key))
                {
                    assets.Remove(asset.Key);
                }
                // we must pass the previous parent url to make the asset uri absolute
                assets.Add(asset.Key, asset.Value);
            }
        }

        public static StacAsset CreateAbsoluteStacAsset(this IAsset asset, IStacObject stacObject, Uri parentUrl)
        {
            StacAsset newAsset = null;
            if (asset is StacAssetAsset)
                newAsset = new StacAsset((asset as StacAssetAsset).StacAsset, stacObject);
            else
            {
                newAsset = new StacAsset(stacObject, asset.Uri, asset.Roles, asset.Title, asset.ContentType);
                newAsset.Properties.AddRange(asset.Properties);
                newAsset.FileExtension().Size = asset.ContentLength;
            }
            if (!newAsset.Uri.IsAbsoluteUri)
                newAsset.Uri = new Uri(parentUrl, asset.Uri);
            return newAsset;
        }

        public static void MakeAssetUriRelative(this StacItemNode stacItemNode)
        {
            foreach (var asset in stacItemNode.StacItem.Assets)
            {
                if (!asset.Value.Uri.IsAbsoluteUri) continue;
                var relativeUri = stacItemNode.Uri.MakeRelativeUri(asset.Value.Uri);
                if (!relativeUri.IsAbsoluteUri)
                {
                    asset.Value.Uri = relativeUri;
                    continue;
                }
            }
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

        public static IResource CreateResource(this StacLink stacLink)
        {
            if (stacLink is StacObjectLink stacObjectLink)
            {
                return stacObjectLink.StacObject.CreateResource(stacObjectLink.Uri);
            }

            return new GenericResource(stacLink.Uri);
        }

        public static IResource CreateResource(this IStacObject stacObject, Uri uri)
        {
            if (stacObject is StacItem stacItem)
                return new StacItemNode(stacItem, uri);
            if (stacObject is StacCollection stacCollection)
                return new StacCollectionNode(stacCollection, uri);
            if (stacObject is StacCatalog stacCatalog)
                return new StacCatalogNode(stacCatalog, uri);

            return new GenericResource(uri);
        }
    }

}
