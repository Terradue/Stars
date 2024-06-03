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
using Stac.Extensions.Sar;
using Stac.Extensions.Sat;
using Stac.Extensions.View;
using Terradue.Stars.Data.Model.Shared;
using Terradue.Stars.Geometry.GeoJson;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Services.Plugins;

namespace Terradue.Stars.Data.Model.Metadata.Rcm
{

    // public class Product : Product {}

    [PluginPriority(1000)]
    public class RcmMetadataExtraction : MetadataExtraction
    {
        private const string MISSION = "rcm";
        private const string LEVEL_0 = "L0";
        private const string LEVEL_1A = "L1A";
        private const string LEVEL_1C = "L1C";
        private const string LEVEL_1D = "L1D";

        public override string Label => "RADARSAT (CSA) constellation product metadata extractor";

        public RcmMetadataExtraction(ILogger<RcmMetadataExtraction> logger, IResourceServiceProvider resourceServiceProvider) : base(logger, resourceServiceProvider)
        {
        }

        protected override async Task<StacNode> ExtractMetadata(IItem item, string suffix)
        {
            logger.LogDebug("Retrieving the metadata file in the product package");
            IAsset manifest = FindFirstAssetFromFileNameRegex(item, "[0-9a-zA-Z_-]*(manifest.safe)$");
            IAsset auxFile = FindFirstAssetFromFileNameRegex(item, "[0-9a-zA-Z_-]*(product.xml)$");
            IAsset kmlFile = FindFirstAssetFromFileNameRegex(item, "[0-9a-zA-Z_-]*(mapOverlay.kml)$");
            if (auxFile == null)
            {
                throw new FileNotFoundException(string.Format("Unable to find the metadata file asset"));
            }
            logger.LogDebug(string.Format("Metadata file is {0}", auxFile.Uri));

            IStreamResource auxFileStreamable = await resourceServiceProvider.GetStreamResourceAsync(auxFile, System.Threading.CancellationToken.None);
            if (auxFileStreamable == null)
            {
                logger.LogError("metadata file asset is not streamable, skipping metadata extraction");
                return null;
            }
            logger.LogDebug("Deserializing metadata");
            Product auxiliary = await DeserializeProduct(auxFileStreamable);
            logger.LogDebug("Metadata deserialized. Starting metadata generation");

            IStreamResource kmlFileStreamable = await resourceServiceProvider.GetStreamResourceAsync(kmlFile, System.Threading.CancellationToken.None);
            Kml kml = null;
            if (kmlFileStreamable != null)
                kml = await DeserializeKml(kmlFileStreamable);
            else
                logger.LogError("KML file asset is not streamable, skipping geometry extraction");

            string stacIdentifier = null;
            Match match = Regex.Match(auxFile.Uri.ToString(), @"(.*\/)*(?'identifier'RCM(1|2|3)_[^\.\/]*)(\.\w+)*(\/.*)*");
            if (match.Success)
                stacIdentifier = match.Groups["identifier"].Value;
            else
                stacIdentifier = item.Id;

            StacItem stacItem = new StacItem(stacIdentifier, GetGeometry(auxiliary, kml),
                GetCommonMetadata(auxiliary));

            AddSatStacExtension(auxiliary, stacItem);
            AddSarStacExtension(auxiliary, stacItem);
            AddProjStacExtension(auxiliary, kml, stacItem);
            AddViewStacExtension(auxiliary, stacItem);
            // AddProcessingStacExtension(auxiliary, stacItem);

            AddOtherProperties(auxiliary, stacItem);

            AddAssets(stacItem, auxiliary, item);

            return StacItemNode.Create(stacItem, item.Uri); ;

        }

        private void AddEoBandPropertyInItem(StacItem stacItem)
        {
            var eo = stacItem.EoExtension();

            eo.Bands = stacItem.Assets.Values.Where(a => a.EoExtension().Bands != null).SelectMany(a => a.EoExtension().Bands).OrderBy(b => b.Name).ToArray();
        }

        private void AddAssets(StacItem stacItem, Product auxiliary, IAssetsContainer assetsContainer)
        {
            foreach (var asset in assetsContainer.Assets.Values)
            {
                AddAsset(stacItem, auxiliary, asset);
            }
        }

