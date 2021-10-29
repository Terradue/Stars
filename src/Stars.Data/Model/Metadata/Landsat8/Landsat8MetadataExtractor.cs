using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Humanizer;
using Kajabity.Tools.Java;
using Microsoft.Extensions.Logging;
using ProjNet.CoordinateSystems;
using Stac;
using Stac.Extensions.Eo;
using Stac.Extensions.Processing;
using Stac.Extensions.Projection;
using Stac.Extensions.Sar;
using Stac.Extensions.Sat;
using Stac.Extensions.View;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Services.Plugins;

namespace Terradue.Stars.Data.Model.Metadata.Landsat8
{

    public class Auxiliary : JavaProperties
    {
        public new string GetProperty(string key)
        {
            var r = base.GetProperty(key);
            if (!string.IsNullOrEmpty(r)) r = r.Trim('"');
            return r;
        }

        public bool IsLandsat8()
        {
            return SpaceCraftID == "LANDSAT_8";
        }
        //properties
        public string SpaceCraftID => GetProperty("SPACECRAFT_ID");
        public string SceneId => GetProperty("LANDSAT_SCENE_ID");
        public string CreationDateTime => GetProperty("FILE_DATE") ?? GetProperty("DATE_PRODUCT_GENERATED");
        public string AcquisitionDateTime => GetProperty("DATE_ACQUIRED") + "T" + GetProperty("SCENE_CENTER_TIME");

        //platform
        public string[] Instruments => GetProperty("SENSOR_ID").ToLower().Split('_');
        public string CollectionNumber => GetProperty("COLLECTION_NUMBER");

        public string Platform
        {
            get
            {
                return SpaceCraftID.ToLower().Hyphenate();
            }
        }
        public double GSD
        {
            get
            {
                var cells = new List<double>{
                    double.Parse(GetProperty("GRID_CELL_SIZE_PANCHROMATIC")),
                    double.Parse(GetProperty("GRID_CELL_SIZE_REFLECTIVE")),
                    double.Parse(GetProperty("GRID_CELL_SIZE_THERMAL"))
                };
                return cells.Min();
            }
        }

        // projection
        public string MapProjection => GetProperty("MAP_PROJECTION");
        public string Orientation => GetProperty("ORIENTATION");
        public int UTM_zone => int.Parse(GetProperty("UTM_ZONE"));

        // orbit
        public int OrbitNumber => int.Parse(GetProperty("WRS_PATH"));
        public int Row => int.Parse(GetProperty("WRS_ROW"));
        public int Path => int.Parse(GetProperty("WRS_PATH"));
        public readonly string OrbitDirection = "descending";

        // images attributes
        public double SunAzimuth => double.Parse(GetProperty("SUN_AZIMUTH"));
        public double SunElevation => double.Parse(GetProperty("SUN_ELEVATION"));
        public readonly double OffNadirAngle = 0;
        internal string ProductId => GetProperty("LANDSAT_PRODUCT_ID") ?? GetProperty("METADATA_FILE_NAME").Replace("_MTL.txt", "");

        public string CollectionCategory => GetProperty("COLLECTION_CATEGORY");

        public double IncidenceAngle
        {
            get
            {
                switch (GetProperty("NADIR_OFFNADIR"))
                {
                    case "NADIR":
                        return 0;
                    case "OFF_NADIR":
                        return double.Parse(GetProperty("ROLL_ANGLE"));
                    default:
                        return 0;
                }
            }
        }

        // EO
        public double CloudCover
        {
            get
            {
                try
                {
                    return double.Parse(GetProperty("CLOUD_COVER"));
                }
                catch (Exception)
                {
                    return 0;
                };
            }
        }

        public double CloudCoverLand
        {
            get
            {
                try
                {
                    return double.Parse(GetProperty("CLOUD_COVER_LAND"));
                }
                catch (Exception)
                {
                    return 0;
                };
            }
        }

        // processing
        public string DataType => GetProperty("DATA_TYPE");

