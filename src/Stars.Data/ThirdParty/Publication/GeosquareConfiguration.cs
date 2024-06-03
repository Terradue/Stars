using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Terradue.Stars.Data.ThirdParty.Geosquare
{
    public class GeosquareConfiguration
    {

        public string DefaultIndex { get; set; }

        public bool CreateIndex { get; set; }

        public Dictionary<string, UriMap> UriMaps { get; set; }

        public Uri MapUri(Uri uri)
        {
            if (UriMaps == null) return uri;  // no map
            var mapping = UriMaps.FirstOrDefault(kvp =>
            {
                try
                {
                    Regex regex = new Regex(kvp.Value.Pattern);
                    return regex.IsMatch(uri.ToString());
                }
                catch { }
                return false;
            });
            if (mapping.Value != null && !string.IsNullOrEmpty(mapping.Value.Replacement))
                return new Uri(Regex.Replace(uri.ToString(), mapping.Value.Pattern, mapping.Value.Replacement));
            return uri;
        }

        public Dictionary<string, UriMap> OpenSearchTemplatesMap { get; internal set; }

        public Uri BaseUri { get; set; }

        public string BaseUrl
        {
            get
            {
                return BaseUri?.ToString();
            }
            set
            {
                BaseUri = new Uri(value);
            }
        }

        public string GetOpenSearchForUri(Uri uri)
        {
            var mapping = OpenSearchTemplatesMap.FirstOrDefault(kvp =>
            {
                try
                {
                    return Regex.IsMatch(uri.ToString(), kvp.Value.Pattern);
                }
                catch { }
                return false;
            });
            if (mapping.Value != null && !string.IsNullOrEmpty(mapping.Value.Replacement))
                return Regex.Replace(uri.ToString(), mapping.Value.Pattern, mapping.Value.Replacement);
            return uri.ToString();
        }
    }

    public class UriMap
    {
        public string Pattern { get; set; }

        public string Replacement { get; set; }
    }
}
