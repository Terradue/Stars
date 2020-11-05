using System;
using Newtonsoft.Json;

namespace Terradue.Stars.Services.Store
{
    public class CatalogueConfiguration
    {

        public string Url { get; set; }

        [JsonIgnore]
        public Uri Uri => new Uri(Url);

        public string DestinationUrl { get; set; }

        [JsonIgnore]
        public Uri DestinationUri => new Uri(DestinationUrl);

        public string Identifier { get; set; }
        public string Description { get; set; }
    }
}