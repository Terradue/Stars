// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: Kompsat3MetadataExtractor.cs

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
using Itenso.TimePeriod;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using ProjNet.CoordinateSystems;
using Stac;
using Stac.Extensions.Eo;
using Stac.Extensions.Processing;
using Stac.Extensions.Projection;
using Stac.Extensions.Raster;
using Stac.Extensions.Sat;
using Stac.Extensions.View;
using Terradue.Stars.Data.Model.Metadata.Inpe.Schemas;
using Terradue.Stars.Data.Model.Metadata.Rcm;
using Terradue.Stars.Geometry.GeoJson;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Services.Plugins;

namespace Terradue.Stars.Data.Model.Metadata.Cas5001
{

    [PluginPriority(1000)]
    public class Cas5001MetadataExtractor : MetadataExtraction
    {
        public static GainOffsetValues[] gainOffsetLookup = new GainOffsetValues[]
        {
            new GainOffsetValues(2022, Cas5001Season.Summer, EoBandCommonName.blue, 0.249, -69.748),
            new GainOffsetValues(2022, Cas5001Season.Summer, EoBandCommonName.green, 0.148, -66.854),
            new GainOffsetValues(2022, Cas5001Season.Summer, EoBandCommonName.red, 0.191, -44.395),
            new GainOffsetValues(2022, Cas5001Season.Summer, EoBandCommonName.nir, 0.110, -103.057),
            new GainOffsetValues(2022, Cas5001Season.Summer, EoBandCommonName.pan, 0.178, -85.401),
            new GainOffsetValues(2023, Cas5001Season.Summer, EoBandCommonName.blue, 0.181, -48.747),
            new GainOffsetValues(2023, Cas5001Season.Summer, EoBandCommonName.green, 0.116, -54.661),
            new GainOffsetValues(2023, Cas5001Season.Summer, EoBandCommonName.red, 0.155, -37.026),
            new GainOffsetValues(2023, Cas5001Season.Summer, EoBandCommonName.nir, 0.080, -69.367),
            new GainOffsetValues(2023, Cas5001Season.Summer, EoBandCommonName.pan, 0.135, -59.574),
            new GainOffsetValues(2024, Cas5001Season.Summer, EoBandCommonName.blue, 0.220, -63.334),
            new GainOffsetValues(2024, Cas5001Season.Summer, EoBandCommonName.green, 0.135, -59.672),
            new GainOffsetValues(2024, Cas5001Season.Summer, EoBandCommonName.red, 0.175, -40.509),
            new GainOffsetValues(2024, Cas5001Season.Summer, EoBandCommonName.nir, 0.104, -71.435),
            new GainOffsetValues(2024, Cas5001Season.Summer, EoBandCommonName.pan, 0.165, -57.102),
            new GainOffsetValues(2022, Cas5001Season.Winter, EoBandCommonName.blue, 0.143, -65.958),
            new GainOffsetValues(2022, Cas5001Season.Winter, EoBandCommonName.green, 0.110, -52.135),
            new GainOffsetValues(2022, Cas5001Season.Winter, EoBandCommonName.red, 0.140, -32.310),
            new GainOffsetValues(2022, Cas5001Season.Winter, EoBandCommonName.nir, 0.107, -42.834),
            new GainOffsetValues(2022, Cas5001Season.Winter, EoBandCommonName.pan, 0.088, -38.418),
            new GainOffsetValues(2023, Cas5001Season.Winter, EoBandCommonName.blue, 0.096, -39.468),
            new GainOffsetValues(2023, Cas5001Season.Winter, EoBandCommonName.green, 0.084, -38.798),
            new GainOffsetValues(2023, Cas5001Season.Winter, EoBandCommonName.red, 0.107, -25.667),
            new GainOffsetValues(2023, Cas5001Season.Winter, EoBandCommonName.nir, 0.094, -42.437),
            new GainOffsetValues(2023, Cas5001Season.Winter, EoBandCommonName.pan, 0.071, -33.653),
            new GainOffsetValues(2024, Cas5001Season.Winter, EoBandCommonName.blue, 0.114, -42.374),
            new GainOffsetValues(2024, Cas5001Season.Winter, EoBandCommonName.green, 0.101, -37.246),
            new GainOffsetValues(2024, Cas5001Season.Winter, EoBandCommonName.red, 0.133, -26.284),
            new GainOffsetValues(2024, Cas5001Season.Winter, EoBandCommonName.nir, 0.110, -25.427),
            new GainOffsetValues(2024, Cas5001Season.Winter, EoBandCommonName.pan, 0.091, -32.973),
        };

        public override string Label => "Korea Multi-Purpose Satellite-3 (KARI) mission product metadata extractor";

        private const string ASCENDING = "Ascending Orbit";
        private const string DESCENDING = "Descending Orbit";

