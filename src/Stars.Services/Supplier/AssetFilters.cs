using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Services.Supplier
{
    public class AssetFilters : List<IAssetFilter>
    {

        public static AssetFilters None => new AssetFilters();

        public static AssetFilters SkipRelative
        {
            get
            {
                var af = new AssetFilters();
                af.Add(new UriAssetFilter(new Regex(@"(^(?:[a-z0-9]+:)?\/\/|^\/[a-zA-Z0-9-_]+)")));
                return af;
            }
        }
    }

    public interface IAssetFilter
    {
        bool IsMatch(KeyValuePair<string, IAsset> asset);
    }

    public class UriAssetFilter : IAssetFilter
    {
        public UriAssetFilter(Regex uriRegex)
        {
            UriRegex = uriRegex;
        }

        public Regex UriRegex { get; set; }

        public bool IsMatch(KeyValuePair<string, IAsset> asset)
        {
            return UriRegex.IsMatch(asset.Value.Uri.ToString());
        }
    }



    public class RolesAssetFilter : IAssetFilter
    {

        public Regex RolesRegex { get; set; }

        public RolesAssetFilter(Regex rolesRegex)
        {
            RolesRegex = rolesRegex;
        }

        public bool IsMatch(KeyValuePair<string, IAsset> asset)
        {
            return asset.Value.Roles.Any(role => RolesRegex.IsMatch(role));
        }
    }

    public class KeyAssetFilter : IAssetFilter
    {

        public Regex KeyRegex { get; set; }

        public KeyAssetFilter(Regex keyRegex)
        {
            KeyRegex = keyRegex;
        }

        public bool IsMatch(KeyValuePair<string, IAsset> asset)
        {
            return KeyRegex.IsMatch(asset.Key);
        }
    }

    public class PropertyAssetFilter : IAssetFilter
    {

        public KeyValuePair<string, Regex> PropertyRegexPattern { get; set; }

        public PropertyAssetFilter(string key, Regex valuePattern)
        {
            PropertyRegexPattern = new KeyValuePair<string, Regex>(key, valuePattern);
        }

        public bool IsMatch(KeyValuePair<string, IAsset> asset)
        {
            return asset.Value.Properties.ContainsKey(PropertyRegexPattern.Key) &&
                PropertyRegexPattern.Value.IsMatch(asset.Value.Properties[PropertyRegexPattern.Key].ToString());
        }
    }
}