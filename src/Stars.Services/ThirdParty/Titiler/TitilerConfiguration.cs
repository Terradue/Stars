﻿// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: TitilerConfiguration.cs

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
            ServiceMaps = configuration.ServiceMaps;
        }

        [Obsolete("Use ServiceMaps instead")]
        public string BaseUrl { get; set; }

        [JsonIgnore]
        public Uri BaseUri => new Uri(BaseUrl);

        [Obsolete("Use ServiceMaps instead")]
        public string Identifier { get; set; }

        public Dictionary<string, ServiceMap> ServiceMaps { get; set; }

        public Dictionary<string, UriMap> UriMaps { get; set; }

        public Uri MapUri(Uri uri)
        {
            if (UriMaps == null) return uri;
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

        public Uri GetService(Uri uri)
        {
            if (ServiceMaps == null) return BaseUri;
            var mapping = ServiceMaps.FirstOrDefault(kvp =>
            {
                try
                {
                    Regex regex = new Regex(kvp.Value.UrlPattern);
                    return regex.IsMatch(uri.ToString());
                }
                catch { }
                return false;
            });
            if (mapping.Value != null && !string.IsNullOrEmpty(mapping.Value.ServiceUrl))
                return new Uri(mapping.Value.ServiceUrl);
            return BaseUri;
        }
    }

    public class UriMap
    {
        public string Pattern { get; set; }

        public string Replacement { get; set; }
    }

    public class ServiceMap
    {
        public string UrlPattern { get; set; }

        public string ServiceUrl { get; set; }
    }
}

