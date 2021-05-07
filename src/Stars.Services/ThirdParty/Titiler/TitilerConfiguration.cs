using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Terradue.Stars.Services.ThirdParty.Titiler
{
    public class TitilerConfiguration
    {
        public TitilerConfiguration()
        {
            BaseUrl = "https://api.cogeo.xyz/";
            UrlSourceMappings = new Dictionary<string, string>();
        }

        public string BaseUrl { get; set; }

        [JsonIgnore]
        public Uri BaseUri => new Uri(BaseUrl);

        public string Identifier { get; set; }

        public IDictionary<string, string> UrlSourceMappings { get; set; }

    }
}

