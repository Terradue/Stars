using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Terradue.Stars.Services.ThirdParty.Titiler
{
    public class TitilerConfiguration
    {
        public TitilerConfiguration()
        {
        }

        public string BaseUrl { get; set; }

        [JsonIgnore]
        public Uri BaseUri => new Uri(BaseUrl);

        public string Identifier { get; set; }

    }
}

