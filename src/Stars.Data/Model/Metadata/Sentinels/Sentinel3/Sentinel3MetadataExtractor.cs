// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: Sentinel3MetadataExtractor.cs

using System;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using Stac;
using Terradue.OpenSearch.Sentinel.Data.Safe;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Model.Stac;

namespace Terradue.Stars.Data.Model.Metadata.Sentinels.Sentinel3
{
    public class Sentinel3MetadataExtractor : SentinelMetadataExtractor
    {
        private S3SafeStacFactory s3SafeStacFactory;

        private static XmlSerializer xfduSerializer = new XmlSerializer(typeof(XFDUType));

        public override string Label => "Sentinel-3 (ESA) mission product metadata extractor";

        public Sentinel3MetadataExtractor(ILogger<Sentinel3MetadataExtractor> logger, IResourceServiceProvider resourceServiceProvider) : base(logger, resourceServiceProvider)
        {
        }

        public override bool CanProcess(IResource route, IDestination destinations)
        {
            if (!(route is IItem item)) return false;
            try
            {
                IAsset manifestAsset = GetManifestAsset(item);
                XFDUType manifest = ReadManifest(manifestAsset).GetAwaiter().GetResult();
                return manifest != null;
            }
            catch
            {
                return false;
            }
        }

        protected override Task<SentinelSafeStacFactory> CreateSafeStacFactoryAsync(XFDUType manifest, IItem item, string identifier)
        {
            s3SafeStacFactory = Terradue.Stars.Data.Model.Metadata.Sentinels.Sentinel3.S3SafeStacFactory.Create(manifest, item, identifier);
            return Task.FromResult<SentinelSafeStacFactory>(s3SafeStacFactory);
        }

        protected override async Task AddAssets(StacItem stacItem, IItem item, string identifier, SentinelSafeStacFactory stacFactory)
        {
            foreach (var ncAsset in FindAllAssetsFromFileNameRegex(item, "\\.nc$"))
            {
                string filename = Path.GetFileNameWithoutExtension(ncAsset.Value.Uri.ToString());
                await AddNetcdfAsset(stacItem, ncAsset.Value, stacFactory);
            }
            stacItem.Assets.Add("manifest", CreateManifestAsset(stacItem, GetManifestAsset(item)).Value);
        }

        private async Task AddNetcdfAsset(StacItem stacItem, IAsset ncAsset, SentinelSafeStacFactory stacFactory)
        {
            string filename = Path.GetFileNameWithoutExtension(ncAsset.Uri.AbsolutePath);
            string title = s3SafeStacFactory.GetAssetTitle(filename);
            StacAsset stacAsset = StacAsset.CreateDataAsset(stacItem, ncAsset.Uri, new System.Net.Mime.ContentType("application/x-netcdf"), title);
            stacAsset.Properties.AddRange(ncAsset.Properties);

            stacItem.Assets.Add(Path.GetFileNameWithoutExtension(ncAsset.Uri.AbsolutePath), stacAsset);
        }


        public override async Task<XFDUType> ReadManifest(IAsset manifestAsset)
        {
            logger.LogDebug("Opening Manifest {0}", manifestAsset.Uri);

            using (Stream stream = await resourceServiceProvider.GetAssetStreamAsync(manifestAsset, System.Threading.CancellationToken.None))
            {
                var reader = XmlReader.Create(stream);
                logger.LogDebug("Deserializing Manifest {0}", manifestAsset.Uri);

                return (XFDUType)xfduSerializer.Deserialize(reader);
            }
        }

        protected override IAsset GetManifestAsset(IItem item)
        {
            IAsset manifestAsset = FindFirstAssetFromFileNameRegex(item, "xfdumanifest.xml$") ?? throw new FileNotFoundException(string.Format("Unable to find the manifest SAFE file asset"));
            return manifestAsset;
        }

        protected override Task AddAdditionalProperties(StacItem stacItem, IItem item, string identifier, SentinelSafeStacFactory stacFactory)
        {
            if (IncludeProviderProperty)
            {
                AddSingleProvider(
                    stacItem.Properties,
                    "ESA/EC (Copernicus)",
                    "The Sentinel-3 mission comprises a constellation of two polar-orbiting satellites, equipped with instruments to monitor Earth's oceans, land, and atmosphere, enabling detailed observations of features like sea level, ocean currents, sea ice, and vegetation.",
                    new StacProviderRole[] { StacProviderRole.producer, StacProviderRole.processor, StacProviderRole.licensor },
                    new Uri("https://sentinels.copernicus.eu/copernicus/sentinel-3")
                );
            }
            return Task.CompletedTask;
        }
    }
}
