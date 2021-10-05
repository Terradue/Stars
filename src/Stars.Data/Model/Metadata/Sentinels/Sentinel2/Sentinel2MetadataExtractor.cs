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

        public Sentinel2MetadataExtractor(ILogger<Sentinel2MetadataExtractor> logger) : base(logger)
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

        protected override Task AddAssets(StacItem stacItem, IItem item, SentinelSafeStacFactory stacFactory)
        {
            if ( stacFactory.Manifest.informationPackageMap.contentUnit[0].unitType == "Product_Level-1C" ){
                Sentinel2Level1MetadataExtractor metadataExtractor = new Sentinel2Level1MetadataExtractor(loggerS2);
                return metadataExtractor.AddAssets(stacItem, item, stacFactory);
            }
            if ( stacFactory.Manifest.informationPackageMap.contentUnit[0].unitType == "Product_Level-2A" ){
                Sentinel2Level2MetadataExtractor metadataExtractor = new Sentinel2Level2MetadataExtractor(loggerS2);
                return metadataExtractor.AddAssets(stacItem, item, stacFactory);
            }
            return null;
        }
    }
}
