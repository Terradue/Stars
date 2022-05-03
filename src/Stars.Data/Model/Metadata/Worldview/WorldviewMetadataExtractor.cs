using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Kajabity.Tools.Java;
using Microsoft.Extensions.Logging;
using Stac;
using Stac.Extensions.Eo;
using Stac.Extensions.Processing;
using Stac.Extensions.View;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Model.Stac;
using Stac.Extensions.Raster;
using Terradue.Stars.Services;
using Terradue.Stars.Geometry.GeoJson;
using Stac.Extensions.Projection;

namespace Terradue.Stars.Data.Model.Metadata.Worldview
{
    public class WorldviewMetadataExtractor : MetadataExtraction
    {
        public override string Label => "WorldView (DigitalGlobe) contellation product metadata extractor";

        public WorldviewMetadataExtractor(ILogger<WorldviewMetadataExtractor> logger) : base(logger) { }

        public override bool CanProcess(IResource route, IDestination destination)
        {
            IItem item = route as IItem;
            if (item == null) return false;
            IAsset metadata = FindFirstAssetFromFileNameRegex(item, "^(WV)[0-9a-zA-Z_-]*(\\.txt)$");


            IAsset isdMetadata = FindFirstAssetFromFileNameRegex(item, "[0-9a-zA-Z_-]*(\\.XML)$");

            if (metadata == null || isdMetadata == null)
            {
                return false;
            }

            return true;
        }

        protected override async Task<StacNode> ExtractMetadata(IItem item, string suffix)
        {
            logger.LogDebug("Retrieving the metadata file in the product package");
            IAsset metadataFile = FindFirstAssetFromFileNameRegex(item, "^(WV)[0-9a-zA-Z_-]*(\\.txt)$");
            if (metadataFile == null)
            {
                throw new FileNotFoundException(String.Format("Unable to find the metadata file asset"));
            }

            logger.LogDebug(String.Format("Metadata file is {0}", metadataFile.Uri));


            //  loading properties in dictionary
            IAsset isdMetadataFile = FindFirstAssetFromFileNameRegex(item, "[0-9a-zA-Z_-]*(\\.XML)$");
            IStreamResource isdMetadataFileStreamable = isdMetadataFile.GetStreamable();
            if (isdMetadataFileStreamable == null)
            {
                logger.LogError("metadata file asset is not streamable, skipping metadata extraction");
                return null;
            }

            logger.LogDebug("Deserializing metadata files");
            var metadata = new JavaProperties();
            metadata.Load(await metadataFile.GetStreamable().GetStreamAsync());

            Isd isdMetadata = await DeserializeProductMetadata(isdMetadataFileStreamable);
            logger.LogDebug("Metadata files deserialized. Starting metadata generation");


            // retrieving GeometryObject from metadata
            var geometryObject = GetGeometryObjectFromProductMetadata(metadata);

            // geometry and instruments
            var commonMetadata = GetCommonMetadata(metadata);

            // stac item id
            string stacItemId = metadata["ENTITY_ID"];

            // initializing the stac item object
            StacItem stacItem = new StacItem(stacItemId, geometryObject, commonMetadata);

            AddEoStacExtension(isdMetadata, stacItem);
            AddViewStacExtension(isdMetadata, stacItem);
            AddProjStacExtension(isdMetadata, stacItem);
            AddProcessingStacExtension(metadata, stacItem);

            FillBasicsProperties(metadata, stacItem.Properties);

            AddAssets(stacItem, metadata, isdMetadata, item);

            // AddEoBandPropertyInItem(stacItem);
            return StacItemNode.Create(stacItem, item.Uri);
        }

        private void AddProcessingStacExtension(JavaProperties metadata, StacItem stacItem)
        {
            var proc = stacItem.ProcessingExtension();
            proc.Level = metadata["VENDOR_PROCESS_LEVEL"];
        }

        private void AddAssets(StacItem stacItem,
                                JavaProperties metadata,
                                Isd isdMetadata,
                                IAssetsContainer assetsContainer)
        {
            foreach (var asset in assetsContainer.Assets.Values.OrderBy(a => a.Uri.ToString()))
            {
                AddAsset(stacItem, metadata, isdMetadata, asset);
            }

        }