        //Geometry
        public double LowerLeftCoordinateLat => double.Parse(GetProperty("CORNER_LL_LAT_PRODUCT"));
        public double LowerLeftCoordinateLon => double.Parse(GetProperty("CORNER_LL_LON_PRODUCT"));
        public double LowerRightCoordinateLat => double.Parse(GetProperty("CORNER_LR_LAT_PRODUCT"));
        public double LowerRightCoordinateLon => double.Parse(GetProperty("CORNER_LR_LON_PRODUCT"));
        public double UpperLeftCoordinateLat => double.Parse(GetProperty("CORNER_UL_LAT_PRODUCT"));
        public double UpperLeftCoordinateLon => double.Parse(GetProperty("CORNER_UL_LON_PRODUCT"));
        public double UpperRightCoordinateLat => double.Parse(GetProperty("CORNER_UR_LAT_PRODUCT"));
        public double UpperRightCoordinateLon => double.Parse(GetProperty("CORNER_UR_LON_PRODUCT"));

        // Band
        public string Filename_Band1 => GetProperty("FILE_NAME_BAND_1");
        public string Filename_Band2 => GetProperty("FILE_NAME_BAND_2");
        public string Filename_Band3 => GetProperty("FILE_NAME_BAND_3");
        public string Filename_Band4 => GetProperty("FILE_NAME_BAND_4");
        public string Filename_Band5 => GetProperty("FILE_NAME_BAND_5");
        public string Filename_Band6 => GetProperty("FILE_NAME_BAND_6");
        public string Filename_Band7 => GetProperty("FILE_NAME_BAND_7");
        public string Filename_Band8 => GetProperty("FILE_NAME_BAND_8");
        public string Filename_Band9 => GetProperty("FILE_NAME_BAND_9");
        public string Filename_Band10 => GetProperty("FILE_NAME_BAND_10");
        public string Filename_Band11 => GetProperty("FILE_NAME_BAND_11");

    }

    [PluginPriority(1000)]
    public class Landsat8MetadataExtraction : MetadataExtraction
    {

        public override string Label => "Landsat-8 (NASA) mission product metadata extractor";

        private const string ASCENDING = "Ascending Orbit";
        private const string DESCENDING = "Descending Orbit";

        public Landsat8MetadataExtraction(ILogger<Landsat8MetadataExtraction> logger) : base(logger)
        {
        }

        protected override async Task<StacNode> ExtractMetadata(IItem item, string suffix)
        {
            logger.LogDebug("Retrieving the metadata file in the product package");
            IAsset auxFile = FindFirstAssetFromFileNameRegex(item, "[0-9a-zA-Z_-]*(_MTL\\.txt)$");
            if (auxFile == null)
            {
                throw new FileNotFoundException(String.Format("Unable to find the metadata file asset"));
            }
            logger.LogDebug(String.Format("Metadata file is {0}", auxFile.Uri));

            IStreamable auxFileStreamable = auxFile.GetStreamable();
            if (auxFileStreamable == null)
            {
                logger.LogError("metadata file asset is not streamable, skipping metadata extraction");
                return null;
            }

            logger.LogDebug("Deserializing metadata");
            Auxiliary auxiliary = await DeserializeAuxiliary(auxFileStreamable);

            logger.LogDebug("Metadata deserialized. Starting metadata generation");

            StacItem stacItem = new StacItem(auxiliary.ProductId,
                                                GetGeometry(auxiliary),
                                                GetCommonMetadata(auxiliary));
            stacItem.SetCollection("landsat-c1l1", new Uri("../landsat8.json", UriKind.Relative));
            AddEoStacExtension(auxiliary, stacItem);
            AddSatStacExtension(auxiliary, stacItem);
            AddLandsatStacExtension(auxiliary, stacItem);
            AddProjStacExtension(auxiliary, stacItem);
            AddViewStacExtension(auxiliary, stacItem);
            AddProcessingStacExtension(auxiliary, stacItem);
            AddOtherProperties(auxiliary, stacItem);

            AddAssets(stacItem, auxiliary, item);

            // AddEoBandPropertyInItem(stacItem);

            return StacItemNode.CreateUnlocatedNode(stacItem);

        }

        private void AddProcessingStacExtension(Auxiliary auxiliary, StacItem stacItem)
        {
            var proc = stacItem.ProcessingExtension();
            proc.Level = auxiliary.GetProperty("PROCESSING_LEVEL") ?? auxiliary.DataType;
        }

        private void AddEoBandPropertyInItem(StacItem stacItem)
        {
            var eo = stacItem.EoExtension();

            eo.Bands = stacItem.Assets.Values.Where(a => a.EoExtension().Bands != null).SelectMany(a => a.EoExtension().Bands).ToArray();
        }

