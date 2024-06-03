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

        public override string ToString()
        {
            return Count == 0 ? "No filter" : string.Join(", ", this.Select(af => af.ToString()));
        }

        public static AssetFilters CreateAssetFilters(IEnumerable<string> assetFiltersStr)
        {
            AssetFilters assetFilters = new AssetFilters();
            if (assetFiltersStr == null)
                return assetFilters;
            Regex propertyRegex = new Regex(@"^\{(?'key'[\w:]*)\}(?'value'.*)$");
            foreach (var assetFilterStr in assetFiltersStr)
            {
                Match propertyMatch = propertyRegex.Match(assetFilterStr);
                if (propertyMatch.Success)
                {
                    if (propertyMatch.Groups["key"].Value == "roles")
                    {
                        assetFilters.Add(new RolesAssetFilter(new Regex(propertyMatch.Groups["value"].Value)));
                        continue;
                    }
                    if (propertyMatch.Groups["key"].Value == "uri")
                    {
                        assetFilters.Add(new UriAssetFilter(new Regex(propertyMatch.Groups["value"].Value)));
                        continue;
                    }
                    if (propertyMatch.Groups["key"].Value == "type")
                    {
                        assetFilters.Add(new ContentTypeAssetFilter(propertyMatch.Groups["value"].Value, null));
                        continue;
                    }
                    Dictionary<string, Regex> dic = new Dictionary<string, Regex>();
                    dic.Add(propertyMatch.Groups["key"].Value, new Regex(propertyMatch.Groups["value"].Value));
                    assetFilters.Add(new PropertyAssetFilter(dic));
                    continue;
                }
                else
                {
                    assetFilters.Add(new KeyAssetFilter(new Regex("^" + assetFilterStr + "$")));
                }
            }
            return assetFilters;
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

        public override string ToString()
        {
            return "uri[regex]: " + UriRegex.ToString();
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

        public override string ToString()
        {
            return "role[regex]: " + RolesRegex.ToString();
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

        public override string ToString()
        {
            return "key[regex]: " + KeyRegex.ToString();
        }
    }

    public class PropertyAssetFilter : IAssetFilter
    {
        public Dictionary<string, Regex> PropertiesRegexPattern { get; set; }

        public PropertyAssetFilter(Dictionary<string, Regex> filters)
        {
            PropertiesRegexPattern = new Dictionary<string, Regex>(filters);
        }

        public bool IsMatch(KeyValuePair<string, IAsset> asset)
        {
            return PropertiesRegexPattern.All(kvp =>
            {
                return asset.Value.Properties.ContainsKey(kvp.Key) &&
                kvp.Value.IsMatch(asset.Value.Properties[kvp.Key].ToString());
            });
        }
    }

    public class ContentTypeAssetFilter : IAssetFilter
    {
        private readonly string mediaType;

        private readonly bool negated = false;

        private readonly Dictionary<string, string> parameters;

        public ContentTypeAssetFilter(string mediaType, Dictionary<string, string> parameters)
        {
            if( mediaType.StartsWith("!") )
            {
                this.mediaType = mediaType.Substring(1);
                this.negated = true;
            }
            else
            {
                this.mediaType = mediaType;
            }
            this.parameters = parameters;
        }

        public bool IsMatch(KeyValuePair<string, IAsset> asset)
        {
            return negated ?
                ( string.IsNullOrEmpty(mediaType) ||
                  !asset.Value.ContentType.MediaType.Equals(mediaType, System.StringComparison.InvariantCultureIgnoreCase)
                ) &&
                ( parameters == null ||
                  parameters.Any(p => !asset.Value.ContentType.Parameters.ContainsKey(p.Key) ||
                                      asset.Value.ContentType.Parameters[p.Key] != p.Value)
                ) :
                ( string.IsNullOrEmpty(mediaType) ||
                  asset.Value.ContentType.MediaType.Equals(mediaType, System.StringComparison.InvariantCultureIgnoreCase)
                ) &&
                ( parameters == null ||
                  parameters.Any(p => asset.Value.ContentType.Parameters.ContainsKey(p.Key) &&
                                      asset.Value.ContentType.Parameters[p.Key] == p.Value)
                );
        }
    }

    public class NotAssetFilter : IAssetFilter
    {
        private readonly IAssetFilter assetFilter;

        public NotAssetFilter(IAssetFilter assetFilter)
        {
            this.assetFilter = assetFilter;
        }

        public bool IsMatch(KeyValuePair<string, IAsset> asset)
        {
            return !assetFilter.IsMatch(asset);
        }
    }
}