        private StacAsset GetGenericAsset(StacItem stacItem, IAsset asset, string[] roles)
        {
            StacAsset stacAsset = new StacAsset(stacItem, asset.Uri);
            foreach (var role in roles)
            {
                stacAsset.Roles.Add(role);
            }
            stacAsset.Properties.AddRange(asset.Properties);
            stacAsset.MediaType =
                new System.Net.Mime.ContentType(MimeTypes.GetMimeType(Path.GetFileName(asset.Uri.ToString())));
            return stacAsset;
        }


        private void AddAsset(StacItem stacItem,
                                JavaProperties metadata,
                                Isd isdMetadata,
                                IAsset asset)
        {
            string filename = Path.GetFileName(asset.Uri.ToString());

            // thumbnail
            if (filename.EndsWith(".XML", true, CultureInfo.InvariantCulture)){
                stacItem.Assets.Add("manifest",
                    GetGenericAsset(stacItem, asset, new[] { "metadata" }));
            }


            Regex rgx = new Regex("^(WV)[0-9a-zA-Z_-]*(\\.txt)$");
            if (rgx.Match(filename).Success)
            {
                stacItem.Assets.Add("metadata",
                    GetGenericAsset(stacItem, asset, new[] { "metadata" }));
            }

            rgx = new Regex("^(WV)[0-9a-zA-Z_-]*(\\.jpg)$");
            if (rgx.Match(filename).Success)
            {
                stacItem.Assets.TryAdd("overview",
                    GetGenericAsset(stacItem, asset, new[] { "overview" }));
            }


            if (filename.EndsWith(".tif", true, CultureInfo.InvariantCulture))
            {
                string bandName = metadata["SENSOR_TYPE"].ToLower();
                AddRasterAsset(stacItem, asset, metadata, isdMetadata);
            }
        }


