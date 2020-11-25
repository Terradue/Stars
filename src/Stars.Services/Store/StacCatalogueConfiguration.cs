using System;
using Newtonsoft.Json;

namespace Terradue.Stars.Services.Store
{
    public class StacCatalogueConfiguration
    {

        public string Url { get; set; }

        [JsonIgnore]
        public Uri Uri { get => new Uri(Url); set => Url = value.ToString(); }

        public string DestinationUrl { get; set; }

        [JsonIgnore]
        public Uri DestinationUri { get => new Uri(DestinationUrl); set => DestinationUrl = value.ToString(); }

        public string Identifier { get; set; }
        public string Description { get; set; }
    }
}