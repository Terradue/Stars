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
using ProjNet.CoordinateSystems;
using Stac;
using Stac.Extensions.Eo;
using Stac.Extensions.Processing;
using Stac.Extensions.Projection;
using Stac.Extensions.Raster;
using Stac.Extensions.Sat;
using Stac.Extensions.View;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Services.Plugins;

namespace Terradue.Stars.Data.Model.Metadata.Isro
{

    [PluginPriority(1000)]
    public class IsroMetadataExtractor : MetadataExtraction
    {

        private const string ASCENDING = "Ascending Orbit";
        private const string DESCENDING = "Descending Orbit";

        private static string[] satelitteIds = new string[] { "CARTOSAT-2", "IRS-R2", "IRS-R2A" };

        public override string Label => "ResourceSat/CartoSat (ISRO) constellation product metadata extractor";

        public IsroMetadataExtractor(ILogger<IsroMetadataExtractor> logger) : base(logger)
        {
        }

        public override bool CanProcess(IResource route, IDestination destinations)
        {
            IItem item = route as IItem;
            if (item == null) return false;
            try
            {
                IAsset metadataAsset = GetMetadataAsset(item);
                JavaProperties metadata = ReadMetadata(metadataAsset.GetStreamable()).GetAwaiter().GetResult();
                return (metadata.AllKeys.Contains("SatID") && satelitteIds.Contains(metadata["SatID"]));
            }
            catch
            {
                return false;
            }
        }

        protected override async Task<StacNode> ExtractMetadata(IItem item, string suffix)
        {
            IAsset metadataAsset = GetMetadataAsset(item);
            if (metadataAsset == null)
            {
                throw new FileNotFoundException(String.Format("Unable to find the metadata file asset"));
            }
            logger.LogDebug(String.Format("Metadata file is {0}", metadataAsset.Uri));
            logger.LogDebug("Reading metadata");
            JavaProperties metadata = await ReadMetadata(metadataAsset.GetStreamable());
            logger.LogDebug("Metadata deserialized. Starting metadata generation");

            StacItem stacItem = new StacItem(string.Format("{0}_{1}_{2}_{3}",
                                                            metadata["SatID"].Replace("-", ""),
                                                            !string.IsNullOrEmpty(metadata["ProductCode"]) ? metadata["ProductCode"] : metadata["ProdCode"],
                                                            metadata["ProdType"],
                                                            metadata["ProductID"]),
                                                GetGeometry(metadata),
                                                GetCommonMetadata(metadata));

            AddEoStacExtension(metadata, stacItem);
            AddSatStacExtension(metadata, stacItem);
            AddProjStacExtension(metadata, stacItem);
            AddViewStacExtension(metadata, stacItem);
            AddProcessingStacExtension(metadata, stacItem);

            AddAssets(stacItem, metadata, item);

            // AddEoBandPropertyInItem(stacItem);
            FillBasicsProperties(metadata, stacItem.Properties);

            return StacItemNode.CreateUnlocatedNode(stacItem);

        }

        private void AddProcessingStacExtension(JavaProperties metadata, StacItem stacItem)
        {
            var proc = stacItem.ProcessingExtension();
            proc.Level = metadata["ProcessingLevel"];
        }

        private void AddEoBandPropertyInItem(StacItem stacItem)
        {
            var eo = stacItem.EoExtension();
            eo.Bands = stacItem.Assets.Values.Where(a => a.EoExtension().Bands != null).SelectMany(a => a.EoExtension().Bands).ToArray();
        }

        private void AddAssets(StacItem stacItem, JavaProperties metadata, IAssetsContainer assetsContainer)
        {
            Dictionary<string, StacAssetAsset> stacAssets = new Dictionary<string, StacAssetAsset>();
            foreach (var asset in assetsContainer.Assets.Values.OrderBy(f => Path.GetFileName(f.Uri.ToString())))
            {
                AddAsset(stacItem, metadata, asset);
            }
        }

        private double GetEAI(string bandName, JavaProperties metadata)
        {
            switch (bandName)
            {
                case "pan":
                    return 0;
                case "green":
                    if (metadata["Sensor"] == "L3")
                        return 181.83;
                    if (metadata["Sensor"] == "L4FX")
                        return 181.89;
                    break;
                case "red":
                    if (metadata["Sensor"] == "L3")
                        return 155.92;
                    if (metadata["Sensor"] == "L4FX")
                        return 156.96;
                    break;
                case "nir":
                    if (metadata["Sensor"] == "L3")
                        return 108.96;
                    if (metadata["Sensor"] == "L4FX")
                        return 110.48;
                    break;
                case "swir16":
                    if (metadata["Sensor"] == "L3")
                        return 24.38;
                    break;
                default:
                    return 0;
            }
            return 0;
        }

