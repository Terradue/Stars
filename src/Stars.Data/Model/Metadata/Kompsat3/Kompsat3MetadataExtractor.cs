using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using ProjNet.CoordinateSystems;
using Stac;
using Stac.Extensions.Eo;
using Stac.Extensions.Processing;
using Stac.Extensions.Projection;
using Stac.Extensions.Sat;
using Stac.Extensions.View;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Services.Plugins;
using Stac.Extensions.Raster;
using Humanizer;
using Newtonsoft.Json.Linq;
using Terradue.Stars.Services;
using Terradue.Stars.Geometry.GeoJson;

namespace Terradue.Stars.Data.Model.Metadata.Kompsat3
{

    [PluginPriority(1000)]
    public class Kompsat3MetadataExtraction : MetadataExtraction
    {

        public override string Label => "Korea Multi-Purpose Satellite-3 (KARI) mission product metadata extractor";

        private const string ASCENDING = "Ascending Orbit";
        private const string DESCENDING = "Descending Orbit";

        public Kompsat3MetadataExtraction(ILogger<Kompsat3MetadataExtraction> logger, IResourceServiceProvider resourceServiceProvider) : base(logger, resourceServiceProvider)
        {
        }

        protected override async Task<StacNode> ExtractMetadata(IItem item, string suffix)
        {
            logger.LogDebug("Retrieving the metadata file in the product package");
            IAsset auxFile = FindFirstAssetFromFileNameRegex(item, "[0-9a-zA-Z_-]*(Aux\\.xml)$");
            if (auxFile == null)
            {
                throw new FileNotFoundException(String.Format("Unable to find the metadata file asset"));
            }
            logger.LogDebug(String.Format("Metadata file is {0}", auxFile.Uri));

            IStreamResource auxFileStreamable = await resourceServiceProvider.GetStreamResourceAsync(auxFile);
            if (auxFileStreamable == null)
            {
                logger.LogError("metadata file asset is not streamable, skipping metadata extraction");
                return null;
            }

            logger.LogDebug("Deserializing metadata");
            Auxiliary auxiliary = await DeserializeAuxiliary(auxFileStreamable);
            logger.LogDebug("Metadata deserialized. Starting metadata generation");

            StacItem stacItem = new StacItem(Regex.Replace(auxiliary.Images.First().Value.ImageFileName, @"(.*)(_\w+.tif)", @"$1"),
                                                GetGeometry(auxiliary.Images.First().Value.ImagingCoordinates),
                                                GetCommonMetadata(auxiliary));

            AddEoStacExtension(auxiliary, stacItem);
            AddSatStacExtension(auxiliary, stacItem);
            AddProjStacExtension(auxiliary, stacItem);
            AddViewStacExtension(auxiliary, stacItem);
            AddProcessingStacExtension(auxiliary, stacItem);
            AddOtherProperties(auxiliary, stacItem);

            AddAssets(stacItem, auxiliary, item);

            // AddEoBandPropertyInItem(stacItem);
            FillBasicsProperties(auxiliary, stacItem.Properties);

            return StacItemNode.Create(stacItem, item.Uri);; ;

        }

        private void AddProcessingStacExtension(Auxiliary auxiliary, StacItem stacItem)
        {
            var proc = stacItem.ProcessingExtension();
            proc.Level = auxiliary.General.ProductLevel.Replace("Level", "L");
            proc.Software.Add("PMS", auxiliary.General.PMSVersionNo);
        }

        private void AddEoBandPropertyInItem(StacItem stacItem)
        {
            var eo = stacItem.EoExtension();

            eo.Bands = stacItem.Assets.Values.Where(a => a.EoExtension().Bands != null).SelectMany(a => a.EoExtension().Bands).ToArray();
        }

        private void AddAssets(StacItem stacItem, Auxiliary auxiliary, IAssetsContainer assetsContainer)
        {
            foreach (var asset in assetsContainer.Assets.Values)
            {
                AddAsset(stacItem, auxiliary, asset);
            }
            foreach (var image in auxiliary.Images)
            {
                var asset = FindFirstAssetFromFileNameRegex(assetsContainer, image.Value.ImageFileName);
                if (asset == null)
                    continue;
                AddbandAsset(stacItem, image.Key, asset, image.Value, GetEAI(image.Key)); ;
            }
        }

