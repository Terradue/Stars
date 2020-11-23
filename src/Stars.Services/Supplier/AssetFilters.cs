using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Terradue.Stars.Services.Supplier
{
    public class AssetFilters : Dictionary<string, AssetFilter>
    {
        public static AssetFilters None => new AssetFilters();

        public static AssetFilters SkipRelative
        {
            get
            {
                var af = new AssetFilters();
                af.Add("absoluteUri", new AssetFilter() { UriRegex = new Regex(@"^(?:[a-z]+:)?//") });
                return af;
            }
        }
    }

    public class AssetFilter
    {
        public Regex UriRegex { get; set; }

        public Regex RolesRegex { get; set; }

        public Regex KeyRegex { get; set; }

        public KeyValuePair<string, Regex> PropertyRegexPattern { get; set; }
    }
}