        private void AddAsset(StacItem stacItem, Product auxiliary, IAsset asset)
        {
            string filename = Path.GetFileName(asset.Uri.ToString());
            if (filename.Equals("productOverview.png"))
                stacItem.Assets.Add("overview", GetGenericAsset(stacItem, asset, "overview"));
            if (filename.EndsWith("product.xml", true, CultureInfo.InvariantCulture))
                stacItem.Assets.Add("metadata", GetGenericAsset(stacItem, asset, "metadata"));
            if (filename.EndsWith("manifest.safe", true, CultureInfo.InvariantCulture))
                stacItem.Assets.Add("manifest", GetGenericAsset(stacItem, asset, "metadata"));
            if (filename.EndsWith("_HH.tif", true, CultureInfo.InvariantCulture))
                stacItem.Assets.Add("amplitude-hh", GetAmplitudeAsset(stacItem, asset, "data", new string[] { "HH" }));
            if (filename.EndsWith("_HV.tif", true, CultureInfo.InvariantCulture))
                stacItem.Assets.Add("amplitude-hv", GetAmplitudeAsset(stacItem, asset, "data", new string[] { "HV" }));
            if (filename.EndsWith("_VH.tif", true, CultureInfo.InvariantCulture))
                stacItem.Assets.Add("amplitude-vh", GetAmplitudeAsset(stacItem, asset, "data", new string[] { "VH" }));
            if (filename.EndsWith("_VV.tif", true, CultureInfo.InvariantCulture))
                stacItem.Assets.Add("amplitude-vv", GetAmplitudeAsset(stacItem, asset, "data", new string[] { "VV" }));
            if (filename.EndsWith("_CV.tif", true, CultureInfo.InvariantCulture))
                stacItem.Assets.Add("amplitude-v", GetAmplitudeAsset(stacItem, asset, "data", new string[] { "VV" }));
            if (filename.EndsWith("_CH.tif", true, CultureInfo.InvariantCulture))
                stacItem.Assets.Add("amplitude-h", GetAmplitudeAsset(stacItem, asset, "data", new string[] { "HH" }));
            if (asset.Uri.ToString().Contains("calibration/"))
                stacItem.Assets.Add("calibration-" + Path.GetFileNameWithoutExtension(filename), GetGenericAsset(stacItem, asset, "metadata"));
        }

        private StacAsset GetGenericAsset(IStacObject stacObject, IAsset asset, string role)
        {
            StacAsset stacAsset = new StacAsset(stacObject, asset.Uri);
            stacAsset.Properties.AddRange(asset.Properties);
            stacAsset.Roles.Add(role);
            stacAsset.MediaType = new System.Net.Mime.ContentType(MimeTypes.GetMimeType(Path.GetFileName(asset.Uri.ToString())));
            return stacAsset;
        }

        private StacAsset GetAmplitudeAsset(IStacObject stacObject, IAsset asset, string role, string[] pol)
        {
            StacAsset stacAsset = new StacAsset(stacObject, asset.Uri);
            stacAsset.Properties.AddRange(asset.Properties);
            stacAsset.Roles.Add(role);
            stacAsset.Roles.Add(role);
            stacAsset.MediaType = new System.Net.Mime.ContentType(MimeTypes.GetMimeType(Path.GetFileName(asset.Uri.ToString())));
            stacAsset.Properties.SetProperty("polarization", pol);
            return stacAsset;
        }

        private void AddOtherProperties(Product auxiliary, StacItem stacItem)
        {
            if (IncludeProviderProperty)
            {
                AddSingleProvider(
                    stacItem.Properties,
                    "CSA",
                    "The RADARSAT Constellation Mission (RCM) is Canada's new generation of Earth observation satellites. The RCM uses a trio of satellites to take daily scans of the country and its waters.",
                    new StacProviderRole[] { StacProviderRole.producer, StacProviderRole.processor, StacProviderRole.licensor },
                    new Uri("https://www.asc-csa.gc.ca/eng/satellites/radarsat/what-is-rcm.asp")
                );
            }
        }

