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
using Terradue.OpenSearch.Sentinel.Data.Safe.Sentinel.S2.Level2;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Data.Model.Metadata.Sentinels.Sentinel2
{
    public class Sentinel2Level2MetadataExtractor : Sentinel2MetadataExtractor
    {
        public static XmlSerializer s2L2AProductSerializer = new XmlSerializer(typeof(Terradue.OpenSearch.Sentinel.Data.Safe.Sentinel.S2.Level2.Level2A_User_Product));
        public static XmlSerializer s2L2AProductTileSerializer = new XmlSerializer(typeof(Level2A_Tile));


        public Sentinel2Level2MetadataExtractor(ILogger<Sentinel2MetadataExtractor> logger) : base(logger)
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
            throw new FormatException(String.Format("Not a Sentinel-2 Level 2A manifest SAFE file asset"));

        }

        protected async override Task AddAssets(StacItem stacItem, IItem item, SentinelSafeStacFactory stacFactory)
        {
            var mtdAsset = FindFirstAssetFromFileNameRegex(item, "MTD_MSIL2A.xml$");
            if (mtdAsset == null)
                throw new FileNotFoundException("Product metadata file 'MTD_MSIL2A.xml' not found");
            var mtdtlAsset = FindFirstAssetFromFileNameRegex(item, "MTD_TL.xml$");
            Level2A_Tile mtdTile = null;
            if (mtdtlAsset != null)
                mtdTile = (Level2A_Tile)s2L2AProductTileSerializer.Deserialize(await mtdtlAsset.GetStreamable().GetStreamAsync());

            Level2A_User_Product Level2A_User_Product = (Level2A_User_Product)s2L2AProductSerializer.Deserialize(await mtdAsset.GetStreamable().GetStreamAsync());
            StacAsset mtdStacAsset = StacAsset.CreateMetadataAsset(stacItem, mtdAsset.Uri, new ContentType(MimeTypes.GetMimeType(mtdAsset.Uri.ToString())));
            mtdStacAsset.Properties.AddRange(mtdAsset.Properties);
            stacItem.Assets.Add("mtd", mtdStacAsset);

            foreach (var bandAsset in FindAllAssetsFromFileNameRegex(item, @"(?!MSK).*\.jp2$").OrderBy(a => Path.GetFileName(a.Value.Uri.ToString()), StringComparer.InvariantCultureIgnoreCase))
            {
                try
                {
                    var bandStacAsset = AddJp2BandAsset(stacItem, bandAsset.Value, item, Level2A_User_Product, mtdTile);

                }
                catch (Exception)
                {
                    logger.LogWarning("asset {0} skipped : error during harvesting", bandAsset.Key);
                    continue;
                }
            }

            stacItem.Assets.Add("manifest", CreateManifestAsset(stacItem, GetManifestAsset(item)).Value);

        }

        private string AddJp2BandAsset(StacItem stacItem, IAsset bandAsset, IItem item, Level2A_User_Product level2AUserProduct, Level2A_Tile? mtdTile)
        {

            // checking if the jp2 is a MSK, if yes skip.
            string msk = Path.GetFileNameWithoutExtension(bandAsset.Uri.ToString()).Split('_')[0];
            if (msk == "MSK")
            {
                return String.Empty;
            }

            StacAsset stacAsset = StacAsset.CreateDataAsset(stacItem, bandAsset.Uri,
                new System.Net.Mime.ContentType("image/jp2")
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
            string assetName = bandId;
            var spectralInfo = level2AUserProduct.General_Info.Product_Image_Characteristics.Spectral_Information_List.FirstOrDefault(si => si.physicalBand.ToString() == bandId.Replace("B0", "B"));
            if (spectralInfo != null)
            {
                assetName = Sentinel2Level1MetadataExtractor.GetBandNameConvention(spectralInfo);
                EoBandObject eoBandObject = new EoBandObject(assetName + "-" + res, Sentinel2Level1MetadataExtractor.GetBandCommonName(spectralInfo));
                eoBandObject.CenterWavelength = spectralInfo.Wavelength.CENTRAL.Value / 1000;
                eoBandObject.Description = string.Format("{0} {1}nm BOA {2}", Sentinel2Level1MetadataExtractor.GetBandCommonName(spectralInfo), Math.Round(spectralInfo.Wavelength.CENTRAL.Value), res);
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
                stacAsset.Title = string.Format("{0} {1}nm BOA {2}", Sentinel2Level1MetadataExtractor.GetBandCommonName(spectralInfo), Math.Round(spectralInfo.Wavelength.CENTRAL.Value), res);
            }
            stacAsset.SetProperty("gsd", double.Parse(res.TrimEnd('m')));
            if (stacItem.Assets.ContainsKey(assetName))
            {
                assetName = bandId + "-" + stacAsset.GetProperty<double>("gsd").ToString();
            }
            stacAsset.Roles.Add("reflectance");
            stacItem.Assets.Add(assetName, stacAsset);
            return assetName;
        }

        protected override SentinelSafeStacFactory CreateSafeStacFactory(XFDUType manifest, IItem item, string identifier)
        {
            return S2SafeStacFactory.Create(manifest, item, identifier);
        }
    }
}