        public Cas5001MetadataExtractor(ILogger<Cas5001MetadataExtractor> logger, IResourceServiceProvider resourceServiceProvider) : base(logger, resourceServiceProvider)
        {
        }

        protected override async Task<StacNode> ExtractMetadata(IItem item, string suffix)
        {
            logger.LogDebug("Retrieving the metadata file in the product package");
            IAsset auxFile = FindFirstAssetFromFileNameRegex(item, "[0-9a-zA-Z_-]*(Aux\\.xml)$") ?? throw new FileNotFoundException(string.Format("Unable to find the metadata file asset"));
            logger.LogDebug(string.Format("Metadata file is {0}", auxFile.Uri));

            IStreamResource auxFileStreamable = await resourceServiceProvider.GetStreamResourceAsync(auxFile, System.Threading.CancellationToken.None);
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

            AddAssets(stacItem, auxiliary, item);

            // AddEoBandPropertyInItem(stacItem);
            FillBasicsProperties(auxiliary, stacItem.Properties);
            AddOtherProperties(auxiliary, stacItem);

            return StacNode.Create(stacItem, item.Uri); ; ;

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
                AddBandAsset(stacItem, image.Key, asset, image.Value, auxiliary);
            }
        }

        private double GetEAI(string key)
        {
            switch (key.ToLower())
            {
                case "panchromatic":
                case "pan":
                    return 1258.38;
                case "blue":
                    return 1984.65;
                case "green":
                    return 1815.54;
                case "red":
                    return 1536.38;
                case "near infrared":
                case "nir":
                    return 967.99;
                default:
                    return 0;
            }
        }