        private double GetEAI(string key)
        {
            switch (key)
            {
                case "PAN":
                    return 0;
                case "MS1":
                case "PS1":
                    return 2001;
                case "MS2":
                case "PS2":
                    return 1875;
                case "MS3":
                case "PS3":
                    return 1525;
                case "MS4":
                case "PS4":
                    return 1027;
                default:
                    return 0;
            }
        }

        private void AddAsset(StacItem stacItem, Auxiliary auxiliary, IAsset asset)
        {
            string filename = Path.GetFileName(asset.Uri.ToString());
            if (filename.EndsWith("_th.jpg", true, CultureInfo.InvariantCulture))
                stacItem.Assets.Add("thumbnail", GetGenericAsset(stacItem, asset, "thumbnail"));
            if (filename.EndsWith("_br.jpg", true, CultureInfo.InvariantCulture))
                stacItem.Assets.TryAdd("overview", GetGenericAsset(stacItem, asset, "overview"));
            if (filename.EndsWith("_br.jpw", true, CultureInfo.InvariantCulture))
                stacItem.Assets.Add("overview-worldfile", GetGenericAsset(stacItem, asset, "overview-worldfile"));
            if (filename.EndsWith("_Aux.xml", true, CultureInfo.InvariantCulture))
                stacItem.Assets.Add("metadata", GetGenericAsset(stacItem, asset, "metadata"));

        }

        private void AddbandAsset(StacItem stacItem, string bandKey, IAsset asset, IImage image, double eai)
        {
            StacAsset stacAsset = StacAsset.CreateDataAsset(stacItem, asset.Uri,
                new System.Net.Mime.ContentType(MimeTypes.GetMimeType(asset.Uri.ToString()))
            );
            stacAsset.Properties.AddRange(asset.Properties);
            stacAsset.Properties.Add("gsd", image.ImageGSD.Column);
            string bandName = image.ImageColor.StartsWith("Not", true, CultureInfo.InvariantCulture) ? bandKey : image.ImageColor;
            if (bandName.StartsWith("Pan", true, CultureInfo.InvariantCulture))
                bandName = "PAN";
            EoBandObject eoBandObject = new EoBandObject(bandKey,
                                            GetEoCommonName(bandName));
            var bandRanges = Array.ConvertAll<string, double>(image.Bandwidth.Split('-'), s => double.Parse(s, CultureInfo.CreateSpecificCulture("en-US")));
            double? bandwidth = (bandRanges.Length > 1 ? (bandRanges[0] + ((bandRanges[1] - bandRanges[0]) / 2)) : bandRanges[0]) / 100;
            stacAsset.EoExtension().CloudCover = image.CloudCover?.Average;
            RasterBand rasterBand = new RasterBand();
            if (image.DNRange != null)
                rasterBand.Statistics = new Stac.Common.Statistics(
                                                double.Parse(image.DNRange?.MinimumDN),
                                                double.Parse(image.DNRange?.MaximumDN),
                                                null, null, null
                                            );
            stacAsset.ProjectionExtension().Shape = new int[2] { image.ImageSize.Width, image.ImageSize.Height };
            if (bandKey != "PAN")
            {
                rasterBand.Scale = image.RadianceConversion.Gain;
                rasterBand.Offset = image.RadianceConversion.Offset;
                eoBandObject.SolarIllumination = eai;
            }
            else
            {
                bandwidth /= 10;
            }
            if (JObject.FromObject(rasterBand).Children().Count() > 0)
                stacAsset.RasterExtension().Bands = new RasterBand[1] { rasterBand };
            eoBandObject.CenterWavelength = bandwidth;
            stacAsset.Title = string.Format("{0} {1}nm radiance", bandName.ToLower().Titleize(), bandwidth * 1000, image);
            stacAsset.EoExtension().Bands = new EoBandObject[1] { eoBandObject };
            stacItem.Assets.Add(bandKey, stacAsset);
        }

