// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: StacLinkTranslator.cs

using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Stac;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router.Translator;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Services.Plugins;

namespace Terradue.Stars.Services.Translator
{
    [PluginPriority(1)]
    public class StacLinkTranslator : ITranslator
    {
        private ILogger logger;
        private readonly IResourceServiceProvider resourceServiceProvider;
        private readonly ICredentials credentials;

        public int Priority { get; set; }
        public string Key { get => "StacLinkTranslator"; set { } }

        public string Label => "STAC alternate link finder";

        public string Description => "This translator is able to find the STAC alternate link in a catalog or item and return the corresponding STAC node.";

        public StacLinkTranslator(ILogger<StacLinkTranslator> logger,
                                  IResourceServiceProvider resourceServiceProvider,
                                  ICredentials credentials)
        {
            this.logger = logger;
            this.resourceServiceProvider = resourceServiceProvider;
            this.credentials = credentials;
        }

        public async Task<T> TranslateAsync<T>(IResource route, CancellationToken ct) where T : IResource
        {
            if (typeof(T) == typeof(StacNode) || typeof(T) == typeof(StacCatalogNode))
            {
                if (route is ICatalog catalogNode)
                {
                    foreach (IResourceLink stacLink in catalogNode.GetLinks().Where(l => l.Relationship == "alternate" && l.ContentType.MediaType == "application/json"))
                    {
                        try
                        {
                            var stacRoute = await resourceServiceProvider.CreateStreamResourceAsync(stacLink, ct);
                            var stacCatalog = StacConvert.Deserialize<IStacCatalog>(await stacRoute.ReadAsStringAsync(ct));
                            if (stacCatalog != null)
                                return (T)(new StacCatalogNode(stacCatalog, stacRoute.Uri) as IResource);
                        }
                        catch { }
                    }
                }
            }

            if (typeof(T) == typeof(StacNode) || typeof(T) == typeof(StacItemNode))
            {
                if (route is IItem itemNode)
                {
                    var links = itemNode.GetLinks();
                    foreach (IResourceLink stacLink in links.Where(l => l.Relationship == "alternate" &&
                                (l.ContentType?.MediaType == "application/json" || l.ContentType?.MediaType == "application/geo+json")))
                    {
                        try
                        {
                            var stacRoute = await resourceServiceProvider.CreateStreamResourceAsync(stacLink, ct);
                            var stacItem = StacConvert.Deserialize<StacItem>(await stacRoute.ReadAsStringAsync(ct));
                            if (stacItem != null)
                                return (T)(new StacItemNode(stacItem, stacRoute.Uri) as IResource);
                        }
                        catch { }
                    }
                }
            }

            return default(T);
        }
    }
}
