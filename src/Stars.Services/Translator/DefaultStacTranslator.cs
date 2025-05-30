﻿// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: DefaultStacTranslator.cs

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Stac;
using Stac.Extensions.File;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router.Translator;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Services.Plugins;

namespace Terradue.Stars.Services.Translator
{
    [PluginPriority(10)]
    public class DefaultStacTranslator : ITranslator
    {
        private readonly ILogger logger;

        public int Priority { get; set; }
        public string Key { get => "DefaultStacTranslator"; set { } }

        public string Label => "Default STAC Translator";

        public string Description
        {
            get
            {
                var desc = "This is the default STAC Translator that can read any catalog or item implementing the ICatalog or IItem interfaces:";
                // Get the list of all the ICatalog and IItem implementations in the current assembly
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (Assembly assembly in assemblies)
                {
                    try
                    {
                        foreach (Type type in assembly.GetTypes())
                        {
                            if ((typeof(ICatalog).IsAssignableFrom(type) || typeof(IItem).IsAssignableFrom(type))
                                && !type.IsInterface && !type.IsAbstract && !type.IsGenericType && !type.IsNested)
                            {
                                // Get the xml documentation for the type if available
                                desc += $" {type.Name}";
                            }
                        }
                    }
                    catch { }
                }
                return desc;
            }
        }

        public DefaultStacTranslator(ILogger<DefaultStacTranslator> logger)
        {
            this.logger = logger;
        }

        public Task<T> TranslateAsync<T>(IResource route, CancellationToken ct) where T : IResource
        {
            if (route is ICatalog catalogRoute)
            {
                return Task.FromResult((T)CreateStacCatalogNode(catalogRoute));
            }
            if (route is IItem itemRoute)
            {
                return Task.FromResult((T)CreateStacItemNode(itemRoute));
            }
            return Task.FromResult(default(T));
        }

        private IResource CreateStacCatalogNode(ICatalog node)
        {
            StacCatalog catalog = new StacCatalog(node.Id, node.Title, CreateStacLinks(node));
            return new StacCatalogNode(catalog, node.Uri);
        }

        private IEnumerable<StacLink> CreateStacLinks(ICatalog node)
        {
            return new List<StacLink>();
        }

        private IItem CreateStacItemNode(IItem node)
        {
            StacItem stacItem = new StacItem(node.Id, node.Geometry, node.Properties);
            MakeStacItemValid(stacItem);
            // Assets
            foreach (var asset in node.Assets)
            {
                Uri relativeUri = asset.Value.Uri;
                if (asset.Value.Uri.IsAbsoluteUri)
                    relativeUri = node.Uri.MakeRelativeUri(asset.Value.Uri);
                stacItem.Assets.Add(asset.Key, CreateStacAsset(asset.Value, stacItem, relativeUri));
            }
            // Links
            foreach (var link in node.GetLinks())
            {
                if (!link.Uri.IsAbsoluteUri)
                    continue;
                stacItem.Links.Add(new StacLink(link.Uri, link.Relationship, link.Title, link.ContentType?.ToString()));
            }
            return new StacItemNode(stacItem, node.Uri);
        }

        private void MakeStacItemValid(StacItem stacItem)
        {
            Itenso.TimePeriod.ITimePeriod dateTime = Itenso.TimePeriod.TimeInterval.Anytime;
            try
            {
                dateTime = stacItem.DateTime;
            }
            catch
            {
                stacItem.DateTime = dateTime;
            }
        }

        private StacAsset CreateStacAsset(IAsset asset, StacItem stacItem, Uri uri)
        {
            Preconditions.CheckNotNull(asset);
            StacAsset stacAsset = null;
            if (!(asset is StacAssetAsset stacAssetAsset))
            {
                stacAsset = new StacAsset(stacItem, uri, asset.Roles, asset.Title, asset.ContentType);
                try
                {
                    stacAsset.FileExtension().Size = asset.ContentLength;
                }
                catch { }
                foreach (var key in asset.Properties.Keys)
                {
                    stacAsset.Properties.Remove(key);
                    stacAsset.Properties.Add(key, asset.Properties[key]);
                }
            }
            else
            {
                stacAsset = stacAssetAsset.StacAsset;
                stacAsset.Uri = uri;
            }

            return stacAsset;
        }
    }
}
