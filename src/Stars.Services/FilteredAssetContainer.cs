using System;
using System.Collections.Generic;
using System.Linq;
using Terradue.Stars.Interface;
using Terradue.Stars.Services.Supplier;

namespace Terradue.Stars.Services
{
    internal class FilteredAssetContainer : IAssetsContainer
    {
        private IAssetsContainer assetsContainer;
        private AssetFilters assetFilters;

        public FilteredAssetContainer(IAssetsContainer assetsContainer, AssetFilters assetFilters)
        {
            if (assetsContainer == null) throw new ArgumentNullException("assetsContainer");
            this.assetsContainer = assetsContainer;
            this.assetFilters = assetFilters;
        }

        public IReadOnlyDictionary<string, IAsset> Assets => assetsContainer.Assets.Where(a => AssetMatch(a)).ToDictionary(k => k.Key, k => k.Value);

        public Uri Uri => assetsContainer.Uri;

        private bool AssetMatch(KeyValuePair<string, IAsset> asset)
        {
            if (assetFilters == null) return true;
            return assetFilters.All(af => af.IsMatch(asset));
        }
    }
}