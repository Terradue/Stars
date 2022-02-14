using System;
using System.Collections.Generic;
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
using Terradue.OpenSearch.Sentinel.Data.Safe.Sentinel.S2.Level1;
using Terradue.OpenSearch.Sentinel.Data.Safe.Sentinel.S2.Level1.Granules;
using Terradue.Stars.Interface;
using Terradue.Stars.Services.Model.Stac;

namespace Terradue.Stars.Data.Model.Metadata.Sentinels.Sentinel2
{
    public class Sentinel2Level1MetadataExtractor : Sentinel2MetadataExtractor
    {
        public static XmlSerializer s2L1CProductSerializer = new XmlSerializer(typeof(Terradue.OpenSearch.Sentinel.Data.Safe.Sentinel.S2.Level1.Level1C_User_Product));
        public static XmlSerializer s2L1CProductTileSerializer = new XmlSerializer(typeof(Terradue.OpenSearch.Sentinel.Data.Safe.Sentinel.S2.Level1.Granules.Level1C_Tile));

        public Sentinel2Level1MetadataExtractor(ILogger<Sentinel2MetadataExtractor> logger) : base(logger)
        {
        }

        public override async Task<XFDUType> ReadManifest(IAsset manifestAsset)
        {
            XFDUType xfdu = await base.ReadManifest(manifestAsset);
            if (xfdu.informationPackageMap.contentUnit[0].textInfo.StartsWith("Sentinel-2", true, CultureInfo.InstalledUICulture)
                && xfdu.informationPackageMap.contentUnit[0].textInfo.ToLower().Contains("level-1c"))
            {
                return xfdu;
            }
            throw new FormatException(String.Format("Not a Sentinel-2 Level 1C manifest SAFE file asset"));
        }

        protected async override Task AddAssets(StacItem stacItem, IItem item, SentinelSafeStacFactory stacFactory)
        {
            var mtdAsset = FindFirstAssetFromFileNameRegex(item, "MTD_MSIL1C.xml$");
            if (mtdAsset == null)
                throw new FileNotFoundException("Product metadata file 'MTD_MSIL1C.xml' not found");
            var mtdtlAsset = FindFirstAssetFromFileNameRegex(item, "MTD_TL.xml$");
            Level1C_Tile mtdTile = null;
            if (mtdtlAsset != null)
                mtdTile = (Level1C_Tile)s2L1CProductTileSerializer.Deserialize(await mtdtlAsset.GetStreamable().GetStreamAsync());

            Level1C_User_Product level1C_User_Product = (Level1C_User_Product)s2L1CProductSerializer.Deserialize(await mtdAsset.GetStreamable().GetStreamAsync());
            StacAsset mtdStacAsset = StacAsset.CreateMetadataAsset(stacItem, mtdAsset.Uri, new ContentType(MimeTypes.GetMimeType(mtdAsset.Uri.ToString())));
            mtdStacAsset.Properties.AddRange(mtdAsset.Properties);
            stacItem.Assets.Add("mtd", mtdStacAsset);

            foreach (var bandAsset in FindAllAssetsFromFileNameRegex(item, "^(?!MSK).*.jp2$").OrderBy(a => Path.GetFileName(a.Value.Uri.ToString()), StringComparer.InvariantCultureIgnoreCase))
            {
                AddJp2BandAsset(stacItem, bandAsset.Value, item, level1C_User_Product, mtdTile);
            }

            stacItem.Assets.Add("manifest", CreateManifestAsset(stacItem, GetManifestAsset(item)).Value);

        }