        private void AddAssets(StacItem stacItem, Auxiliary auxiliary, IAssetsContainer assetsContainer)
        {
            foreach (var asset in assetsContainer.Assets.OrderBy(a => a.Key))
            {
                AddAsset(stacItem, auxiliary, asset.Value);
            }
        }

        private void AddAsset(StacItem stacItem, Auxiliary auxiliary, IAsset asset)
        {
            string filename = Path.GetFileName(asset.Uri.ToString());
            if (filename.EndsWith("_B1.TIF", true, CultureInfo.InvariantCulture) || filename.EndsWith("_B01.TIF", true, CultureInfo.InvariantCulture))
                AddbandAsset(stacItem, "B1", asset, EoBandCommonName.coastal, 0.44, 0.02, 30);
            if (filename.EndsWith("_B2.TIF", true, CultureInfo.InvariantCulture) || filename.EndsWith("_B02.TIF", true, CultureInfo.InvariantCulture))
                AddbandAsset(stacItem, "B2", asset, EoBandCommonName.blue, 0.48, 0.06, 30);
            if (filename.EndsWith("_B3.TIF", true, CultureInfo.InvariantCulture) || filename.EndsWith("_B03.TIF", true, CultureInfo.InvariantCulture))
                AddbandAsset(stacItem, "B3", asset, EoBandCommonName.green, 0.56, 0.06, 30);
            if (filename.EndsWith("_B4.TIF", true, CultureInfo.InvariantCulture) || filename.EndsWith("_B04.TIF", true, CultureInfo.InvariantCulture))
                AddbandAsset(stacItem, "B4", asset, EoBandCommonName.red, 0.65, 0.04, 30);
            if (filename.EndsWith("_B5.TIF", true, CultureInfo.InvariantCulture) || filename.EndsWith("_B05.TIF", true, CultureInfo.InvariantCulture))
                AddbandAsset(stacItem, "B5", asset, EoBandCommonName.nir, 0.86, 0.03, 30);
            if (filename.EndsWith("_B6.TIF", true, CultureInfo.InvariantCulture) || filename.EndsWith("_B06.TIF", true, CultureInfo.InvariantCulture))
                AddbandAsset(stacItem, "B6", asset, EoBandCommonName.swir16, 1.6, 0.08, 30);
            if (filename.EndsWith("_B7.TIF", true, CultureInfo.InvariantCulture) || filename.EndsWith("_B07.TIF", true, CultureInfo.InvariantCulture))
                AddbandAsset(stacItem, "B7", asset, EoBandCommonName.swir22, 2.2, 0.2, 30);
            if (filename.EndsWith("_B8.TIF", true, CultureInfo.InvariantCulture) || filename.EndsWith("_B08.TIF", true, CultureInfo.InvariantCulture))
                AddbandAsset(stacItem, "B8", asset, EoBandCommonName.pan, 0.59, 0.18, 15);
            if (filename.EndsWith("_B9.TIF", true, CultureInfo.InvariantCulture) || filename.EndsWith("_B09.TIF", true, CultureInfo.InvariantCulture))
                AddbandAsset(stacItem, "B9", asset, EoBandCommonName.cirrus, 1.37, 0.02, 30);
            if (filename.EndsWith("_B10.TIF", true, CultureInfo.InvariantCulture))
                AddbandAsset(stacItem, "B10", asset, EoBandCommonName.lwir11, 10.9, 0.8, 100);
            if (filename.EndsWith("_B11.TIF", true, CultureInfo.InvariantCulture))
                AddbandAsset(stacItem, "B11", asset, EoBandCommonName.lwir12, 12, 1, 100);
            if (filename.EndsWith("_BQA.TIF", true, CultureInfo.InvariantCulture))
                stacItem.Assets.Add("BQA", GetGenericAsset(stacItem, asset, "BQA"));
            if (filename.EndsWith("_MTL.txt", true, CultureInfo.InvariantCulture))
                stacItem.Assets.Add("metadata", GetGenericAsset(stacItem, asset, "metadata"));
            if (filename.EndsWith("_ANG.txt", true, CultureInfo.InvariantCulture))
                stacItem.Assets.Add("angle_coefficient", GetGenericAsset(stacItem, asset, "metadata"));
        }