        private StacAsset GetGenericAsset(IStacObject stacObject, IAsset asset, string role)
        {
            StacAsset stacAsset = new StacAsset(stacObject, asset.Uri);
            stacAsset.Properties.AddRange(asset.Properties);
            stacAsset.Roles.Add(role);
            stacAsset.MediaType = new System.Net.Mime.ContentType(MimeTypes.GetMimeType(Path.GetFileName(asset.Uri.ToString())));
            return stacAsset;
        }

        private void AddOtherProperties(Auxiliary auxiliary, StacItem stacItem)
        {
            stacItem.Properties.Add("product_type", "PAN_MS_" + auxiliary.General.ProductLevel.Replace("Level", "L"));
        }

        private void AddViewStacExtension(Auxiliary auxiliary, StacItem stacItem)
        {
            var view = new ViewStacExtension(stacItem);
            view.OffNadir = auxiliary.Images.First().Value.Angle.Offnadir;
            view.IncidenceAngle = auxiliary.Images.First().Value.Angle.Incidence;
            view.Azimuth = auxiliary.Images.First().Value.Angle.Azimuth;
            view.SunAzimuth = auxiliary.Metadata.MetadataBlock.Average(mb => mb.SunAngle.Azimuth);
            view.SunElevation = auxiliary.Metadata.MetadataBlock.Average(mb => mb.SunAngle.Elevation);
        }

        private void AddSatStacExtension(Auxiliary auxiliary, StacItem stacItem)
        {
            var sat = new SatStacExtension(stacItem);
            sat.AbsoluteOrbit = auxiliary.General.OrbitNumber;
            sat.RelativeOrbit = auxiliary.General.OrbitNumber;
            switch (auxiliary.General.OrbitDirection)
            {
                case ASCENDING:
                    sat.OrbitState = "ascending";
                    break;
                case DESCENDING:
                    sat.OrbitState = "descending";
                    break;
            }
        }

        private void AddEoStacExtension(Auxiliary auxiliary, StacItem stacItem)
        {
            EoStacExtension eo = stacItem.EoExtension();
            if (auxiliary.Images.First().Value.CloudCover != null)
                eo.CloudCover = auxiliary.Images.First().Value.CloudCover.Average;
            else
                eo.CloudCover = 0;
        }

        private void AddProjStacExtension(Auxiliary auxiliary, StacItem stacItem)
        {
            ProjectionStacExtension proj = stacItem.ProjectionExtension();
            switch (auxiliary.General.Projection.Type)
            {
                case "UTM":
                    int zone = int.Parse(auxiliary.General.Projection.Parameter.Substring(1));
                    bool north = auxiliary.General.Projection.Parameter[0] == 'N';
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
            var platformname = auxiliary.General.Satellite.ToLower();
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
            var instrumentName = auxiliary.General.Sensor.ToLower();
            if (!string.IsNullOrEmpty(instrumentName))
            {
                properties.Remove("instruments");
                properties.Add("instruments", new string[] { instrumentName });
            }
            properties["sensor_type"] = "optical";

            var gsd = auxiliary.Images.FirstOrDefault(i => i.Key.StartsWith("P")).Value.ImageGSD.Column;
            if (gsd != 0)
            {
                properties.Remove("gsd");
                properties.Add("gsd", gsd);
            }
        }

        private void FillDateTimeProperties(Auxiliary auxiliary, Dictionary<string, object> properties)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            string format = "yyyyMMddHHmmss.fffffff";
            DateTime startDate = DateTime.MinValue;
            DateTime.TryParseExact(auxiliary.Images.First().Value.ImagingTime.ImagingStartTime.UTC, format, provider, DateTimeStyles.AssumeUniversal, out startDate);
            DateTime endDate = startDate;
            DateTime.TryParseExact(auxiliary.Images.First().Value.ImagingTime.ImagingEndTime.UTC, format, provider, DateTimeStyles.AssumeUniversal, out endDate);

            Itenso.TimePeriod.TimeInterval dateInterval = new Itenso.TimePeriod.TimeInterval(startDate, endDate);

            // remove previous values
            properties.Remove("datetime");
            properties.Remove("start_datetime");
            properties.Remove("end_datetime");

            // datetime, start_datetime, end_datetime
            if (dateInterval.IsAnytime)
            {
                properties.Add("datetime", null);
            }

            if (dateInterval.IsMoment)
            {
                properties.Add("datetime", dateInterval.Start);
            }
            else
            {
                properties.Add("datetime", dateInterval.Start);
                properties.Add("start_datetime", dateInterval.Start);
                properties.Add("end_datetime", dateInterval.End);
            }

            DateTime createdDate = DateTime.MinValue;
            DateTime.TryParseExact(auxiliary.General.CreateDate, format, provider, DateTimeStyles.AssumeUniversal, out createdDate);

            if (createdDate.Ticks != 0)
            {
                properties.Remove("created");
                properties.Add("created", createdDate);
            }

            properties.Remove("updated");
            properties.Add("updated", DateTime.UtcNow);
        }