        private GainOffsetValues GetGainOffsetValues(IImage image, EoBandCommonName bandCommonName)
        {
            DateTime.TryParseExact(image.ImagingTime.ImagingCenterTime.UTC, "yyyyMMddHHmmss.fffffff", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime imageTime);
            double latitude = image.ImagingCoordinates.ImageGeogCenter.Latitude;

            Cas5001Season season;
            int year;
            if (imageTime.Month >= 5 && imageTime.Month < 11)
            {
                season = latitude > 0 ? Cas5001Season.Summer : Cas5001Season.Winter;
                year = imageTime.Year;
            }
            else if (imageTime.Month >= 11)
            {
                season = latitude > 0 ? Cas5001Season.Winter : Cas5001Season.Summer;
                year = imageTime.Year;
            }
            else
            {
                season = latitude > 0 ? Cas5001Season.Winter : Cas5001Season.Summer;
                year = imageTime.Year - 1;
            }

            GainOffsetValues best = null;

            foreach (GainOffsetValues lookup in gainOffsetLookup)
            {
                if (lookup.BandName == bandCommonName && lookup.Season == season)
                {
                    if (best == null || Math.Abs(lookup.Year - year) < Math.Abs(best.Year - year))
                    {
                        best = lookup;
                    }
                }
            }
            return best;
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

        private void AddBandAsset(StacItem stacItem, string bandKey, IAsset asset, IImage image, Auxiliary auxiliary)
        {

            StacAsset stacAsset = new StacAsset(stacItem, asset.Uri, new string[] { "data", "dn" }, null, new System.Net.Mime.ContentType(MimeTypes.GetMimeType(asset.Uri.ToString())));
            stacAsset.Properties.AddRange(asset.Properties);
            stacAsset.Properties.Add("gsd", image.ImageGSD.Column);
            string bandName = image.ImageColor.StartsWith("Not", true, CultureInfo.InvariantCulture) ? bandKey : image.ImageColor;
            if (bandName.StartsWith("Pan", true, CultureInfo.InvariantCulture))
                bandName = "PAN";
            EoBandCommonName bandCommonName = GetEoCommonName(bandName);
            EoBandObject eoBandObject = new EoBandObject(image.ImageColor, bandCommonName);
            var bandRanges = Array.ConvertAll(image.Bandwidth.Split('-'), s => double.Parse(s, CultureInfo.CreateSpecificCulture("en-US")));
            double? bandwidth = (bandRanges.Length > 1 ? (bandRanges[0] + bandRanges[1]) / 2 : bandRanges[0]) / 1000;

            stacAsset.EoExtension().CloudCover = image.CloudCover?.Average;
            RasterBand rasterBand = new RasterBand();
            if (image.DNRange != null)
                rasterBand.Statistics = new Stac.Common.Statistics(
                                                double.Parse(image.DNRange?.MinimumDN),
                                                double.Parse(image.DNRange?.MaximumDN),
                                                null, null, null
                                            );
            stacAsset.ProjectionExtension().Shape = new int[2] { image.ImageSize.Height, image.ImageSize.Width };

            GainOffsetValues gainOffsetValues = GetGainOffsetValues(image, bandCommonName);
            if (gainOffsetValues != null)
            {
                rasterBand.Scale = gainOffsetValues.Gain;
                rasterBand.Offset = gainOffsetValues.Offset;
            }
            eoBandObject.SolarIllumination = GetEAI(image.ImageColor);

            if (JObject.FromObject(rasterBand).Children().Count() > 0)
                stacAsset.RasterExtension().Bands = new RasterBand[1] { rasterBand };
            eoBandObject.CenterWavelength = bandwidth;

            switch (bandCommonName)
            {
                case EoBandCommonName.pan:
                    eoBandObject.FullWidthHalfMax = 0.45;
                    break;
                case EoBandCommonName.blue:
                    eoBandObject.FullWidthHalfMax = 0.07;
                    break;
                case EoBandCommonName.green:
                    eoBandObject.FullWidthHalfMax = 0.08;
                    break;
                case EoBandCommonName.red:
                    eoBandObject.FullWidthHalfMax = 0.06;
                    break;
                case EoBandCommonName.nir:
                    eoBandObject.FullWidthHalfMax = 0.14;
                    break;
            }

            stacAsset.Title = string.Format("{0} {1}", auxiliary.General.ProductLevel.Replace("Level", "L"), image.ImageColor);
            stacAsset.EoExtension().Bands = new EoBandObject[1] { eoBandObject };

            Regex keyRegex = new Regex(@"([^_]+_[^_]+)\.[^\.]+$");
            Match match = keyRegex.Match(image.ImageFileName);
            string assetKey = match.Success ? match.Groups[1].Value.Replace("_", "-") : bandKey;

            stacItem.Assets.Add(assetKey, stacAsset);
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
            stacItem.Properties.Add("metadata_format", "XML");
            stacItem.Properties.Add("data_file_format", auxiliary.General.ImageFormat);
            if (IncludeProviderProperty)
            {
                StacProvider provider1 = CreateSingleProvider(
                    "NGII",
                    "The National Land Satellite (CAS500-1: Compact Advanced Satellite 500) was exclusively developed in Korea through a collaboration between MOLIT and the Ministry of Science and ICT. It serves as a high-precision Earth observation satellite, designed for efficient land management and disaster response. The satellite is currently operated by the NGII.",
                    new StacProviderRole[] { StacProviderRole.producer, StacProviderRole.processor, StacProviderRole.licensor },
                    null
                );
                StacProvider provider2 = CreateSingleProvider(
                    "KARI",
                    "The Compact Advanced Satellite 500 1 (CAS500-1) is a Korean satellite under a research and development program of the Korean Aerospace Research Institute (KARI).",
                    new StacProviderRole[] { StacProviderRole.licensor },
                    null
                );

                stacItem.Properties.Add("providers", new StacProvider[] { provider1, provider2 });

            }
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
            var platformName = auxiliary.General.Satellite;
            if (platformName.ToLower() == "cas500-1") sat.PlatformInternationalDesignator = "2021-022A";
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
            var platformName = auxiliary.General.Satellite.ToLower();
            if (!string.IsNullOrEmpty(platformName))
            {
                properties.Remove("platform");
                properties.Add("platform", platformName);

                var constellationName = Regex.Replace(platformName, "-?\\d$", String.Empty);
                properties.Remove("constellation");
                properties.Add("constellation", constellationName);

                properties.Remove("mission");
                properties.Add("mission", platformName);
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
            DateTime centerDate = startDate;
            DateTime.TryParseExact(auxiliary.Images.First().Value.ImagingTime.ImagingCenterTime.UTC, format, provider, DateTimeStyles.AssumeUniversal, out centerDate);

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
                properties.Add("datetime", centerDate);
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

        private void FillBasicsProperties(Auxiliary auxiliary, IDictionary<string, object> properties)
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
            using (var stream = await auxiliaryFile.GetStreamAsync(System.Threading.CancellationToken.None))
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
            IAsset manifestAsset = FindFirstAssetFromFileNameRegex(item, @"[0-9a-zA-Z_-]*(Aux\.xml)$") ?? throw new FileNotFoundException(string.Format("Unable to find the metadata file asset"));
            return manifestAsset;
        }

        public override bool CanProcess(IResource route, IDestination destinations)
        {
            if (!(route is IItem item)) return false;
            try
            {
                IAsset metadataAsset = GetMetadataAsset(item);
                Auxiliary metadata = DeserializeAuxiliary(resourceServiceProvider.GetStreamResourceAsync(metadataAsset, System.Threading.CancellationToken.None).GetAwaiter().GetResult()).GetAwaiter().GetResult();
                return metadata.General.Satellite.StartsWith("CAS500-1");
            }
            catch
            {
                return false;
            }
        }


    }

}