        private void AddRasterAsset(StacItem stacItem, IAsset asset, JavaProperties metadata, Isd isdMetadata)
        {
            string bandName = metadata["SENSOR_TYPE"].ToLower();
            string platformNumber = metadata["PLATFORM_NUMBER"];
            StacAsset stacAsset = StacAsset.CreateDataAsset(stacItem, asset.Uri,
                new System.Net.Mime.ContentType(MimeTypes.GetMimeType(asset.Uri.ToString()))
            );
            stacAsset.Properties.AddRange(asset.Properties);
            stacAsset.Roles.Add("dn");
            stacAsset.SetProperty("gsd", double.Parse(metadata["PIXEL_SIZE_X"]));

            stacAsset.ProjectionExtension().Shape = new int[] { isdMetadata.TIL.TILESIZEY, isdMetadata.TIL.TILESIZEX };

            // WORLDVIEW-1
            if (platformNumber == "1")
            {
                List<EoBandObject> eoBandObjects = new List<EoBandObject>();
                List<RasterBand> rasterBandObjects = new List<RasterBand>();
                if (isdMetadata.IMD.BAND_P != null)
                {
                    eoBandObjects.Add(
                    CreateEoBandObject(EoBandCommonName.pan.ToString(),
                                                   EoBandCommonName.pan,
                                                   0.6322,
                                                   0.2846 / 2,
                                                   1478.62)
                    );
                    rasterBandObjects.Add(
                    CreateRasterBandObject(-1.824,
                                            1.016 * (isdMetadata.IMD.BAND_P.ABSCALFACTOR / isdMetadata.IMD.BAND_P.EFFECTIVEBANDWIDTH))
                    );
                }
                stacAsset.EoExtension().Bands = eoBandObjects.ToArray();
                stacAsset.RasterExtension().Bands = rasterBandObjects.ToArray();
                stacItem.Assets.Add("pan", stacAsset);
            }

            // WORLDVIEW-2
            else if (platformNumber == "2")
            {
                List<EoBandObject> eoBandObjects = new List<EoBandObject>();
                List<RasterBand> rasterBandObjects = new List<RasterBand>();
                if (isdMetadata.IMD.BAND_P != null)
                {
                    eoBandObjects.Add(
                     CreateEoBandObject(EoBandCommonName.pan.ToString(),
                                                EoBandCommonName.pan,
                                                0.6322,
                                                0.2846 / 2,
                                                1580.8140)
                    );
                    rasterBandObjects.Add(
                        CreateRasterBandObject(-1.7,
                                               0.923 * (isdMetadata.IMD.BAND_P.ABSCALFACTOR / isdMetadata.IMD.BAND_P.EFFECTIVEBANDWIDTH)));
                }
                if (isdMetadata.IMD.BAND_C != null)
                {
                    eoBandObjects.Add(
                        CreateEoBandObject(EoBandCommonName.coastal.ToString(),
                                           EoBandCommonName.coastal,
                                           0.4273,
                                           0.0473 / 2,
                                           1758.2229)
                    );
                    rasterBandObjects.Add(
                        CreateRasterBandObject(-7.154,
                                           0.863 * (isdMetadata.IMD.BAND_C.ABSCALFACTOR / isdMetadata.IMD.BAND_C.EFFECTIVEBANDWIDTH)));
                }
                if (isdMetadata.IMD.BAND_B != null)
                {
                    eoBandObjects.Add(
                        CreateEoBandObject(EoBandCommonName.blue.ToString(),
                                           EoBandCommonName.blue,
                                           0.4779,
                                           0.0543 / 2,
                                           1974.2416)
                    );
                    rasterBandObjects.Add(
                        CreateRasterBandObject(-4.189,
                                           0.905 * (isdMetadata.IMD.BAND_B.ABSCALFACTOR / isdMetadata.IMD.BAND_B.EFFECTIVEBANDWIDTH)));
                }
                if (isdMetadata.IMD.BAND_G != null)
                {
                    eoBandObjects.Add(
                        CreateEoBandObject(EoBandCommonName.green.ToString(),
                                           EoBandCommonName.green,
                                           0.5462,
                                           0.0374 / 2,
                                           1856.4104)
                    );
                    rasterBandObjects.Add(
                        CreateRasterBandObject(-3.287,
                                           0.907 * (isdMetadata.IMD.BAND_G.ABSCALFACTOR / isdMetadata.IMD.BAND_G.EFFECTIVEBANDWIDTH)));
                }
                if (isdMetadata.IMD.BAND_Y != null)
                {
                    eoBandObjects.Add(
                        CreateEoBandObject(EoBandCommonName.yellow.ToString(),
                                           EoBandCommonName.yellow,
                                           0.6078,
                                           0.0374 / 2,
                                           1738.4791)
                    );
                    rasterBandObjects.Add(
                        CreateRasterBandObject(-1.816,
                                           0.938 * (isdMetadata.IMD.BAND_Y.ABSCALFACTOR / isdMetadata.IMD.BAND_Y.EFFECTIVEBANDWIDTH)));
                }
                if (isdMetadata.IMD.BAND_R != null)
                {
                    eoBandObjects.Add(
                        CreateEoBandObject(EoBandCommonName.red.ToString(),
                                           EoBandCommonName.red,
                                           0.6588,
                                           0.0374 / 2,
                                           1559.4555)
                    );
                    rasterBandObjects.Add(
                        CreateRasterBandObject(-1.350,
                                           0.945 * (isdMetadata.IMD.BAND_R.ABSCALFACTOR / isdMetadata.IMD.BAND_R.EFFECTIVEBANDWIDTH)));
                }
                if (isdMetadata.IMD.BAND_RE != null)
                {
                    eoBandObjects.Add(
                        CreateEoBandObject(EoBandCommonName.rededge.ToString(),
                                           EoBandCommonName.rededge,
                                           0.7237,
                                           0.0374 / 2,
                                           1342.0695)
                    );
                    rasterBandObjects.Add(
                        CreateRasterBandObject(-2.617,
                                           0.980 * (isdMetadata.IMD.BAND_RE.ABSCALFACTOR / isdMetadata.IMD.BAND_RE.EFFECTIVEBANDWIDTH)));
                }
                if (isdMetadata.IMD.BAND_N != null)
                {
                    eoBandObjects.Add(
                        CreateEoBandObject(EoBandCommonName.nir08.ToString(),
                                           EoBandCommonName.nir08,
                                           0.8313,
                                           0.0374 / 2,
                                           1069.7302)
                    );
                    rasterBandObjects.Add(
                        CreateRasterBandObject(-3.752,
                                           0.982 * (isdMetadata.IMD.BAND_N.ABSCALFACTOR / isdMetadata.IMD.BAND_N.EFFECTIVEBANDWIDTH)));
                }
                if (isdMetadata.IMD.BAND_N2 != null)
                {
                    eoBandObjects.Add(
                        CreateEoBandObject(EoBandCommonName.nir09.ToString(),
                                           EoBandCommonName.nir09,
                                            0.908,
                                           0.0996 / 2,
                                           861.2866)
                    );
                    rasterBandObjects.Add(
                        CreateRasterBandObject(-1.507,
                                           0.954 * (isdMetadata.IMD.BAND_N2.ABSCALFACTOR / isdMetadata.IMD.BAND_N2.EFFECTIVEBANDWIDTH)));
                }
                stacAsset.EoExtension().Bands = eoBandObjects.ToArray();
                stacAsset.RasterExtension().Bands = rasterBandObjects.ToArray();
                stacItem.Assets.Add("pms", stacAsset);
            }

            // WORLDVIEW-3
            else if (platformNumber == "3")
            {
                List<EoBandObject> eoBandObjects = new List<EoBandObject>();
                List<RasterBand> rasterBandObjects = new List<RasterBand>();
                if (isdMetadata.IMD.BAND_P != null)
                {
                    eoBandObjects.Add(
                     CreateEoBandObject(EoBandCommonName.pan.ToString(),
                                                EoBandCommonName.pan,
                                                0.6494,
                                                0.2896 / 2,
                                                1574.41)
                    );
                    rasterBandObjects.Add(
                        CreateRasterBandObject(-1.7,
                                                0.923 * (isdMetadata.IMD.BAND_P.ABSCALFACTOR / isdMetadata.IMD.BAND_P.EFFECTIVEBANDWIDTH)));
                }
                if (isdMetadata.IMD.BAND_C != null)
                {
                    eoBandObjects.Add(
                        CreateEoBandObject(EoBandCommonName.coastal.ToString(),
                                           EoBandCommonName.coastal,
                                           0.4274,
                                           0.0405 / 2,
                                           1757.89)
                    );
                    rasterBandObjects.Add(
                        CreateRasterBandObject(-7.154,
                                           0.863 * (isdMetadata.IMD.BAND_C.ABSCALFACTOR / isdMetadata.IMD.BAND_C.EFFECTIVEBANDWIDTH)));
                }
                if (isdMetadata.IMD.BAND_B != null)
                {
                    eoBandObjects.Add(
                        CreateEoBandObject(EoBandCommonName.blue.ToString(),
                                           EoBandCommonName.blue,
                                           0.4819,
                                           0.0540 / 2,
                                           2004.61)
                    );
                    rasterBandObjects.Add(
                        CreateRasterBandObject(-4.189,
                                           0.905 * (isdMetadata.IMD.BAND_B.ABSCALFACTOR / isdMetadata.IMD.BAND_B.EFFECTIVEBANDWIDTH)));
                }
                if (isdMetadata.IMD.BAND_G != null)
                {
                    eoBandObjects.Add(
                        CreateEoBandObject(EoBandCommonName.green.ToString(),
                                           EoBandCommonName.green,
                                           0.5471,
                                           0.0618 / 2,
                                           1830.18)
                    );
                    rasterBandObjects.Add(
                        CreateRasterBandObject(-3.287,
                                           0.907 * (isdMetadata.IMD.BAND_G.ABSCALFACTOR / isdMetadata.IMD.BAND_G.EFFECTIVEBANDWIDTH)));
                }
                if (isdMetadata.IMD.BAND_Y != null)
                {
                    eoBandObjects.Add(
                        CreateEoBandObject(EoBandCommonName.yellow.ToString(),
                                           EoBandCommonName.yellow,
                                           0.6043,
                                           0.0381 / 2,
                                           1712.07)
                    );
                    rasterBandObjects.Add(
                        CreateRasterBandObject(-1.816,
                                           0.938 * (isdMetadata.IMD.BAND_Y.ABSCALFACTOR / isdMetadata.IMD.BAND_Y.EFFECTIVEBANDWIDTH)));
                }
                if (isdMetadata.IMD.BAND_R != null)
                {
                    eoBandObjects.Add(
                        CreateEoBandObject(EoBandCommonName.red.ToString(),
                                           EoBandCommonName.red,
                                           0.6601,
                                           0.0585 / 2,
                                           1535.33)
                    );
                    rasterBandObjects.Add(
                        CreateRasterBandObject(-1.350,
                                           0.945 * (isdMetadata.IMD.BAND_R.ABSCALFACTOR / isdMetadata.IMD.BAND_R.EFFECTIVEBANDWIDTH)));
                }
                if (isdMetadata.IMD.BAND_RE != null)
                {
                    eoBandObjects.Add(
                        CreateEoBandObject(EoBandCommonName.rededge.ToString(),
                                           EoBandCommonName.rededge,
                                           0.7227,
                                           0.0387 / 2,
                                           1348.08)
                    );
                    rasterBandObjects.Add(
                        CreateRasterBandObject(-2.617,
                                           0.980 * (isdMetadata.IMD.BAND_RE.ABSCALFACTOR / isdMetadata.IMD.BAND_RE.EFFECTIVEBANDWIDTH)));
                }
                if (isdMetadata.IMD.BAND_N != null)
                {
                    eoBandObjects.Add(
                        CreateEoBandObject(EoBandCommonName.nir08.ToString(),
                                           EoBandCommonName.nir08,
                                           0.8240,
                                           0.1004 / 2,
                                           1055.94)
                    );
                    rasterBandObjects.Add(
                        CreateRasterBandObject(-3.752,
                                           0.982 * (isdMetadata.IMD.BAND_N.ABSCALFACTOR / isdMetadata.IMD.BAND_N.EFFECTIVEBANDWIDTH)));
                }
                if (isdMetadata.IMD.BAND_N2 != null)
                {
                    eoBandObjects.Add(
                        CreateEoBandObject(EoBandCommonName.nir09.ToString(),
                                           EoBandCommonName.nir09,
                                          0.9136,
                                           0.0889 / 2,
                                           858.77)
                    );
                    rasterBandObjects.Add(
                        CreateRasterBandObject(-1.507,
                                           0.954 * (isdMetadata.IMD.BAND_N2.ABSCALFACTOR / isdMetadata.IMD.BAND_N2.EFFECTIVEBANDWIDTH)));
                }
                stacAsset.EoExtension().Bands = eoBandObjects.ToArray();
                stacAsset.RasterExtension().Bands = rasterBandObjects.ToArray();
                stacItem.Assets.Add("pms", stacAsset);
            }
        }