        private void AddViewStacExtension(Product auxiliary, StacItem stacItem)
        {
            var view = new ViewStacExtension(stacItem);

            if (auxiliary.SceneAttributes.ImageAttributes.IncAngNearRng != null && auxiliary.SceneAttributes.ImageAttributes.IncAngFarRng != null)
                view.IncidenceAngle = (double.Parse(auxiliary.SceneAttributes.ImageAttributes.IncAngNearRng.Text) + double.Parse(auxiliary.SceneAttributes.ImageAttributes.IncAngFarRng.Text)) / 2;

            // view.OffNadir = view.OffNadir / auxiliary.Root.SubSwaths.SubSwath.Count();
        }

        private void AddSarStacExtension(Product auxiliary, StacItem stacItem)
        {
            SarStacExtension sar = stacItem.SarExtension();

            sar.Required(GetInstrumentMode(auxiliary),
                GetFrequencyBand(auxiliary),
                GetPolarizations(auxiliary),
                GetProductType(auxiliary)
            );

            sar.ObservationDirection = ParseObservationDirection(auxiliary.SourceAttributes.RadarParameters.AntennaPointing);

            //Radar Frequency In gigahertz (GHz), for Center Frequency, multiple the value by 1e-9
            sar.CenterFrequency = double.Parse(auxiliary.SourceAttributes.RadarParameters.RadarCenterFrequency.Text) / 1000000000;

            sar.ResolutionRange = GetResolution(auxiliary);
            sar.ResolutionAzimuth = sar.ResolutionRange;

            sar.PixelSpacingRange = double.Parse(auxiliary.ImageReferenceAttributes.RasterAttributes.SampledPixelSpacing.Text);
            sar.PixelSpacingAzimuth = double.Parse(auxiliary.ImageReferenceAttributes.RasterAttributes.SampledPixelSpacing.Text);

            sar.LooksRange = auxiliary.ImageGenerationParameters.SarProcessingInformation.NumberOfRangeLooks;
            sar.LooksAzimuth = auxiliary.ImageGenerationParameters.SarProcessingInformation.NumberOfAzimuthLooks;
        }
        private void AddProcessingStacExtension(Product auxiliary, StacItem stacItem)
        {
            ProcessingStacExtension process = stacItem.ProcessingExtension();
            process.Level = GetProcessingLevel(auxiliary);
        }

        private string GetProcessingLevel(Product auxiliary)
        {
            return "";
        }

        private string GetInstrumentMode(Product auxiliary)
        {
            // switch (auxiliary.SourceAttributes.BeamMode)
            // {
            //     case BEAM_MODE.LOW_RESOLUTION_100:
            //         return "SC100";
            //     case BEAM_MODE.MEDIUM_RESOLUTION_50:
            //         return "SC50";
            //     case BEAM_MODE.MEDIUM_RESOLUTION_30:
            //         return "SC30";
            //     case BEAM_MODE.MEDIUM_RESOLUTION_16:
            //         return "16M";
            //     case BEAM_MODE.HIGH_RESOLUTION_5:
            //         return "5M";
            //     case BEAM_MODE.VERY_HIGH_RESOLUTION_3:
            //         return "3M";
            //     case BEAM_MODE.LOW_NOISE:
            //         return "LN";
            //     case BEAM_MODE.QUAD_POLARIZATION:
            //         return "QP";
            //     case BEAM_MODE.SHIP_DETECTION:
            //         return "SD";
            //     case BEAM_MODE.SPOTLIGHT:
            //         return "FSL";
            //     default:
            //         return "";
            // }
            //There are tons of them

            //Take the entire string if no numbers 
            //That since the beginning to the (eventual) first letter after the number group
            string inputString = auxiliary.SourceAttributes.BeamModeMnemonic;
            Match match = Regex.Match(inputString, @"^([^0-9]*\d+[^A-Za-z]*[A-Za-z]?)|([^0-9]+)");
            if (match.Success)
            {
                return match.Groups[1].Value != "" ? match.Groups[1].Value : match.Groups[2].Value;
            }
            else
            {
                return inputString;
            }
        }