        private string AddJp2BandAsset(StacItem stacItem, IAsset bandAsset, IItem item, Level1C_User_Product level1CUserProduct, Level1C_Tile? mtdTile)
        {
            StacAsset stacAsset = StacAsset.CreateDataAsset(stacItem, bandAsset.Uri,
                new System.Net.Mime.ContentType("image/jp2")
            );
            stacAsset.Properties.AddRange(bandAsset.Properties);
            string bandId = Path.GetFileNameWithoutExtension(bandAsset.Uri.ToString()).Split('_').Last();
            if (bandId == "PVI")
            {
                stacAsset.Roles.Clear();
                stacAsset.Roles.Add("overview");
                stacItem.Assets.Add(bandId, stacAsset);
                return bandId;
            }
            var spectralInfo = level1CUserProduct.General_Info.Product_Image_Characteristics.Spectral_Information_List.FirstOrDefault(si => si.physicalBand.ToString() == bandId.Replace("B0", "B"));
            if (spectralInfo != null)
            {
                stacAsset.SetProperty("gsd", (double)spectralInfo.RESOLUTION);
                EoBandObject eoBandObject = new EoBandObject(GetBandNameConvention(spectralInfo), GetBandCommonName(spectralInfo));
                eoBandObject.CenterWavelength = spectralInfo.Wavelength.CENTRAL.Value / 1000;
                eoBandObject.Description = string.Format("{0} {1}nm TOA {2}", GetBandCommonName(spectralInfo), Math.Round(spectralInfo.Wavelength.CENTRAL.Value), spectralInfo.RESOLUTION);
                var solarIrradiance = level1CUserProduct.General_Info.Product_Image_Characteristics.Reflectance_Conversion.Solar_Irradiance_List.FirstOrDefault(si => si.bandId == spectralInfo.bandId);
                if (solarIrradiance != null)
                    eoBandObject.SolarIllumination = solarIrradiance.Value;
                stacAsset.EoExtension().Bands = new EoBandObject[1] { eoBandObject };
                RasterBand rasterBand = new RasterBand();
                rasterBand.Nodata = 0;
                rasterBand.Statistics = new Stac.Common.Statistics(0, 10000, null, null, null);
                rasterBand.SpatialResolution = (double)spectralInfo.RESOLUTION;
                if ( mtdTile != null ){
                    
                    var size =  mtdTile.Geometric_Info.Tile_Geocoding.Size.First(s => s.resolution == spectralInfo.RESOLUTION);
                    stacAsset.ProjectionExtension().Shape = new int[2] { size.NCOLS, size.NROWS };
                }
                stacAsset.RasterExtension().Bands = new RasterBand[1] { rasterBand };
                stacAsset.Title = eoBandObject.Description = string.Format("{0} {1}nm TOA {2}", GetBandCommonName(spectralInfo), Math.Round(spectralInfo.Wavelength.CENTRAL.Value), spectralInfo.RESOLUTION);
            }
            
            stacAsset.Roles.Add("reflectance");
            stacItem.Assets.Add(bandId, stacAsset);

            return bandId;
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

        public static string GetBandNameConvention(OpenSearch.Sentinel.Data.Dimap.A_PRODUCT_INFO_USERL1CProduct_Image_CharacteristicsSpectral_Information spectralInfo)
        {
            switch (spectralInfo.physicalBand)
            {
                case OpenSearch.Sentinel.Data.Dimap.A_PHYSICAL_BAND_NAME.B1:
                    return EoBandCommonName.coastal.ToString();
                case OpenSearch.Sentinel.Data.Dimap.A_PHYSICAL_BAND_NAME.B2:
                    return EoBandCommonName.blue.ToString();
                case OpenSearch.Sentinel.Data.Dimap.A_PHYSICAL_BAND_NAME.B3:
                    return EoBandCommonName.green.ToString();
                case OpenSearch.Sentinel.Data.Dimap.A_PHYSICAL_BAND_NAME.B4:
                    return EoBandCommonName.red.ToString();
                case OpenSearch.Sentinel.Data.Dimap.A_PHYSICAL_BAND_NAME.B5:
                    return "rededge70";
                case OpenSearch.Sentinel.Data.Dimap.A_PHYSICAL_BAND_NAME.B6:
                    return "rededge74";
                case OpenSearch.Sentinel.Data.Dimap.A_PHYSICAL_BAND_NAME.B7:
                    return "rededge78";
                case OpenSearch.Sentinel.Data.Dimap.A_PHYSICAL_BAND_NAME.B8:
                    return EoBandCommonName.nir.ToString();
                case OpenSearch.Sentinel.Data.Dimap.A_PHYSICAL_BAND_NAME.B8A:
                    return EoBandCommonName.nir08.ToString();
                case OpenSearch.Sentinel.Data.Dimap.A_PHYSICAL_BAND_NAME.B9:
                    return EoBandCommonName.nir09.ToString();
                case OpenSearch.Sentinel.Data.Dimap.A_PHYSICAL_BAND_NAME.B10:
                    return EoBandCommonName.cirrus.ToString();
                case OpenSearch.Sentinel.Data.Dimap.A_PHYSICAL_BAND_NAME.B11:
                    return EoBandCommonName.swir16.ToString();
                case OpenSearch.Sentinel.Data.Dimap.A_PHYSICAL_BAND_NAME.B12:
                    return EoBandCommonName.swir22.ToString();
                default:
                    return spectralInfo.bandId.ToString();
            }
        }

        protected override SentinelSafeStacFactory CreateSafeStacFactory(XFDUType manifest, IItem item, string identifier)
        {
            return S2SafeStacFactory.Create(manifest, item, identifier);
        }
    }
}
