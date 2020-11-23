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

        public IDictionary<string, IAsset> GetAssets()
        {
            Dictionary<string, IAsset> filteredAssets = new Dictionary<string, IAsset>();
            foreach (var asset in assetsContainer.GetAssets())
            {
                if (AssetMatch(asset))
                    filteredAssets.Add(asset.Key, asset.Value);
            }
            return filteredAssets;
        }

        private bool AssetMatch(KeyValuePair<string, IAsset> asset)
        {
            if (assetFilters == null) return true;
            return assetFilters.Values.All(af =>
                (af.KeyRegex == null || af.KeyRegex.IsMatch(asset.Key)) &&
                (af.RolesRegex == null || asset.Value.Roles.Any(role => af.RolesRegex.IsMatch(role))) &&
                (af.UriRegex == null || af.UriRegex.IsMatch(asset.Value.Uri.ToString())) &&
                (af.PropertyRegexPattern.Value == null || af.PropertyRegexPattern.Value.IsMatch(asset.Value.Properties[af.PropertyRegexPattern.Key].ToString()))
            );
        }
    }
}