        private SarCommonFrequencyBandName GetFrequencyBand(Product auxiliary)
        {
            return SarCommonFrequencyBandName.C; //HARDCODED it is a SAR-C instrument            
        }
        private string[] GetPolarizations(Product auxiliary)
        {
            var pols = new List<string>();
            var polarizations = auxiliary.SourceAttributes.RadarParameters.Polarizations.Split(' ');
            foreach (var pol in polarizations)
            {
                if (pol == "CH")
                    pols.Add("HH");
                else if (pol == "CV")
                    pols.Add("VV");
                else
                    pols.Add(pol);
            }
            return pols.ToArray();
        }
        private string GetProductType(Product auxiliary)
        {
            return auxiliary.ImageGenerationParameters.GeneralProcessingInformation.ProductType;
        }

        private double GetResolution(Product auxiliary)
        {
            switch (auxiliary.SourceAttributes.BeamMode)
            {
                case BEAM_MODE.LOW_RESOLUTION_100:
                    return 100;
                case BEAM_MODE.MEDIUM_RESOLUTION_50:
                    return 50;
                case BEAM_MODE.MEDIUM_RESOLUTION_30:
                    return 30;
                case BEAM_MODE.MEDIUM_RESOLUTION_16:
                    return 16;
                case BEAM_MODE.HIGH_RESOLUTION_5:
                    return 5;
                case BEAM_MODE.VERY_HIGH_RESOLUTION_3:
                    return 3;
                case BEAM_MODE.LOW_NOISE:
                    return 100;
                case BEAM_MODE.QUAD_POLARIZATION:
                    return 9;
                case BEAM_MODE.SHIP_DETECTION:
                case BEAM_MODE.SPOTLIGHT:
                default:
                    return 0;
            }
        }

        private void AddSatStacExtension(Product auxiliary, StacItem stacItem)
        {
            var sat = new SatStacExtension(stacItem);
            sat.OrbitState = auxiliary.SourceAttributes.OrbitAndAttitude.OrbitInformation.PassDirection.ToLower();
        }

        private void AddProjStacExtension(Product auxiliary, Kml kml, StacItem stacItem)
        {
            ProjectionStacExtension proj = stacItem.ProjectionExtension();
            try
            {
                int zone = auxiliary.ImageReferenceAttributes.GeographicInformation.mapProjection.utmProjectionParameters.utmZone;
                bool north = auxiliary.ImageReferenceAttributes.GeographicInformation.mapProjection.utmProjectionParameters.hemisphere == "N";
                ProjectedCoordinateSystem utm = ProjectedCoordinateSystem.WGS84_UTM(zone, north);
                proj.SetCoordinateSystem(utm);
            }
            catch
            {
                proj.Epsg = null;
            }
            proj.Shape = new int[2] { auxiliary.SceneAttributes.ImageAttributes.SamplesPerLine, auxiliary.SceneAttributes.ImageAttributes.NumLines };
        }

        private IDictionary<string, object> GetCommonMetadata(Product auxiliary)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();

            FillDateTimeProperties(auxiliary, properties);
            // TODO Licensing
            // TODO Provider
            FillInstrument(auxiliary, properties);
            FillBasicsProperties(auxiliary, properties);

            return properties;
        }

        private void FillInstrument(Product auxiliary, Dictionary<string, object> properties)
        {

            properties.Remove("platform");
            properties.Remove("mission");
            properties.Add("mission", MISSION.ToLower());
            properties.Add("platform", auxiliary.SourceAttributes.Satellite.ToLower());

            // instruments
            properties.Remove("instruments");
            properties.Add("instruments", new string[] { auxiliary.SourceAttributes.Sensor.ToLower() });

            properties.Remove("sensor_type");
            properties.Add("sensor_type", "radar");

            double gsd = GetResolution(auxiliary);

            if (gsd != 0)
            {
                properties.Remove("gsd");
                properties.Add("gsd", gsd);
            }
        }

        private void FillDateTimeProperties(Product auxiliary, Dictionary<string, object> properties)
        {

            CultureInfo provider = CultureInfo.InvariantCulture;
            string format = "yyyy-MM-ddTHH:mm:ssZ";
            DateTime startDate = auxiliary.SourceAttributes.RawDataStartTime;

            // remove previous values
            properties.Remove("datetime");
            properties.Remove("start_datetime");
            properties.Remove("end_datetime");
            properties.Remove("created");
            properties.Remove("updated");

            properties.Add("datetime", startDate.ToUniversalTime().ToString(format));
            properties.Add("created", startDate.ToUniversalTime().ToString(format));
            properties.Add("updated", DateTime.UtcNow.ToString(format));
        }