        private void AddAsset(StacItem stacItem, JavaProperties metadata, IAsset asset)
        {
            string filename = Path.GetFileName(asset.Uri.ToString());
            if (filename.EndsWith(".jpg", true, CultureInfo.InvariantCulture)){
                if (stacItem.Assets.TryAdd("overview",
                    StacAsset.CreateOverviewAsset(stacItem, asset.Uri, new System.Net.Mime.ContentType(MimeTypes.GetMimeType(filename))))){
                        stacItem.Assets["overview"].Properties.AddRange(asset.Properties);
                    }
            }
            if (filename.EndsWith("MET.txt", true, CultureInfo.InvariantCulture)){
                stacItem.Assets.Add("metadata",
                    StacAsset.CreateMetadataAsset(stacItem, asset.Uri, new System.Net.Mime.ContentType(MimeTypes.GetMimeType(filename))));
                stacItem.Assets["metadata"].Properties.AddRange(asset.Properties);
            }
            if (filename.EndsWith("META.txt", true, CultureInfo.InvariantCulture)){
                stacItem.Assets.Add("metadata",
                    StacAsset.CreateMetadataAsset(stacItem, asset.Uri, new System.Net.Mime.ContentType(MimeTypes.GetMimeType(filename))));
                stacItem.Assets["metadata"].Properties.AddRange(asset.Properties);
            }
            if (filename.EndsWith("REP.txt", true, CultureInfo.InvariantCulture)){
                stacItem.Assets.Add("geodata",
                    StacAsset.CreateMetadataAsset(stacItem, asset.Uri, new System.Net.Mime.ContentType(MimeTypes.GetMimeType(filename))));
                stacItem.Assets["geodata"].Properties.AddRange(asset.Properties);
            }
            if (filename.EndsWith("RPC.txt", true, CultureInfo.InvariantCulture)){
                stacItem.Assets.Add("geodata",
                    StacAsset.CreateMetadataAsset(stacItem, asset.Uri, new System.Net.Mime.ContentType(MimeTypes.GetMimeType(filename))));
                stacItem.Assets["geodata"].Properties.AddRange(asset.Properties);
            }
            if (filename.EndsWith(".tif", true, CultureInfo.InvariantCulture))
                AddbandAsset(metadata, asset, stacItem);

        }

        private void AddbandAsset(JavaProperties metadata, IAsset asset, StacItem stacItem)
        {
            StacAsset stacAsset = StacAsset.CreateDataAsset(stacItem, asset.Uri,
                new System.Net.Mime.ContentType(MimeTypes.GetMimeType(asset.Uri.ToString()))
            );
            stacAsset.Properties.AddRange(asset.Properties);
            string filename = Path.GetFileName(asset.Uri.ToString());
            string bandName = "unknown";
            double gain = 0;

            if (filename.EndsWith("BAND2.tif", true, CultureInfo.InvariantCulture))
            {
                gain = double.Parse(metadata["B2_Lmax"]) / 1024;
                bandName = "green";
            }
            if (filename.EndsWith("BAND3.tif", true, CultureInfo.InvariantCulture))
            {
                gain = double.Parse(metadata["B3_Lmax"]) / 1024;
                bandName = "red";
            }
            if (filename.EndsWith("BAND4.tif", true, CultureInfo.InvariantCulture))
            {
                gain = double.Parse(metadata["B4_Lmax"]) / 1024;
                bandName = "nir";
            }
            if (filename.EndsWith("BAND5.tif", true, CultureInfo.InvariantCulture))
            {
                gain = double.Parse(metadata["B5_Lmax"]) / 1024;
                bandName = "swir16";
            }
            if (filename.EndsWith("_P.tif", true, CultureInfo.InvariantCulture)
                || filename.EndsWith("PAN", true, CultureInfo.InvariantCulture))
                bandName = "pan";

            EoBandObject eoBandObject = new EoBandObject(bandName,
                                            GetEoCommonName(bandName));
            if (bandName != "pan")
            {
                RasterBand rasterBandObject = new RasterBand();
                rasterBandObject.Scale = gain;
                stacAsset.RasterExtension().Bands = new RasterBand[1] { rasterBandObject };
                // eoBandObject.Properties.Add("offset", image.RadianceConversion.Offset);
                eoBandObject.SolarIllumination = GetEAI(bandName, metadata);
            }
            stacAsset.EoExtension().Bands = new EoBandObject[1] { eoBandObject };
            
            stacItem.Assets.Add(bandName, stacAsset);
        }

