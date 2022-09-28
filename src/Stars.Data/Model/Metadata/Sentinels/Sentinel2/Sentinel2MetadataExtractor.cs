using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Stac;
using Terradue.OpenSearch.Sentinel.Data.Safe;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Data.Model.Metadata.Sentinels.Sentinel2
{
    public class Sentinel2MetadataExtractor : SentinelMetadataExtractor
    {
        protected readonly ILogger<Sentinel2MetadataExtractor> loggerS2;

        public override string Label => "Sentinel-2 (ESA) mission product metadata extractor";

        public Sentinel2MetadataExtractor(ILogger<Sentinel2MetadataExtractor> logger, IResourceServiceProvider resourceServiceProvider) : base(logger, resourceServiceProvider)
        {
            this.loggerS2 = logger;
        }

        public override async Task<XFDUType> ReadManifest(IAsset manifestAsset)
        {
            XFDUType xfdu = await base.ReadManifest(manifestAsset);
            if (!xfdu.informationPackageMap.contentUnit[0].textInfo.StartsWith("Sentinel-2", true, CultureInfo.InvariantCulture))
            {
                throw new FormatException(String.Format("Not a Sentinel-2 manifest SAFE file asset"));
            }
            return xfdu;
        }

        protected override SentinelSafeStacFactory CreateSafeStacFactory(XFDUType manifest, IItem item, string identifier)
        {
            return S2SafeStacFactory.Create(manifest, item, identifier);
        }

        protected override SentinelMetadataExtractor GetMatchingExtractorInstance(SentinelSafeStacFactory stacFactory)
        {
            if ( stacFactory.Manifest.informationPackageMap.contentUnit[0].unitType == "Product_Level-1C" )
            {
                return new Sentinel2Level1MetadataExtractor(loggerS2, resourceServiceProvider);
            }
            if ( stacFactory.Manifest.informationPackageMap.contentUnit[0].unitType == "Product_Level-2A" )
            {
                return new Sentinel2Level2MetadataExtractor(loggerS2, resourceServiceProvider);
            }
            return base.GetMatchingExtractorInstance(stacFactory);
        }
        
        // This method is not normally used, subclass method is called directly with the proper instance
        protected override Task AddAssets(StacItem stacItem, IItem item, SentinelSafeStacFactory stacFactory)
        {
            Sentinel2MetadataExtractor metadataExtractor = GetMatchingExtractorInstance(stacFactory) as Sentinel2MetadataExtractor;
            if (metadataExtractor == null) return Task.CompletedTask;

            return metadataExtractor.AddAssets(stacItem, item, stacFactory);
        }

        // This method is not normally used, subclass method is called directly with the proper instance
        protected override Task AddAdditionalProperties(StacItem stacItem, IItem item, SentinelSafeStacFactory stacFactory)
        {
            Sentinel2MetadataExtractor metadataExtractor = GetMatchingExtractorInstance(stacFactory) as Sentinel2MetadataExtractor;
            if (metadataExtractor == null) return Task.CompletedTask;

            return metadataExtractor.AddAdditionalProperties(stacItem, item, stacFactory);
        }
    }
}
