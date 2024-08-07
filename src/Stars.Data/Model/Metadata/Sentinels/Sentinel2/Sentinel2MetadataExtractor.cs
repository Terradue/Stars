// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: Sentinel2MetadataExtractor.cs

using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Stac;
using Stac.Extensions.Eo;
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
            loggerS2 = logger;
        }

        public override async Task<XFDUType> ReadManifest(IAsset manifestAsset)
        {
            XFDUType xfdu = await base.ReadManifest(manifestAsset);
            if (!xfdu.informationPackageMap.contentUnit[0].textInfo.StartsWith("Sentinel-2", true, CultureInfo.InvariantCulture))
            {
                throw new FormatException(string.Format("Not a Sentinel-2 manifest SAFE file asset"));
            }
            return xfdu;
        }

        protected override async Task<SentinelSafeStacFactory> CreateSafeStacFactoryAsync(XFDUType manifest, IItem item, string identifier)
        {
            // Read the sentinel2 level and use the proper factory
            SentinelSafeStacFactory stacFactory = null;
            if (manifest.informationPackageMap.contentUnit[0].unitType == "Product_Level-1C")
            {
                var extractor = new Sentinel2Level1MetadataExtractor(loggerS2, resourceServiceProvider);
                stacFactory = await extractor.CreateSafeStacFactoryAsync(manifest, item, identifier);
            }
            else if (manifest.informationPackageMap.contentUnit[0].unitType == "Product_Level-2A")
            {
                var extractor = new Sentinel2Level2MetadataExtractor(loggerS2, resourceServiceProvider);
                stacFactory = await extractor.CreateSafeStacFactoryAsync(manifest, item, identifier);
            }
            else
            {
                throw new FormatException(string.Format("Not a Sentinel-2 manifest SAFE file asset"));
            }

            return stacFactory;
        }

        protected SentinelMetadataExtractor GetMatchingExtractorInstance(SentinelSafeStacFactory stacFactory)
        {
            if (stacFactory.Manifest.informationPackageMap.contentUnit[0].unitType == "Product_Level-1C")
            {
                return new Sentinel2Level1MetadataExtractor(loggerS2, resourceServiceProvider);
            }
            if (stacFactory.Manifest.informationPackageMap.contentUnit[0].unitType == "Product_Level-2A")
            {
                return new Sentinel2Level2MetadataExtractor(loggerS2, resourceServiceProvider);
            }
            return base.GetMatchingExtractorInstance(stacFactory);
        }

        // This method is not normally used, subclass method is called directly with the proper instance
        protected override Task AddAssets(StacItem stacItem, IItem item, string identifier, SentinelSafeStacFactory stacFactory)
        {
            if (!(GetMatchingExtractorInstance(stacFactory) is Sentinel2MetadataExtractor metadataExtractor)) return Task.CompletedTask;

            return metadataExtractor.AddAssets(stacItem, item, identifier, stacFactory);
        }

        // This method is not normally used, subclass method is called directly with the proper instance
        protected override Task AddAdditionalProperties(StacItem stacItem, IItem item, string identifier, SentinelSafeStacFactory stacFactory)
        {
            if (!(GetMatchingExtractorInstance(stacFactory) is Sentinel2MetadataExtractor metadataExtractor)) return Task.CompletedTask;

            Task task = metadataExtractor.AddAdditionalProperties(stacItem, item, identifier, stacFactory);

            if (IncludeProviderProperty)
            {
                AddSingleProvider(
                    stacItem.Properties,
                    "ESA/EC (Copernicus)",
                    "The Copernicus Sentinel-2 mission comprises a constellation of two polar-orbiting satellites placed in the same sun-synchronous orbit, phased at 180° to each other. It aims at monitoring variability in land surface conditions, and its wide swath width (290 km) and high revisit time (10 days at the equator with one satellite, and 5 days with 2 satellites under cloud-free conditions which results in 2-3 days at mid-latitudes) will support monitoring of Earth's surface changes.",
                    new StacProviderRole[] { StacProviderRole.producer, StacProviderRole.processor, StacProviderRole.licensor },
                    new Uri("https://sentinel.esa.int/web/sentinel/missions/sentinel-2")
                );
            }
            return task;
        }

        public static EoBandCommonName GetBandCommonName(OpenSearch.Sentinel.Data.Dimap.A_PRODUCT_INFO_USERL1CProduct_Image_CharacteristicsSpectral_Information spectralInfo)
        {
            switch (spectralInfo.physicalBand)
            {
                case OpenSearch.Sentinel.Data.Dimap.A_PHYSICAL_BAND_NAME.B1:
                    return EoBandCommonName.coastal;
                case OpenSearch.Sentinel.Data.Dimap.A_PHYSICAL_BAND_NAME.B2:
                    return EoBandCommonName.blue;
                case OpenSearch.Sentinel.Data.Dimap.A_PHYSICAL_BAND_NAME.B3:
                    return EoBandCommonName.green;
                case OpenSearch.Sentinel.Data.Dimap.A_PHYSICAL_BAND_NAME.B4:
                    return EoBandCommonName.red;
                case OpenSearch.Sentinel.Data.Dimap.A_PHYSICAL_BAND_NAME.B5:
                case OpenSearch.Sentinel.Data.Dimap.A_PHYSICAL_BAND_NAME.B6:
                case OpenSearch.Sentinel.Data.Dimap.A_PHYSICAL_BAND_NAME.B7:
                    return EoBandCommonName.rededge;
                case OpenSearch.Sentinel.Data.Dimap.A_PHYSICAL_BAND_NAME.B8:
                    return EoBandCommonName.nir;
                case OpenSearch.Sentinel.Data.Dimap.A_PHYSICAL_BAND_NAME.B8A:
                    return EoBandCommonName.nir08;
                case OpenSearch.Sentinel.Data.Dimap.A_PHYSICAL_BAND_NAME.B9:
                    return EoBandCommonName.nir09;
                case OpenSearch.Sentinel.Data.Dimap.A_PHYSICAL_BAND_NAME.B10:
                    return EoBandCommonName.cirrus;
                case OpenSearch.Sentinel.Data.Dimap.A_PHYSICAL_BAND_NAME.B11:
                    return EoBandCommonName.swir16;
                case OpenSearch.Sentinel.Data.Dimap.A_PHYSICAL_BAND_NAME.B12:
                    return EoBandCommonName.swir22;
                default:
                    return default(EoBandCommonName);
            }
        }

        public static string GetBandNameConvention(OpenSearch.Sentinel.Data.Dimap.A_PRODUCT_INFO_USERL1CProduct_Image_CharacteristicsSpectral_Information spectralInfo, int resolution, bool useCommonName = true)
        {
            string commonName = null;
            string bandName = null;
            int originalResolution = 0;
            switch (spectralInfo.physicalBand)
            {
                case OpenSearch.Sentinel.Data.Dimap.A_PHYSICAL_BAND_NAME.B1:
                    commonName = EoBandCommonName.coastal.ToString();
                    bandName = "B01";
                    originalResolution = 60;
                    break;
                case OpenSearch.Sentinel.Data.Dimap.A_PHYSICAL_BAND_NAME.B2:
                    commonName = EoBandCommonName.blue.ToString();
                    bandName = "B02";
                    originalResolution = 10;
                    break;
                case OpenSearch.Sentinel.Data.Dimap.A_PHYSICAL_BAND_NAME.B3:
                    commonName = EoBandCommonName.green.ToString();
                    bandName = "B03";
                    originalResolution = 10;
                    break;
                case OpenSearch.Sentinel.Data.Dimap.A_PHYSICAL_BAND_NAME.B4:
                    commonName = EoBandCommonName.red.ToString();
                    bandName = "B04";
                    originalResolution = 10;
                    break;
                case OpenSearch.Sentinel.Data.Dimap.A_PHYSICAL_BAND_NAME.B5:
                    commonName = "rededge70";
                    bandName = "B05";
                    originalResolution = 20;
                    break;
                case OpenSearch.Sentinel.Data.Dimap.A_PHYSICAL_BAND_NAME.B6:
                    commonName = "rededge74";
                    bandName = "B06";
                    originalResolution = 20;
                    break;
                case OpenSearch.Sentinel.Data.Dimap.A_PHYSICAL_BAND_NAME.B7:
                    commonName = "rededge78";
                    bandName = "B07";
                    originalResolution = 20;
                    break;
                case OpenSearch.Sentinel.Data.Dimap.A_PHYSICAL_BAND_NAME.B8:
                    commonName = EoBandCommonName.nir.ToString();
                    bandName = "B08";
                    originalResolution = 10;
                    break;
                case OpenSearch.Sentinel.Data.Dimap.A_PHYSICAL_BAND_NAME.B8A:
                    commonName = EoBandCommonName.nir08.ToString();
                    bandName = "B8A";
                    originalResolution = 20;
                    break;
                case OpenSearch.Sentinel.Data.Dimap.A_PHYSICAL_BAND_NAME.B9:
                    commonName = EoBandCommonName.nir09.ToString();
                    bandName = "B09";
                    originalResolution = 60;
                    break;
                case OpenSearch.Sentinel.Data.Dimap.A_PHYSICAL_BAND_NAME.B10:
                    commonName = EoBandCommonName.cirrus.ToString();
                    bandName = "B10";
                    originalResolution = 60;
                    break;
                case OpenSearch.Sentinel.Data.Dimap.A_PHYSICAL_BAND_NAME.B11:
                    commonName = EoBandCommonName.swir16.ToString();
                    bandName = "B11";
                    originalResolution = 20;
                    break;
                case OpenSearch.Sentinel.Data.Dimap.A_PHYSICAL_BAND_NAME.B12:
                    commonName = EoBandCommonName.swir22.ToString();
                    bandName = "B12";
                    originalResolution = 20;
                    break;
                default:
                    commonName = spectralInfo.bandId.ToString();
                    bandName = commonName;
                    break;
            }
            if (resolution == originalResolution) return commonName;
            return string.Format("{0}-{1}{2}", useCommonName ? commonName : bandName, resolution, useCommonName ? "m" : string.Empty);
        }
    }
}
