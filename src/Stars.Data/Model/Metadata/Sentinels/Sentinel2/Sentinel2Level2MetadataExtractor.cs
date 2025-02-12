﻿// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: Sentinel2Level2MetadataExtractor.cs

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using Stac;
using Stac.Extensions.Eo;
using Stac.Extensions.Projection;
using Stac.Extensions.Raster;
using Terradue.OpenSearch.Sentinel.Data.Safe;
using Terradue.OpenSearch.Sentinel.Data.Safe.Sentinel.S2.Level2;
using Terradue.OpenSearch.Sentinel.Data.Safe.Sentinel.S2.Level2.Granules;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Data.Model.Metadata.Sentinels.Sentinel2
{
    public class Sentinel2Level2MetadataExtractor : Sentinel2MetadataExtractor
    {
        public static XmlSerializer s2L2AUserProductSerializer0510 = new XmlSerializer(typeof(Terradue.OpenSearch.Sentinel.Data.Safe.Sentinel.S2.Level2.psd14.Level2A_User_Product));
        public static XmlSerializer s2L2AUserProductSerializer0511 = new XmlSerializer(typeof(Terradue.OpenSearch.Sentinel.Data.Safe.Sentinel.S2.Level2.psd15.Level2A_User_Product));

        public static XmlSerializer s2L2AProductTileSerializer0510 = new XmlSerializer(typeof(Terradue.OpenSearch.Sentinel.Data.Safe.Sentinel.S2.Level2.Granules.psd14.Level2A_Tile));
        public static XmlSerializer s2L2AProductTileSerializer0511 = new XmlSerializer(typeof(Terradue.OpenSearch.Sentinel.Data.Safe.Sentinel.S2.Level2.Granules.psd15.Level2A_Tile));

        private IAsset mtdAsset;
        private ILevel2A_User_Product level2A_User_Product;

        public Sentinel2Level2MetadataExtractor(ILogger<Sentinel2MetadataExtractor> logger, IResourceServiceProvider resourceServiceProvider) : base(logger, resourceServiceProvider)
        {
        }

        public override async Task<XFDUType> ReadManifest(IAsset manifestAsset)
        {
            XFDUType xfdu = await base.ReadManifest(manifestAsset);
            if (xfdu.informationPackageMap.contentUnit[0].textInfo.StartsWith("Sentinel-2", true, CultureInfo.InstalledUICulture)
                && xfdu.informationPackageMap.contentUnit[0].textInfo.ToLower().Contains("level-2a"))
            {
                return xfdu;
            }
            throw new FormatException(string.Format("Not a Sentinel-2 Level 2A manifest SAFE file asset"));

        }

        protected override async Task AddAssets(StacItem stacItem, IItem item, string identifier, SentinelSafeStacFactory stacFactory)
        {
            var mtdtlAsset = FindFirstAssetFromFileNameRegex(item, "MTD_TL.xml$");
            ILevel2A_Tile mtdTile = null;

            // Get correct serializer considering the product baseline
            XmlSerializer s2L2AProductTileSerializer = GetTileSerializer(identifier);

            if (mtdtlAsset != null)
                mtdTile = (ILevel2A_Tile)s2L2AProductTileSerializer.Deserialize(await resourceServiceProvider.GetAssetStreamAsync(mtdtlAsset, System.Threading.CancellationToken.None));

            await GetUserProduct(item, identifier);

            StacAsset mtdStacAsset = StacAsset.CreateMetadataAsset(stacItem, mtdAsset.Uri, new ContentType(MimeTypes.GetMimeType(mtdAsset.Uri.ToString())));
            mtdStacAsset.Properties.AddRange(mtdAsset.Properties);
            stacItem.Assets.Add("mtd", mtdStacAsset);

            foreach (var bandAsset in FindAllAssetsFromFileNameRegex(item, @"(?!MSK).*\.jp2$").OrderBy(a => Path.GetFileName(a.Value.Uri.ToString()), StringComparer.InvariantCultureIgnoreCase))
            {
                try
                {
                    var bandStacAsset = AddJp2BandAsset(stacItem, bandAsset.Value, item, level2A_User_Product, mtdTile);

                }
                catch (Exception)
                {
                    logger.LogWarning("asset {0} skipped : error during harvesting", bandAsset.Key);
                    continue;
                }
            }

            stacItem.Assets.Add("manifest", CreateManifestAsset(stacItem, GetManifestAsset(item)).Value);

        }

        protected override async Task AddAdditionalProperties(StacItem stacItem, IItem item, string identifier, SentinelSafeStacFactory stacFactory)
        {
            await GetUserProduct(item, identifier);
            stacItem.Properties.Add("processing_baseline", level2A_User_Product.General_Info.L2A_Product_Info.PROCESSING_BASELINE);
        }

        private string AddJp2BandAsset(StacItem stacItem, IAsset bandAsset, IItem item, ILevel2A_User_Product level2AUserProduct, ILevel2A_Tile mtdTile)
        {

            // checking if the jp2 is a MSK, if yes skip.
            string msk = Path.GetFileNameWithoutExtension(bandAsset.Uri.ToString()).Split('_')[0];
            if (msk == "MSK")
            {
                return string.Empty;
            }

            StacAsset stacAsset = StacAsset.CreateDataAsset(stacItem, bandAsset.Uri,
                new ContentType("image/jp2")
            );
            stacAsset.Properties.AddRange(bandAsset.Properties);

            string bandId = Path.GetFileNameWithoutExtension(bandAsset.Uri.ToString()).Split('_')[2];
            if (bandId == "PVI")
            {
                stacAsset.Roles.Clear();
                stacAsset.Roles.Add("overview");
                stacItem.Assets.Add(bandId, stacAsset);
                return bandId;
            }

            string res = Path.GetFileNameWithoutExtension(bandAsset.Uri.ToString()).Split('_')[3];
            double gsd = double.Parse(res.TrimEnd('m'));
            string assetName = bandId;
            var spectralInfo = level2AUserProduct.General_Info.Product_Image_Characteristics.Spectral_Information_List.FirstOrDefault(si => si.physicalBand.ToString() == bandId.Replace("B0", "B"));
            if (spectralInfo != null)
            {
                assetName = GetBandNameConvention(spectralInfo, Convert.ToInt32(gsd), false);
                string eoBandName = GetBandNameConvention(spectralInfo, Convert.ToInt32(gsd), true);
                EoBandObject eoBandObject = new EoBandObject(eoBandName, GetBandCommonName(spectralInfo));
                eoBandObject.CenterWavelength = spectralInfo.Wavelength.CENTRAL.Value / 1000;
                eoBandObject.Description = string.Format("{0} {1}nm BOA {2}", GetBandCommonName(spectralInfo), Math.Round(spectralInfo.Wavelength.CENTRAL.Value), res);
                var solarIrradiance = level2AUserProduct.General_Info.Product_Image_Characteristics.Reflectance_Conversion.Solar_Irradiance_List.FirstOrDefault(si => si.bandId == spectralInfo.bandId);
                if (solarIrradiance != null)
                    eoBandObject.SolarIllumination = solarIrradiance.Value;
                stacAsset.EoExtension().Bands = new EoBandObject[1] { eoBandObject };
                RasterBand rasterBand = new RasterBand();
                rasterBand.Nodata = 0;
                rasterBand.Statistics = new Stac.Common.Statistics(0, 10000, null, null, null);
                rasterBand.SpatialResolution = double.Parse(res.TrimEnd('m'));
                if (mtdTile != null)
                {

                    var size = mtdTile.Geometric_Info.Tile_Geocoding.Size.First(s => s.resolution == int.Parse(res.TrimEnd('m')));
                    stacAsset.ProjectionExtension().Shape = new int[2] { size.NCOLS, size.NROWS };
                }
                stacAsset.RasterExtension().Bands = new RasterBand[1] { rasterBand };
                stacAsset.Title = string.Format("{0} {1}nm BOA {2}", GetBandCommonName(spectralInfo), Math.Round(spectralInfo.Wavelength.CENTRAL.Value), res);
            }
            stacAsset.SetProperty("gsd", gsd);
            if (stacItem.Assets.ContainsKey(assetName)) assetName = bandId + "-" + gsd;
            stacAsset.Roles.Add("reflectance");
            stacItem.Assets.Add(assetName, stacAsset);
            return assetName;
        }

        protected override async Task<SentinelSafeStacFactory> CreateSafeStacFactoryAsync(XFDUType manifest, IItem item, string identifier)
        {
            var mtdtlAsset = FindFirstAssetFromFileNameRegex(item, "MTD_TL.xml$");
            ILevel2A_Tile mtdTile = null;

            // Get correct serializer considering the product baseline
            XmlSerializer s2L2AProductTileSerializer = GetTileSerializer(identifier);

            if (mtdtlAsset != null)
                mtdTile = (ILevel2A_Tile)s2L2AProductTileSerializer.Deserialize(await resourceServiceProvider.GetAssetStreamAsync(mtdtlAsset, System.Threading.CancellationToken.None));

            return S2L2SafeStacFactory.Create(manifest, item, identifier, mtdTile);
        }

        private async Task GetUserProduct(IItem item, string identifier)
        {
            mtdAsset = FindFirstAssetFromFileNameRegex(item, "MTD_MSIL2A.xml$");
            if (mtdAsset == null)
                throw new FileNotFoundException("Product metadata file 'MTD_MSIL2A.xml' not found");
            if (level2A_User_Product != null) return;

            // Get correct serializer considering the product baseline
            XmlSerializer s2L2AUserProductSerializer = GetUserProductSerializer(identifier);
            level2A_User_Product = (ILevel2A_User_Product)s2L2AUserProductSerializer.Deserialize(await resourceServiceProvider.GetAssetStreamAsync(mtdAsset, System.Threading.CancellationToken.None));
        }

        private XmlSerializer GetTileSerializer(string identifier)
        {
            string baseline = identifier.Substring(28, 4);
            if (string.Compare(baseline, "0511") >= 0) return s2L2AProductTileSerializer0511;
            return s2L2AProductTileSerializer0510;
        }

        private XmlSerializer GetUserProductSerializer(string identifier)
        {
            string baseline = identifier.Substring(28, 4);
            if (string.Compare(baseline, "0511") >= 0) return s2L2AUserProductSerializer0511;
            return s2L2AUserProductSerializer0510;
        }
    }
}