        private void AddbandAsset(StacItem stacItem, string assetKey, IAsset asset, EoBandCommonName common_name, double center_wavelength, double full_width_half_max, double gsd)
        {
            StacAsset stacAsset = StacAsset.CreateDataAsset(stacItem, asset.Uri,
                new System.Net.Mime.ContentType(MimeTypes.GetMimeType(asset.Uri.ToString()))
            );
            stacAsset.Properties.AddRange(asset.Properties);
            var eo = stacItem.EoExtension();

            EoBandObject eoBandObject = new EoBandObject(assetKey, common_name);
            eoBandObject.CenterWavelength = center_wavelength;
            eoBandObject.FullWidthHalfMax = full_width_half_max;

            stacAsset.SetProperty("gsd", gsd);
            stacAsset.EoExtension().Bands = new EoBandObject[1] { eoBandObject };
            stacItem.Assets.Add(assetKey, stacAsset);
        }

        private StacAsset GetGenericAsset(StacItem stacItem, IAsset asset, string role)
        {
            StacAsset stacAsset = new StacAsset(stacItem, asset.Uri);
            stacAsset.Properties.AddRange(asset.Properties);
            stacAsset.Roles.Add(role);
            stacAsset.MediaType = new System.Net.Mime.ContentType(MimeTypes.GetMimeType(Path.GetFileName(asset.Uri.ToString())));
            return stacAsset;
        }

        private void AddOtherProperties(Auxiliary auxiliary, StacItem stacItem)
        {
            stacItem.Properties.Add("product_type", auxiliary.DataType);
            FillBasicsProperties(auxiliary, stacItem.Properties);
        }

        private void AddViewStacExtension(Auxiliary auxiliary, StacItem stacItem)
        {
            var view = new ViewStacExtension(stacItem);
            view.OffNadir = auxiliary.OffNadirAngle;
            view.IncidenceAngle = auxiliary.IncidenceAngle;
            // view.Azimuth = auxiliary.Azimuth;
            view.SunAzimuth = auxiliary.SunAzimuth;
            view.SunElevation = auxiliary.SunElevation;
        }

        private void AddSatStacExtension(Auxiliary auxiliary, StacItem stacItem)
        {
            var sat = new SatStacExtension(stacItem);
            sat.AbsoluteOrbit = auxiliary.OrbitNumber;
            sat.RelativeOrbit = auxiliary.OrbitNumber;
            sat.OrbitState = auxiliary.OrbitDirection;
        }

        private void AddLandsatStacExtension(Auxiliary auxiliary, StacItem stacItem)
        {
            stacItem.StacExtensions.Add("https://landsat.usgs.gov/stac/landsat-extension/v1.1.0/schema.json");
            stacItem.SetProperty("landsat:wrs_type", 2);
            stacItem.SetProperty("landsat:wrs_row", auxiliary.Row);
            stacItem.SetProperty("landsat:wrs_path", auxiliary.Path);
            stacItem.SetProperty("landsat:scene_id", auxiliary.SceneId);
            stacItem.SetProperty("landsat:collection_number", auxiliary.CollectionNumber);
            stacItem.SetProperty("landsat:collection_category", auxiliary.CollectionCategory);
            stacItem.SetProperty("landsat:cloud_cover_land", auxiliary.CloudCoverLand);
            
        }

        private void AddEoStacExtension(Auxiliary auxiliary, StacItem stacItem)
        {
            EoStacExtension eo = stacItem.EoExtension();
            eo.CloudCover = auxiliary.CloudCover;
        }

        private void AddProjStacExtension(Auxiliary auxiliary, StacItem stacItem)
        {
            ProjectionStacExtension proj = stacItem.ProjectionExtension();
            switch (auxiliary.MapProjection)
            {
                case "UTM":
                    int zone = auxiliary.UTM_zone;
                    bool north = auxiliary.Orientation[0] == 'N';
                    ProjectedCoordinateSystem utm = ProjectedCoordinateSystem.WGS84_UTM(zone, north);
                    proj.SetCoordinateSystem(utm);
                    break;
            }
        }

        private IDictionary<string, object> GetCommonMetadata(Auxiliary auxiliary)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();

            FillDateTimeProperties(auxiliary, properties);
            // TODO Licensing
            // TODO Provider
            FillInstrument(auxiliary, properties);


            return properties;
        }