        private void AddViewStacExtension(JavaProperties metadata, StacItem stacItem)
        {
            var view = new ViewStacExtension(stacItem);
            double azimuth = 0;
            double.TryParse(metadata["SatelliteHeading"], out azimuth);
            if (azimuth != 0)
                view.Azimuth = azimuth;

            double incidenceAngle = 0;
            double.TryParse(metadata["AngleIncidence"], out incidenceAngle);
            if (incidenceAngle != 0)
                view.IncidenceAngle = incidenceAngle;

            double sunazimuth = 0;
            double.TryParse(metadata["SunAziumthAtCenter"], out sunazimuth);
            if (sunazimuth == 0)
                double.TryParse(metadata["SunAzimuthAtCenter"], out sunazimuth);
            if (sunazimuth != 0)
                view.SunAzimuth = sunazimuth;

            double elevation = 0;
            double.TryParse(metadata["SunElevationAtCenter"], out elevation);
            if (elevation != 0)
                view.SunElevation = elevation;

        }

        private void AddSatStacExtension(JavaProperties metadata, StacItem stacItem)
        {
            var sat = new SatStacExtension(stacItem);
            int orbitno = 0;
            int.TryParse(metadata["ImagingOrbitNo"], out orbitno);
            if (orbitno != 0)
                sat.AbsoluteOrbit = orbitno;

            switch (metadata["ImagingDirection"])
            {
                case "A":
                    sat.OrbitState = "ascending";
                    break;
                case "D":
                    sat.OrbitState = "descending";
                    break;
            }
        }

        private void AddEoStacExtension(JavaProperties metadata, StacItem stacItem)
        {
            EoStacExtension eo = stacItem.EoExtension();
        }

        private void AddProjStacExtension(JavaProperties metadata, StacItem stacItem)
        {
            ProjectionStacExtension proj = stacItem.ProjectionExtension();
            switch (metadata["MapProjection"])
            {
                case "UTM":
                    int zone = int.Parse(metadata["ZoneNo"]);
                    bool north = double.Parse(metadata["ImageULLat"]) > 0;
                    ProjectedCoordinateSystem utm = ProjectedCoordinateSystem.WGS84_UTM(zone, north);
                    proj.SetCoordinateSystem(utm);
                    break;
            }
        }

        private IDictionary<string, object> GetCommonMetadata(JavaProperties metadata)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();

            FillDateTimeProperties(metadata, properties);
            // TODO Licensing
            // TODO Provider
            FillInstrument(metadata, properties);


            return properties;
        }

        private void FillInstrument(JavaProperties metadata, Dictionary<string, object> properties)
        {
            // platform & constellation
            var platformname = metadata["SatID"].ToLower()
                .Replace("_", "-")
                .Replace("irs-r2", "resourcesat-2");
            if (!string.IsNullOrEmpty(platformname))
            {
                properties.Remove("platform");
                properties.Add("platform", platformname);

                var constellationName = platformname.TrimEnd('a').TrimEnd('b');
                properties.Remove("constellation");
                properties.Add("constellation", constellationName);

                properties.Remove("mission");
                properties.Add("mission", constellationName);
            }

            // instruments
            var instrumentName = metadata["Sensor"].ToLower();
            if (!string.IsNullOrEmpty(instrumentName))
            {
                properties.Remove("instruments");
                properties.Add("instruments", new string[] { instrumentName });
            }

            double gsd = 0;
            double.TryParse(metadata["PlannedGSD"], out gsd);
            if (gsd != 0)
            {
                properties.Remove("gsd");
                properties.Add("gsd", gsd);
            }
        }

        private void FillDateTimeProperties(JavaProperties metadata, Dictionary<string, object> properties)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            string format = "dd-MMM-yyyy HH:mm:ss.ffffff";
            DateTime dateTime = DateTime.MinValue;
            DateTime.TryParseExact(metadata["SceneCenterTime"], format, provider, DateTimeStyles.AssumeUniversal, out dateTime);