        private void FillBasicsProperties(Product auxiliary, IDictionary<string, object> properties)
        {
            // title
            properties.Remove("title");
            properties.Add("title", GetTitle(auxiliary, properties));
        }

        protected string GetTitle(Product auxiliary, IDictionary<string, object> properties)
        {
            return string.Format("{0} {1} {2} {3}",
                //StylePlatform(properties.GetProperty<string>("platform")),
                properties.GetProperty<string>("platform").ToUpper(),
                GetProductType(auxiliary),
                GetInstrumentMode(auxiliary),
                string.Join("/", GetPolarizations(auxiliary))
            );
        }

        private GeoJSON.Net.Geometry.IGeometryObject GetGeometry(Product auxiliary, Kml kml)
        {

            var cString = kml.Document.Folder.GroundOverlay.LatLonQuad.Coordinates;
            var cStringArray = cString.Split(' ');
            var coords = cStringArray.Select(item => item.Split(','));

            var p0lat = double.Parse(coords.ElementAt(0)[1]);
            var p0lon = double.Parse(coords.ElementAt(0)[0]);
            var p1lat = double.Parse(coords.ElementAt(1)[1]);
            var p1lon = double.Parse(coords.ElementAt(1)[0]);
            var p2lat = double.Parse(coords.ElementAt(2)[1]);
            var p2lon = double.Parse(coords.ElementAt(2)[0]);
            var p3lat = double.Parse(coords.ElementAt(3)[1]);
            var p3lon = double.Parse(coords.ElementAt(3)[0]);

            GeoJSON.Net.Geometry.LineString lineString = new GeoJSON.Net.Geometry.LineString(
                new GeoJSON.Net.Geometry.Position[5]{
                    new GeoJSON.Net.Geometry.Position(p0lat,p0lon),
                    new GeoJSON.Net.Geometry.Position(p1lat,p1lon),
                    new GeoJSON.Net.Geometry.Position(p2lat,p2lon),
                    new GeoJSON.Net.Geometry.Position(p3lat,p3lon),
                    new GeoJSON.Net.Geometry.Position(p0lat,p0lon)
                }
            );
            return new GeoJSON.Net.Geometry.Polygon(new GeoJSON.Net.Geometry.LineString[] { lineString }).NormalizePolygon();

        }


        /// <summary>Deserialize Product from xml to class</summary>
        /// <param name="auxiliaryFile">The <see cref="StreamWrapper"/> instance linked to the metadata file.</param>
        /// <returns>The deserialized metadata object.</returns>
        public static async Task<Product> DeserializeProduct(IStreamResource auxiliaryFile)
        {
            XmlSerializer ser = new XmlSerializer(typeof(Product));
            Product auxiliary;
            using (var stream = await auxiliaryFile.GetStreamAsync(System.Threading.CancellationToken.None))
            {
                using (XmlReader reader = XmlReader.Create(stream))
                {
                    auxiliary = (Product)ser.Deserialize(reader);
                }
            }
            return auxiliary;
        }

        public static async Task<Kml> DeserializeKml(IStreamResource auxiliaryFile)
        {
            XmlSerializer ser = new XmlSerializer(typeof(Kml));
            Kml auxiliary;
            using (var stream = await auxiliaryFile.GetStreamAsync(System.Threading.CancellationToken.None))
            {
                using (XmlReader reader = XmlReader.Create(stream))
                {
                    auxiliary = (Kml)ser.Deserialize(reader);
                }
            }
            return auxiliary;
        }

        public override bool CanProcess(IResource route, IDestination destinations)
        {
            if (!(route is IItem item)) return false;
            IAsset auxFile = FindFirstAssetFromFileNameRegex(item, "[0-9a-zA-Z_-]*(product.xml)$");
            try
            {
                DeserializeProduct(resourceServiceProvider.GetStreamResourceAsync(auxFile, System.Threading.CancellationToken.None).GetAwaiter().GetResult()).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                return false;//"There was an error reflecting property 'SourceAttributes'."
            }
            if (auxFile == null)
            {
                return false;
            }

            return true;
        }


    }

}