        private void FillBasicsProperties(Auxiliary auxiliary, IDictionary<String, object> properties)
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

        private GeoJSON.Net.Geometry.IGeometryObject GetGeometry(ImagingCoordinates imagingCoordinates)
        {
            GeoJSON.Net.Geometry.LineString lineString = new GeoJSON.Net.Geometry.LineString(
                new GeoJSON.Net.Geometry.Position[7]{
                    new GeoJSON.Net.Geometry.Position(imagingCoordinates.ImageGeogBL.Latitude, imagingCoordinates.ImageGeogBL.Longitude),
                    new GeoJSON.Net.Geometry.Position(imagingCoordinates.ImageGeogBC.Latitude, imagingCoordinates.ImageGeogBC.Longitude),
                    new GeoJSON.Net.Geometry.Position(imagingCoordinates.ImageGeogBR.Latitude, imagingCoordinates.ImageGeogBR.Longitude),
                    new GeoJSON.Net.Geometry.Position(imagingCoordinates.ImageGeogTR.Latitude, imagingCoordinates.ImageGeogTR.Longitude),
                    new GeoJSON.Net.Geometry.Position(imagingCoordinates.ImageGeogTC.Latitude, imagingCoordinates.ImageGeogTC.Longitude),
                    new GeoJSON.Net.Geometry.Position(imagingCoordinates.ImageGeogTL.Latitude, imagingCoordinates.ImageGeogTL.Longitude),
                    new GeoJSON.Net.Geometry.Position(imagingCoordinates.ImageGeogBL.Latitude, imagingCoordinates.ImageGeogBL.Longitude)
                }
            );

            return new GeoJSON.Net.Geometry.Polygon(new GeoJSON.Net.Geometry.LineString[] { lineString }).NormalizePolygon();
        }


        /// <summary>Deserialize Auxiliary from xml to class</summary>
        /// <param name="auxiliaryFile">The <see cref="StreamWrapper"/> instance linked to the metadata file.</param>
        /// <returns>The deserialized metadata object.</returns>
        public static async Task<Auxiliary> DeserializeAuxiliary(IStreamResource auxiliaryFile)
        {
            XmlSerializer ser = new XmlSerializer(typeof(Auxiliary));
            Auxiliary auxiliary;
            using (var stream = await auxiliaryFile.GetStreamAsync())
            {
                using (XmlReader reader = XmlReader.Create(stream))
                {
                    auxiliary = (Auxiliary)ser.Deserialize(reader);
                }
            }
            return auxiliary;
        }

        protected virtual IAsset GetMetadataAsset(IItem item)
        {
            IAsset manifestAsset = FindFirstAssetFromFileNameRegex(item, @"[0-9a-zA-Z_-]*(Aux\.xml)$");
            if (manifestAsset == null)
            {
                throw new FileNotFoundException(String.Format("Unable to find the metadata file asset"));
            }
            return manifestAsset;
        }

        public override bool CanProcess(IResource route, IDestination destinations)
        {
            IItem item = route as IItem;
            if (item == null) return false;
            try
            {
                IAsset metadataAsset = GetMetadataAsset(item);
                Auxiliary metadata = DeserializeAuxiliary(resourceServiceProvider.GetStreamResourceAsync(metadataAsset).GetAwaiter().GetResult()).GetAwaiter().GetResult();
                return metadata.General.Satellite.StartsWith("KOMPSAT-3");
            }
            catch
            {
                return false;
            }
        }


    }

}