        private void AddEoBandPropertyInItem(StacItem stacItem)
        {
            var eo = stacItem.EoExtension();
            eo.Bands = stacItem.Assets.Values.Where(a => a.EoExtension().Bands != null)
                .SelectMany(a => a.EoExtension().Bands).ToArray();
        }


        public static async Task<Isd> DeserializeProductMetadata(IStreamResource productMetadataFile)
        {
            XmlSerializer ser = new XmlSerializer(typeof(Isd));
            Isd auxiliary;
            using (var stream = await productMetadataFile.GetStreamAsync())
            {
                using (XmlReader reader = XmlReader.Create(stream))
                {
                    auxiliary = (Isd)ser.Deserialize(reader);
                }
            }

            return auxiliary;
        }

        private IDictionary<string, object> GetCommonMetadata(JavaProperties metadata)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();

            FillDateTimeProperties(metadata, properties);
            FillInstrument(metadata, properties);


            return properties;
        }

        private void FillInstrument(JavaProperties metadata, Dictionary<string, object> properties)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            string format = "yyyyMMddHHmmss";
            DateTime.TryParseExact(metadata["ACQUISITION_DATE"] + metadata["ACQUISITION_TIME"], format, provider, DateTimeStyles.AssumeUniversal,
                out var datetimeDateTime);

