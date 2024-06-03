// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: EgmsConfiguration.cs

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Terradue.Stars.Services.ThirdParty.Egms
{
    public class EgmsConfiguration
    {
        public EgmsConfiguration()
        {
            BaseUrl = "https://egms.terradue.com/";
        }

        public string BaseUrl { get; set; }

        [JsonIgnore]
        public Uri BaseUri => new Uri(BaseUrl);

        public string Identifier { get; set; }

        public Dictionary<string, UriMap> UriMaps { get; set; }

    }

    public class UriMap
    {
        public string Pattern { get; set; }

        public string Replacement { get; set; }
    }
}

