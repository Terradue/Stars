using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Terradue.Stars.Services.ThirdParty.Titiler
{
    public class TitilerConfiguration
    {
        public TitilerConfiguration()
        {
            BaseUrl = "https://api.cogeo.xyz/";
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
                    var baseUri = new Uri(kvp.Value.From);
                    return uri.AbsoluteUri.StartsWith(baseUri.AbsoluteUri);
                }
                catch { }
                return false;
            });
            if (mapping.Value != null && !string.IsNullOrEmpty(mapping.Value.To)) return new Uri(uri.ToString().Replace(mapping.Value.From, mapping.Value.To));
            return uri;
        }
    }

    public class UriMap
    {
        public string From { get; set; }

        public string To { get; set; }
    }
}