            // remove previous values
            properties.Remove("datetime");
            properties.Remove("start_datetime");
            properties.Remove("end_datetime");

            properties.Add("datetime", datetimeDateTime.ToUniversalTime());

            // remove previous values
            properties.Remove("created");
            properties.Add("created", datetimeDateTime.ToUniversalTime());

            properties.Remove("updated");
            properties.Add("updated", DateTime.UtcNow);

        }

        private void FillDateTimeProperties(JavaProperties metadata,
            Dictionary<string, object> properties)
        {
            // platform & constellation
            var platformName = $"{metadata["PLATFORM"]}-{metadata["PLATFORM_NUMBER"]}".ToLower();
            if (!string.IsNullOrEmpty(platformName))
            {
                properties.Remove("platform");
                properties.Add("platform", platformName);

                properties.Remove("constellation");
                properties.Add("constellation", platformName);

                properties.Remove("mission");
                properties.Add("mission", platformName);
            }

            // instruments
            var instrumentName = $"{metadata["SENSOR"]}".ToLower();
            if (!string.IsNullOrEmpty(instrumentName))
            {
                properties.Remove("instruments");
                properties.Add("instruments", new string[] { instrumentName });
            }
            properties["sensor_type"] = "optical";

            // GSD
            var gsd = double.Parse(metadata["PIXEL_SIZE_X"]);
            if (gsd != 0)
            {
                properties.Remove("gsd");
                properties.Add("gsd", gsd);
            }
        }

        private void FillBasicsProperties(JavaProperties metadata, IDictionary<String, object> properties)
        {
            CultureInfo culture = new CultureInfo("fr-FR");
            // title
            properties.Remove("title");
            properties.Add("title", string.Format("{0} {1} {2} {3}",
                //StylePlatform(properties.GetProperty<string>("platform")),
                properties.GetProperty<string>("platform").ToUpper(),
                properties.GetProperty<string[]>("instruments").First().ToUpper(),
                properties.GetProperty<string>("processing:level").ToUpper(),
                properties.GetProperty<DateTime>("datetime").ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss", culture))
            );
        }


        private void AddViewStacExtension(Isd productMetadata, StacItem stacItem)
        {
            var view = new ViewStacExtension(stacItem);
            view.SunAzimuth = double.Parse(productMetadata.IMD.IMAGE.MEANSUNAZ);
            view.SunElevation = double.Parse(productMetadata.IMD.IMAGE.MEANSUNEL);
        }

        private void AddEoStacExtension(Isd metadata, StacItem stacItem)
        {
            //  source vendor_metadata/<file>.xml xpath /isd/IMD/IMAGE/CLOUDCOVER
            EoStacExtension eo = stacItem.EoExtension();
            eo.CloudCover = double.Parse(metadata.IMD.IMAGE.CLOUDCOVER);
        }


        private EoBandObject CreateEoBandObject(string name, EoBandCommonName eoBandCommonName, double centerWaveLength,
            double fullWidthHalfMax, double eai)
        {
            EoBandObject eoBandObject = new EoBandObject(name, eoBandCommonName);
            eoBandObject.Properties.Add("full_width_half_max", fullWidthHalfMax);
            eoBandObject.SolarIllumination = eai;
            eoBandObject.CenterWavelength = centerWaveLength;
            return eoBandObject;
        }

        private RasterBand CreateRasterBandObject(double offset, double gain)
        {
            RasterBand rasterBandObject = new RasterBand();
            rasterBandObject.Offset = offset;
            rasterBandObject.Scale = gain;
            return rasterBandObject;
        }

        private void AddProjStacExtension(Isd auxiliary, StacItem stacItem)
        {
           stacItem.ProjectionExtension().Epsg = null;
        }


        private GeoJSON.Net.Geometry.IGeometryObject GetGeometryObjectFromProductMetadata(
            JavaProperties data)
        {
            GeoJSON.Net.Geometry.LineString lineString = new GeoJSON.Net.Geometry.LineString(
                new GeoJSON.Net.Geometry.Position[5] {
                    new GeoJSON.Net.Geometry.Position(data["LL_LAT"],
                        data["LL_LONG"]),
                    new GeoJSON.Net.Geometry.Position(data["LR_LAT"],
                        data["LR_LONG"]),
                    new GeoJSON.Net.Geometry.Position(data["UR_LAT"],
                        data["UR_LONG"]),
                    new GeoJSON.Net.Geometry.Position(data["UL_LAT"],
                        data["UL_LONG"]),
                    new GeoJSON.Net.Geometry.Position(data["LL_LAT"],
                        data["LL_LONG"])
                }
            );
            return new GeoJSON.Net.Geometry.Polygon(new[] { lineString }).NormalizePolygon();
        }
    }
}