            if (dateTime.Ticks == 0)
            {
                format = "ddMMMyyHH:mm:ss:ffff";
                dateTime = DateTime.MinValue;
                DateTime.TryParseExact(metadata["DateOfPass"] + metadata["SceneCenterTime"], format, provider, DateTimeStyles.AssumeUniversal, out dateTime);
            }

            Itenso.TimePeriod.TimeInterval dateInterval = new Itenso.TimePeriod.TimeInterval(dateTime, dateTime);

            // remove previous values
            properties.Remove("datetime");

            // datetime, start_datetime, end_datetime
            if (dateInterval.IsAnytime)
            {
                properties.Add("datetime", null);
            }

            if (dateInterval.IsMoment)
            {
                properties.Add("datetime", dateInterval.Start.ToUniversalTime());
            }
            else
            {
                properties.Add("datetime", dateInterval.Start);
                properties.Add("start_datetime", dateInterval.Start);
                properties.Add("end_datetime", dateInterval.End);
            }

            string cformat = "dd-MMM-yyyy HH:mm:ss";
            DateTime createdDate = DateTime.MinValue;
            if (!DateTime.TryParseExact(metadata["GenerationDateTime"], cformat, provider, DateTimeStyles.AssumeUniversal, out createdDate))
            {
                cformat = "ddMMMyy HH:mm:ss";
                DateTime.TryParseExact(metadata["GenerationDateTime"], cformat, provider, DateTimeStyles.AssumeUniversal, out createdDate);
            }


            if (createdDate.Ticks != 0)
            {
                properties.Remove("created");
                properties.Add("created", createdDate.ToUniversalTime());
            }

            properties.Remove("updated");
            properties.Add("updated", DateTime.UtcNow);
        }

        private void FillBasicsProperties(JavaProperties metadata, IDictionary<String, object> properties)
        {
            CultureInfo culture = new CultureInfo("fr-FR");
            // title
            properties.Remove("title");
            properties.Add("title", string.Format("{0} {1} {2} {3}",
                                                  StylePlatform(properties.GetProperty<string>("platform")),
                                                  properties.GetProperty<string[]>("instruments").First().ToUpper(),
                                                  properties.GetProperty<string>("processing:level").ToUpper(),
                                                  properties.GetProperty<DateTime>("datetime").ToUniversalTime().ToString("G", culture)));
        }

        private GeoJSON.Net.Geometry.IGeometryObject GetGeometry(JavaProperties metadata)
        {
            GeoJSON.Net.Geometry.LineString lineString = new GeoJSON.Net.Geometry.LineString(
                new GeoJSON.Net.Geometry.Position[5]{
                    new GeoJSON.Net.Geometry.Position(metadata["ImageULLat"], metadata["ImageULLon"]),
                    new GeoJSON.Net.Geometry.Position(metadata["ImageURLat"], metadata["ImageURLon"]),
                    new GeoJSON.Net.Geometry.Position(metadata["ImageLRLat"], metadata["ImageLRLon"]),
                    new GeoJSON.Net.Geometry.Position(metadata["ImageLLLat"], metadata["ImageLLLon"]),
                    new GeoJSON.Net.Geometry.Position(metadata["ImageULLat"], metadata["ImageULLon"]),
                }
            );

            return new GeoJSON.Net.Geometry.Polygon(new GeoJSON.Net.Geometry.LineString[] { lineString });
        }


        /// <summary>Deserialize JavaProperties from xml to class</summary>
        /// <param name="metadataFile">The <see cref="StreamWrapper"/> instance linked to the metadata file.</param>
        /// <returns>The deserialized metadata object.</returns>
        public static async Task<JavaProperties> ReadMetadata(IStreamable metadataFile)
        {
            var metadata = new JavaProperties();
            metadata.Load(await metadataFile.GetStreamAsync());
            return metadata;
        }

        protected virtual IAsset GetMetadataAsset(IItem item)
        {
            IAsset manifestAsset = FindFirstAssetFromFileNameRegex(item, @"[0-9a-zA-Z_-]*(META*\.(txt|TXT))$");
            if (manifestAsset == null)
            {
                throw new FileNotFoundException(String.Format("Unable to find the metadata file asset"));
            }
            return manifestAsset;
        }




    }

}