        private void FillInstrument(Auxiliary auxiliary, Dictionary<string, object> properties)
        {
            // platform & constellation
            properties.Remove("platform");
            properties.Add("platform", auxiliary.Platform.ToLower());

            // platform & constellation
            properties.Remove("mission");
            properties.Add("mission", auxiliary.Platform.ToLower());

            // instruments            
            properties.Remove("instruments");
            properties.Add("instruments", Array.ConvertAll(auxiliary.Instruments, i => i.ToLower()));

            properties["sensor_type"] = "optical";

            // GSD            
            properties.Remove("gsd");
            properties.Add("gsd", auxiliary.GSD);


        }

        private void FillDateTimeProperties(Auxiliary auxiliary, Dictionary<string, object> properties)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            string format = "yyyy-MM-ddTHH:mm:ssZ";
            DateTime createdDate = DateTime.MinValue;
            DateTime.TryParseExact(auxiliary.CreationDateTime, format, provider, DateTimeStyles.AssumeUniversal, out createdDate);


            // remove previous values
            properties.Remove("created");
            properties.Add("created", createdDate.ToUniversalTime());

            format = "yyyy-MM-ddTHH:mm:ss.fffffffZ";
            DateTime acquiredDate = DateTime.MinValue;
            DateTime.TryParseExact(auxiliary.AcquisitionDateTime, format, provider, DateTimeStyles.AssumeUniversal, out acquiredDate);

            properties.Remove("datetime");
            properties.Add("datetime", acquiredDate.ToUniversalTime());
            properties.Remove("start_datetime");
            properties.Remove("end_datetime");

            properties.Remove("updated");
            properties.Add("updated", DateTime.UtcNow);
        }

        private void FillBasicsProperties(Auxiliary auxiliary, IDictionary<string, object> properties)
        {
            CultureInfo culture = new CultureInfo("fr-FR");
            // title
            properties.Remove("title");
            properties.Add("title", string.Format("{0} {1} {2} {3}",
                                                  StylePlatform(properties.GetProperty<string>("platform")),
                                                  string.Join(" ", properties.GetProperty<string[]>("instruments")).ToUpper(),
                                                  properties.GetProperty<string>("processing:level")?.ToUpper(),
                                                  properties.GetProperty<DateTime>("datetime").ToUniversalTime().ToString("G", culture)));
        }

        private GeoJSON.Net.Geometry.IGeometryObject GetGeometry(Auxiliary auxiliary)
        {
            var ul = new GeoJSON.Net.Geometry.Position(auxiliary.UpperLeftCoordinateLat, auxiliary.UpperLeftCoordinateLon);
            var ur = new GeoJSON.Net.Geometry.Position(auxiliary.UpperRightCoordinateLat, auxiliary.UpperRightCoordinateLon);
            var ll = new GeoJSON.Net.Geometry.Position(auxiliary.LowerLeftCoordinateLat, auxiliary.LowerLeftCoordinateLon);
            var lr = new GeoJSON.Net.Geometry.Position(auxiliary.LowerRightCoordinateLat, auxiliary.LowerRightCoordinateLon);

            GeoJSON.Net.Geometry.LineString lineString = new GeoJSON.Net.Geometry.LineString(
                new GeoJSON.Net.Geometry.Position[5] { ul, ur, lr, ll, ul }
            );

            return new GeoJSON.Net.Geometry.Polygon(new GeoJSON.Net.Geometry.LineString[] { lineString });
        }


        /// <summary>Deserialize Auxiliary from xml to class</summary>
        /// <param name="auxiliaryFile">The <see cref="StreamWrapper"/> instance linked to the metadata file.</param>
        /// <returns>The deserialized metadata object.</returns>
        public static async Task<Auxiliary> DeserializeAuxiliary(IStreamable auxiliaryFile)
        {
            Auxiliary auxiliary = new Auxiliary();
            using (var stream = await auxiliaryFile.GetStreamAsync())
            {
                auxiliary.Load(stream);
            }
            return auxiliary;
        }

        public override bool CanProcess(IResource route, IDestination destinations)
        {
            IItem item = route as IItem;
            if (item == null) return false;
            IAsset auxFile = FindFirstAssetFromFileNameRegex(item, "[0-9a-zA-Z_-]*(MTL\\.txt)$");
            if (auxFile == null)
            {
                return false;
            }

            try
            {
                var auxiliary = DeserializeAuxiliary(auxFile.GetStreamable()).GetAwaiter().GetResult();
                return auxiliary.IsLandsat8();
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }


    }

}