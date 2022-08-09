using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Terradue.Stars.Services.ThirdParty.Titiler
{
    public class TitilerConfiguration
    {
        public TitilerConfiguration()
        {
            BaseUrl = "https://api.cogeo.xyz/";
        }

        public TitilerConfiguration(TitilerConfiguration configuration)
        {
            BaseUrl = configuration.BaseUrl;
            Identifier = configuration.Identifier;
            UriMaps = configuration.UriMaps;
        }

        public void SetValues(TitilerConfiguration configuration)
        {
            BaseUrl = configuration.BaseUrl;
            Identifier = configuration.Identifier;
            UriMaps = configuration.UriMaps;
        }

        public string BaseUrl { get; set; }

        [JsonIgnore]
        public Uri BaseUri => new Uri(BaseUrl);

        public string Identifier { get; set; }

        public Dictionary<string, UriMap> UriMaps { get; set; }

        public Uri MapUri(Uri uri)
        {
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
    }

    public class UriMap
    {
        public string Pattern { get; set; }

        public string Replacement { get; set; }
    }
}

