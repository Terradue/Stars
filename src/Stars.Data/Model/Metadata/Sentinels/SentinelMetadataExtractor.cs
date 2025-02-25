﻿// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: SentinelMetadataExtractor.cs

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using Stac;
using Stac.Extensions.Eo;
using Terradue.OpenSearch.Sentinel.Data.Safe;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Model.Stac;

namespace Terradue.Stars.Data.Model.Metadata.Sentinels
{
    public abstract class SentinelMetadataExtractor : MetadataExtraction
    {
        public static XmlSerializer xfduSerializer = new XmlSerializer(typeof(XFDUType));

        protected SentinelMetadataExtractor(ILogger logger, IResourceServiceProvider resourceServiceProvider) : base(logger, resourceServiceProvider)
        {
        }

        public override bool CanProcess(IResource route, IDestination destination)
        {
            if (!(route is IItem item)) return false;
            try
            {
                IAsset manifestAsset = GetManifestAsset(item);
                XFDUType manifest = ReadManifest(manifestAsset).GetAwaiter().GetResult();
                string identifier = null;
                Match match = Regex.Match(manifestAsset.Uri.ToString(), @"(.*\/)*(?'identifier'S(1|2|3)[^\.\/]{10,})(\.\w+)*(\/.*)*");
                if (match.Success)
                    identifier = match.Groups["identifier"].Value;
                else return false;
                SentinelSafeStacFactory stacFactory = CreateSafeStacFactoryAsync(manifest, item, identifier).GetAwaiter().GetResult();
                return stacFactory != null;
            }
            catch
            {
                return false;
            }
        }

        protected override async Task<StacNode> ExtractMetadata(IItem item, string suffix)
        {
            IAsset manifestAsset = GetManifestAsset(item);

            string identifier = null;
            Match match = Regex.Match(manifestAsset.Uri.ToString(), @"(.*\/)*(?'identifier'S(1|2|3)[^\.\/]{10,})(\.\w+)*(\/.*)*");
            if (match.Success)
                identifier = match.Groups["identifier"].Value;
            else
                identifier = item.Id;
            identifier += suffix;

            XFDUType manifest = await ReadManifest(manifestAsset);

            SentinelSafeStacFactory stacFactory = await CreateSafeStacFactoryAsync(manifest, item, identifier);
            StacItem stacItem = stacFactory.CreateStacItem();

            // Get the proper instance for assets and additional properties
            // (can be this instance or a subclass instance, e.g. for Sentinel-2)
            SentinelMetadataExtractor metadataExtractor = GetMatchingExtractorInstance(stacFactory);

            await metadataExtractor.AddAssets(stacItem, item, identifier, stacFactory);
            await metadataExtractor.AddAdditionalProperties(stacItem, item, identifier, stacFactory);

            // AddEoBandPropertyInItem(stacItem);

            var stacNode = StacNode.Create(stacItem, item.Uri);

            return stacNode;
        }

        private void AddEoBandPropertyInItem(StacItem stacItem)
        {
            var eo = stacItem.EoExtension();
            if (eo == null) return;
            eo.Bands = stacItem.Assets.Values.Where(a => a.EoExtension().Bands != null).SelectMany(a => a.EoExtension().Bands).ToArray();
        }

        protected virtual SentinelMetadataExtractor GetMatchingExtractorInstance(SentinelSafeStacFactory stacFactory)
        {
            return this;
        }

        protected abstract Task AddAssets(StacItem stacItem, IItem item, string identifier, SentinelSafeStacFactory stacFactory);

        protected virtual Task AddAdditionalProperties(StacItem stacItem, IItem item, string identifier, SentinelSafeStacFactory stacFactory)
        {
            return Task.CompletedTask;
        }

        protected virtual IAsset GetManifestAsset(IItem item)
        {
            IAsset manifestAsset = FindFirstAssetFromFileNameRegex(item, "manifest.safe$") ?? throw new FileNotFoundException(string.Format("Unable to find the manifest SAFE file asset"));
            return manifestAsset;
        }
        protected abstract Task<SentinelSafeStacFactory> CreateSafeStacFactoryAsync(XFDUType manifest, IItem item, string identifier);

        public virtual async Task<XFDUType> ReadManifest(IAsset manifestAsset)
        {
            logger.LogDebug("Opening Manifest {0}", manifestAsset.Uri);

            using (Stream stream = await resourceServiceProvider.GetAssetStreamAsync(manifestAsset, System.Threading.CancellationToken.None))
            {
                var reader = XmlReader.Create(stream);
                logger.LogDebug("Deserializing Manifest {0}", manifestAsset.Uri);

                return (XFDUType)xfduSerializer.Deserialize(reader);
            }
        }

        protected KeyValuePair<string, StacAsset> CreateManifestAsset(IStacObject stacObject, IAsset asset)
        {
            StacAsset stacAsset = StacAsset.CreateMetadataAsset(stacObject, asset.Uri, new ContentType("text/xml"), "SAFE Manifest");
            stacAsset.Properties.AddRange(asset.Properties);
            return new KeyValuePair<string, StacAsset>("manifest", stacAsset);
        }
    }
}
