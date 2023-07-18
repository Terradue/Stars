using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using Stac;
using Stac.Extensions.Processing;
using Stac.Extensions.Projection;
using Stac.Extensions.Sat;
using Stac.Extensions.Sar;
using Stac.Extensions.View;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Geometry.GeoJson;
using Terradue.Stars.Data.Model.Shared;
using Terradue.Stars.Services.Processing;

using Microsoft.Extensions.DependencyInjection;
using System.IO.Abstractions;
using Terradue.Stars.Services.Supplier.Destination;
using Terradue.Stars.Services.Supplier.Carrier;

namespace Terradue.Stars.Data.Model.Metadata.Saocom1
{
    public class Saocom1MetadataExtractor : MetadataExtraction
    {
        public static XmlSerializer metadataSerializer = new XmlSerializer(typeof(SAOCOM_XMLProduct));
        public static XmlSerializer manifestSerializer = new XmlSerializer(typeof(XEMT));
        private readonly IFileSystem _fileSystem;
        private readonly CarrierManager _carrierManager;

        public override string Label => "SAR Observation & Communications Satellite (CONAE) constellation product metadata extractor";

        public Saocom1MetadataExtractor(ILogger<Saocom1MetadataExtractor> logger, IResourceServiceProvider resourceServiceProvider, IFileSystem fileSystem, CarrierManager carrierManager) : base(logger, resourceServiceProvider)
        {
            _fileSystem = fileSystem;
            _carrierManager = carrierManager;
        }


        public override bool CanProcess(IResource route, IDestination destination)
        {
            IItem item = route as IItem;
            if (item == null) return false;
            try
            {
                IAsset metadataAsset = GetMetadataAsset(item);
                if (metadataAsset == null)
                {
                    IAsset zipAsset = GetZipAsset(item);
                    return (zipAsset != null);
                }
                else 
                {
                    SAOCOM_XMLProduct metadata = ReadMetadata(metadataAsset).GetAwaiter().GetResult();
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }


        protected override async Task<StacNode> ExtractMetadata(IItem item, string suffix)
        {
            IAsset manifestAsset = GetManifestAsset(item);
            IAsset zipAsset = GetZipAsset(item);
            XEMT manifest = await ReadManifest(manifestAsset);
            IAssetsContainer extractedAssets = null;

            if (zipAsset != null)
            {
                ZipArchiveAsset zipArchiveAsset = new ZipArchiveAsset(zipAsset, logger, resourceServiceProvider, _fileSystem);
                var tmpDestination = LocalFileDestination.Create(_fileSystem.Directory.CreateDirectory(Path.GetDirectoryName(zipArchiveAsset.Uri.AbsolutePath)), item);
                //var tmpArchiveAssetDestination = tmpDestination.To(zipAsset, Guid.NewGuid().ToString());
                //tmpArchiveAssetDestination.PrepareDestination();

                extractedAssets = await zipArchiveAsset.ExtractToDestinationAsync(tmpDestination, _carrierManager, System.Threading.CancellationToken.None);
            }

            if (extractedAssets != null && item is StacItemNode)
            {
                StacItem innerStacItem = (item as StacItemNode).StacItem;
                
                foreach (var kvp in extractedAssets.Assets) innerStacItem.Assets[kvp.Key] = new StacAsset(innerStacItem, kvp.Value.Uri);
                item = StacItemNode.Create(innerStacItem, item.Uri) as StacItemNode;
            }

            IAsset metadataAsset = GetMetadataAsset(item);

            SAOCOM_XMLProduct metadata = await ReadMetadata(metadataAsset);

            IAsset kmlAsset = FindFirstAssetFromFileNameRegex(item, @"(slc|di|gec|gtc)-.*\.kml");
            if (kmlAsset == null) return null;

            Kml kml = null;
            IStreamResource kmlFileStreamable = await resourceServiceProvider.GetStreamResourceAsync(kmlAsset, System.Threading.CancellationToken.None);
            if (kmlFileStreamable != null)
            {
                kml = await DeserializeKml(kmlFileStreamable);
            }
            else
            {
                logger.LogError("KML file asset is not streamable, skipping geometry extraction");
            }

            StacItem stacItem = CreateStacItem(metadata, manifest, item, kml);
            await AddAssets(stacItem, item, manifestAsset);

            var stacNode = StacItemNode.Create(stacItem, item.Uri);

            return stacNode;
        }

        internal virtual StacItem CreateStacItem(SAOCOM_XMLProduct metadata, XEMT manifest, IItem item, Kml kml)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            StacItem stacItem = new StacItem(ReadFilename(item), GetGeometry(item, kml, metadata), properties);
            AddSatStacExtension(metadata, stacItem);
            AddProjStacExtension(metadata, stacItem);
            AddViewStacExtension(metadata, manifest, stacItem);
            AddSarStacExtension(metadata, stacItem, item);
            AddProcessingStacExtension(metadata, stacItem);
            GetCommonMetadata(metadata, manifest, properties, item);
            return stacItem;
        }

        private void AddProcessingStacExtension(SAOCOM_XMLProduct metadata, StacItem stacItem)
        {
            var proc = stacItem.ProcessingExtension();
            proc.Level = GetProcessingLevel(metadata);
        }

        private string GetProcessingLevel(SAOCOM_XMLProduct metadata)
        {
            string productData = GetProductType(metadata);
            switch (productData)
            {
                case "GTC":
                    return "L1D";
                case "GEC":
                    return "L1C";
                case "DI":
                    return "L1B";
                case "SLC":
                    return "L1A";
            }
            return "NA";
        }



        private void AddProjStacExtension(SAOCOM_XMLProduct metadata, StacItem stacItem)
        {
            ProjectionStacExtension proj = stacItem.ProjectionExtension();
            proj.Epsg = null;
            proj.Shape = new int[2] { metadata.Channel[0].RasterInfo.Samples, metadata.Channel[0].RasterInfo.Lines };
        }

        private void AddViewStacExtension(SAOCOM_XMLProduct metadata, XEMT manifest, StacItem stacItem)
        {
            var view = new ViewStacExtension(stacItem);
            view.OffNadir = (15 + 50) / 2;

            double incidenceAngle = GetIncidenceAngle(manifest);
            if (incidenceAngle != 0)
            {
                view.IncidenceAngle = incidenceAngle;
            }
        }

        private double GetIncidenceAngle(XEMT manifest)
        {
            if (manifest == null || manifest.Product == null || manifest.Product.Features == null || manifest.Product.Features.Acquisition == null ||
                manifest.Product.Features.Acquisition.Parameters == null || manifest.Product.Features.Acquisition.Parameters.BeamId == null)
            {
                return 0;
            }
            string beamId = manifest.Product.Features.Acquisition.Parameters.BeamId;
            Dictionary<string, double[]> incidenceAngles = new Dictionary<string, double[]> {
                {"S1SP", new double[] {20.7, 25.0}},
                {"S2SP", new double[] {24.9, 29.2}},
                {"S3SP", new double[] {29.1, 33.8}},
                {"S4SP", new double[] {33.7, 38.3}},
                {"S5SP", new double[] {38.2, 41.3}},
                {"S6SP", new double[] {41.3, 44.5}},
                {"S7SP", new double[] {44.6, 47.1}},
                {"S8SP", new double[] {47.2, 48.7}},
                {"S9SP", new double[] {48.8, 50.2}},
                {"S1DP", new double[] {20.7, 25.0}},
                {"S2DP", new double[] {24.9, 29.2}},
                {"S3DP", new double[] {29.1, 33.8}},
                {"S4DP", new double[] {33.7, 38.3}},
                {"S5DP", new double[] {38.2, 41.3}},
                {"S6DP", new double[] {41.3, 44.5}},
                {"S7DP", new double[] {44.6, 47.1}},
                {"S8DP", new double[] {47.2, 48.7}},
                {"S9DP", new double[] {48.8, 50.2}},
                {"S1QP", new double[] {17.6, 19.6}},
                {"S2QP", new double[] {19.5, 21.5}},
                {"S3QP", new double[] {21.4, 23.3}},
                {"S4QP", new double[] {23.2, 25.4}},
                {"S5QP", new double[] {25.3, 27.3}},
                {"S6QP", new double[] {27.2, 29.6}},
                {"S7QP", new double[] {29.6, 31.2}},
                {"S8QP", new double[] {31.2, 33.0}},
                {"S9QP", new double[] {33.0, 34.6}},
                {"S10QP", new double[] {34.6, 35.5}},
                {"TNASP", new double[] {24.9, 38.3}},
                {"TNBSP", new double[] {38.2, 47.1}},
                {"TNADP", new double[] {24.9, 38.3}},
                {"TNBDP", new double[] {38.2, 47.1}},
                {"TNAQP", new double[] {17.6, 27.3}},
                {"TNBQP", new double[] {27.2, 35.5}},
                {"TWSP", new double[] {24.9, 48.7}},
                {"TWDP", new double[] {24.9, 48.7}},
                {"TWQP", new double[] {17.6, 35.5}},
            };
            if (incidenceAngles.Keys.Contains(beamId))
            {
                return (incidenceAngles[beamId][0] + incidenceAngles[beamId][1]) / 2;
            }
            return 0;
        }

        private void AddSatStacExtension(SAOCOM_XMLProduct metadata, StacItem stacItem)
        {
            var sat = new SatStacExtension(stacItem);
            int orbit = 0;
            int.TryParse(metadata.Channel[0].StateVectorData.OrbitNumber, out orbit);
            if (orbit > 0)
                sat.AbsoluteOrbit = orbit;
            orbit = 0;
            int.TryParse(metadata.Channel[0].StateVectorData.Track, out orbit);
            if (orbit > 0)
                sat.RelativeOrbit = orbit;
            sat.OrbitState = metadata.Channel[0].StateVectorData.OrbitDirection.ToLower();

            int absOrbit;
            if (int.TryParse(metadata.Channel[0].StateVectorData.OrbitNumber, out absOrbit))
                sat.AbsoluteOrbit = absOrbit;
        }

        private string GetProductType(SAOCOM_XMLProduct metadata)
        {
            string fileName = metadata.Channel[0].RasterInfo.FileName;
            return fileName.Substring(0, fileName.IndexOf("-")).ToUpper();
        }

        private string GetInstrumentMode(SAOCOM_XMLProduct metadata)
        {
            return metadata.Channel[0].RasterInfo.FileName;
        }

        private SarCommonFrequencyBandName GetFrequencyBand(SAOCOM_XMLProduct metadata)
        {
            return SarCommonFrequencyBandName.X;
        }

        private void AddSarStacExtension(SAOCOM_XMLProduct metadata, StacItem stacItem, IItem item)
        {
            SarStacExtension sar = stacItem.SarExtension();

            var polarizations = GetPolarizations(item);
            sar.Required(GetInstrumentMode(metadata),
                GetFrequencyBand(metadata),
                polarizations,
                GetProductType(metadata)
            );

            string fileName = metadata.Channel[0].RasterInfo.FileName;
            sar.ObservationDirection = ParseObservationDirection(metadata.Channel[0].DataSetInfo.SideLooking);
            sar.CenterFrequency = double.Parse(metadata.Channel[0].DataSetInfo.Fc_hz) / 100000000;
            sar.ProductType = GetProductType(metadata);
            sar.FrequencyBand = SarCommonFrequencyBandName.L;
            sar.PixelSpacingRange = Math.Abs(double.Parse(metadata.Channel[0].RasterInfo.LinesStep.Text));
            sar.PixelSpacingAzimuth = double.Parse(metadata.Channel[0].RasterInfo.SamplesStep.Text);
            // Overwrite with sampling constants values if available
            if (metadata.Channel[0].SamplingConstants != null)
            {
                if (metadata.Channel[0].SamplingConstants.PSrg_m != null)
                {
                    sar.PixelSpacingRange = metadata.Channel[0].SamplingConstants.PSrg_m.Value;
                }
                if (metadata.Channel[0].SamplingConstants.PSaz_m != null)
                {
                    sar.PixelSpacingAzimuth = metadata.Channel[0].SamplingConstants.PSaz_m.Value;
                }
            }
            sar.InstrumentMode = fileName.Split('-')[3].ToUpper().Substring(0, 2);
            sar.LooksEquivalentNumber = 3;

            string acquisitionMode = fileName.Split('-')[3].ToUpper().Substring(0, 2);
            double resolutionRange;
            double resolutionAzimuth;
            
            switch (acquisitionMode) {
                case "SM":
                    resolutionRange = 10;
                    resolutionAzimuth = 10;
                    break;
                case "TW":
                    // full polarization
                    if (polarizations.Length == 4) {
                        resolutionRange = 100;
                        resolutionAzimuth = 100;
                    }
                    // single or dual polarization
                    else {
                        resolutionRange = 50;
                        resolutionAzimuth = 50;
                    }
                    break;
                case "TN":
                    // full polarization
                    if (polarizations.Length == 4) {
                        resolutionRange = 50;
                        resolutionAzimuth = 50;
                    }
                    // single or dual polarization
                    else {
                        resolutionRange = 30;
                        resolutionAzimuth = 30;
                    }
                    break;
                default:
                    resolutionRange = 0;
                    resolutionAzimuth = 0;
                    break;
            }

            // Overwrite with actual values if available
            if (metadata.Channel[0].SwathInfo.NominalResolution != null)
            {
                if (metadata.Channel[0].SwathInfo.NominalResolution.Range != null)
                {
                    resolutionRange = metadata.Channel[0].SwathInfo.NominalResolution.Range.Value;
                }
                if (metadata.Channel[0].SwathInfo.NominalResolution.Azimuth != null)
                {
                    resolutionAzimuth = metadata.Channel[0].SwathInfo.NominalResolution.Azimuth.Value;
                }
            }

            sar.ResolutionRange = resolutionRange;
            sar.ResolutionAzimuth = resolutionAzimuth;
            
        }

        private IDictionary<string, object> GetCommonMetadata(SAOCOM_XMLProduct metadata, XEMT manifest, Dictionary<string, object> properties, IItem item)
        {
            FillDateTimeProperties(metadata, properties);
            FillInstrument(metadata, manifest, properties);
            FillBasicsProperties(metadata, properties, item);
            AddOtherProperties(metadata, properties, item);

            return properties;
        }

        private void FillBasicsProperties(SAOCOM_XMLProduct metadata, IDictionary<String, object> properties, IItem item)
        {
            CultureInfo culture = new CultureInfo("fr-FR");
            // title
            properties.Remove("title");
            properties.Add("title", string.Format("{0} {1} {2} {3} {4}",
                                                  //StylePlatform(properties.GetProperty<string>("platform")),
                                                  properties.GetProperty<string>("platform").ToUpper(),
                                                  metadata.Channel[0].DataSetInfo.SensorName,
                                                  GetProductType(metadata),
                                                  string.Join("/", GetPolarizations(item)),
                                                  properties.GetProperty<DateTime>("datetime").ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss", culture)));
        }

        private void FillInstrument(SAOCOM_XMLProduct metadata, XEMT manifest, Dictionary<string, object> properties)
        {
            properties.Remove("platform");
            string sensorName = metadata.Channel[0].DataSetInfo.SensorName;
            string serialNumber = (sensorName.Substring(0, 3) == "SAO" ? sensorName.Substring(3) : String.Empty);
            properties.Add("platform", String.Format("saocom-{0}", serialNumber).ToLower());

            properties.Remove("mission");
            properties.Add("mission", "saocom-1");

            properties.Remove("instruments");

            properties.Add("instruments", new string[] { sensorName.ToLower() });

            properties.Remove("sensor_type");
            properties.Add("sensor_type", "radar");

            properties.Remove("gsd");
            double gsd = GetGroundSampleDistance(metadata, manifest);
            if (gsd != 0) properties.Add("gsd", gsd);
        }

        private void FillDateTimeProperties(SAOCOM_XMLProduct metadata, Dictionary<string, object> properties)
        {

            CultureInfo provider = CultureInfo.InvariantCulture;
            DateTime datetime = DateTime.MinValue;
            DateTime.TryParse(metadata.Channel[0].SwathInfo.AcquisitionStartTime, null, DateTimeStyles.AssumeUniversal, out datetime);

            // remove previous values
            properties.Remove("datetime");
            properties.Add("datetime", datetime.ToUniversalTime());

            var format = "dd-MMM-yyyy HH:mm:ss";
            DateTime creationdatetime = DateTime.MinValue;
            DateTime.TryParseExact(metadata.Channel[0].DataSetInfo.ProcessingDate.Split('.')[0], format, null, DateTimeStyles.AssumeUniversal, out creationdatetime);

            if (creationdatetime.Ticks > 0)
            {
                // remove previous values
                properties.Remove("created");
                properties.Add("created", creationdatetime.ToUniversalTime());
            }
        }

        private void AddOtherProperties(SAOCOM_XMLProduct metadata, IDictionary<String, object> properties, IItem item)
        {
            if (IncludeProviderProperty)
            {
                AddSingleProvider(
                    properties,
                    "CONAE", 
                    "The SAOCOM satellite series represents Argentina's approved polarimetric L-band SAR (Synthetic Aperture Radar) constellation of two spacecraft. The SAOCOM-1 mission is composed of two satellites (SAOCOM-1A and -1B) launched consecutively. The overall objective of SAOCOM is to provide an effective Earth observation and disaster monitoring capability.",
                    new StacProviderRole[] { StacProviderRole.producer, StacProviderRole.processor, StacProviderRole.licensor },
                    new Uri("http://saocom.invap.com.ar")
                );
            }
        }

        private async Task<Kml> DeserializeKml(IStreamResource kmlFileStreamable)
        {
            Kml kml = null;
            XmlSerializer ser = new XmlSerializer(typeof(Kml));
            using (var stream = await kmlFileStreamable.GetStreamAsync(System.Threading.CancellationToken.None))
            {
                using (XmlReader reader = XmlReader.Create(stream))
                {
                    kml = (Kml)ser.Deserialize(reader);
                }
            }

            return kml;
        }

        private GeoJSON.Net.Geometry.IGeometryObject GetGeometry(IItem item, Kml kml, SAOCOM_XMLProduct metadata)
        {
            GeoJSON.Net.Geometry.Position[] lineStringPositions = null;
            if (metadata.Channel != null && metadata.Channel.Length != 0 && metadata.Channel[0].GroundCornerPoints != null)
            {
                lineStringPositions = new GeoJSON.Net.Geometry.Position[5];
                GroundCornerPoints groundCornerPoints = metadata.Channel[0].GroundCornerPoints;
                Coordinates[] coordinates = new Coordinates[] {
                    groundCornerPoints.NorthWest,
                    groundCornerPoints.NorthEast,
                    groundCornerPoints.SouthEast,
                    groundCornerPoints.SouthWest,
                };
                for (int i = 0; i < coordinates.Length; i++)
                {
                    double lat = Double.Parse(coordinates[i].Point.Val[0].Text);
                    double lon = Double.Parse(coordinates[i].Point.Val[1].Text);
                    lineStringPositions[i] = new GeoJSON.Net.Geometry.Position(lat, lon);
                }
                lineStringPositions[coordinates.Length] = lineStringPositions[0];
            }
            else if (kml != null)
            {
                string coordStr = kml.GroundOverlay.LatLonQuad.Coordinates;
                string[] coordStrArray = coordStr.Split(' ');

                lineStringPositions = new GeoJSON.Net.Geometry.Position[coordStrArray.Length + 1];

                for (int i = 0; i < coordStrArray.Length; i++)
                {
                    string[] parts = coordStrArray[i].Split(',');
                    double lon = Double.Parse(parts[0]);
                    double lat = Double.Parse(parts[1]);
                    lineStringPositions[i] = new GeoJSON.Net.Geometry.Position(lat, lon);
                }
                lineStringPositions[coordStrArray.Length] = lineStringPositions[0];
            }

            if (lineStringPositions != null)
            {
                GeoJSON.Net.Geometry.LineString lineString = new GeoJSON.Net.Geometry.LineString(lineStringPositions);
                return new GeoJSON.Net.Geometry.Polygon(new GeoJSON.Net.Geometry.LineString[] { lineString }).NormalizePolygon();
            }

            return null;
        }

        private string ReadFilename(IItem item)
        {
            var parameterFile = FindFirstAssetFromFileNameRegex(item, @".*parameter.*\.xml");
            var xDoc = XDocument.Load(resourceServiceProvider.GetAssetStreamAsync(parameterFile, System.Threading.CancellationToken.None).GetAwaiter().GetResult());
            XNamespace np = "http://www.conae.gov.ar/CGSS/XPNet";
            XName xoutput = np + "output";
            XName nValue = np + "value";
            XName nName = np + "name";
            var outputNodes = xDoc.Descendants(xoutput);
            char minimumChar = 'A';
            string output = "NA";
            foreach (XElement node in outputNodes)
            {
                if (!node.Descendants(nValue).FirstOrDefault().Value.Contains("AN"))
                {
                    var nameField = node.Descendants(nName).FirstOrDefault().Value;
                    char processingLevelChar = nameField.Substring(nameField.IndexOf("L1"))[2];

                    if (nameField.Contains("L1") && minimumChar.CompareTo(processingLevelChar) <= 0)
                    {
                        var input = node.Descendants(nValue).FirstOrDefault().Value;
                        output = input.Substring(input.LastIndexOf('/') + 1, input.IndexOf(".") - input.IndexOf('/') - 1);
                        break;
                    }
                }
            }
            return output;
        }



        protected async Task AddAssets(StacItem stacItem, IItem item, IAsset manifestAsset)
        {

            logger.LogDebug("Retrieving the metadata files");
            IAsset dataAsset, metadataAsset;
            KeyValuePair<string, StacAsset> bandStacAsset;
            StacAsset stacAsset;
            
            if (manifestAsset != null)
            {
                stacItem.Assets.Add("manifest", StacAsset.CreateMetadataAsset(stacItem, manifestAsset.Uri, new ContentType("application/xml"), "Manifest (XEMT)"));
                stacItem.Assets["manifest"].Properties.AddRange(manifestAsset.Properties);
            }

            foreach (var val in new string[] { "vv", "vh", "hh", "hv" })
            {
                metadataAsset = FindFirstAssetFromFileNameRegex(item, @"(slc|di|gec|gtc)-.*" + val + @".*\.xml");
                if (metadataAsset == null) continue;
                stacItem.Assets.Add("metadata-" + val, StacAsset.CreateMetadataAsset(stacItem, metadataAsset.Uri,
                        new ContentType(MimeTypes.GetMimeType(metadataAsset.Uri.ToString()))));
                stacItem.Assets["metadata-" + val].Properties.AddRange(metadataAsset.Properties);
                dataAsset = FindFirstAssetFromFileNameRegex(item, Path.GetFileNameWithoutExtension(metadataAsset.Uri.ToString()) + "$");
                if (dataAsset != null)
                {
                    stacAsset = StacAsset.CreateDataAsset(stacItem, dataAsset.Uri, new ContentType("image/tiff; application=geotiff"));
                    stacAsset.Properties.AddRange(dataAsset.Properties);
                    bandStacAsset = new KeyValuePair<string, StacAsset>("amplitude-" + val, stacAsset);
                    stacItem.Assets.Add(bandStacAsset.Key, stacAsset);
                    stacItem.Assets[bandStacAsset.Key].Roles.Add("amplitude");
                    stacItem.Assets[bandStacAsset.Key].Properties.Add("sar:polarizations", new string[] { val.ToUpper() });
                }
            }

            var overview = FindFirstAssetFromFileNameRegex(item, @".*(gtc)-acqId.*\.png");
            if (overview != null){
                stacItem.Assets.Add("overview", StacAsset.CreateOverviewAsset(stacItem, overview.Uri,
                            new ContentType(MimeTypes.GetMimeType(overview.Uri.ToString()))));
                stacItem.Assets["overview"].Properties.AddRange(overview.Properties);
            }

        }

        private string[] GetPolarizations(IItem item)
        {
            List<string> polarizationList = new List<string>();
            foreach (var val in new string[] { "vv", "vh", "hh", "hv" })
            {
                IAsset metadataAsset = null;
                XmlDocument L1BFileData;
                metadataAsset = FindFirstAssetFromFileNameRegex(item, @"(slc|di|gec|gtc)-.*" + val + @".*\.xml");
                if (metadataAsset != null)
                {
                    L1BFileData = new XmlDocument();
                    L1BFileData.Load(resourceServiceProvider.GetAssetStreamAsync(metadataAsset, System.Threading.CancellationToken.None).GetAwaiter().GetResult());
                    var aa = L1BFileData.SelectSingleNode("/SAOCOM_XMLProduct/Channel/SwathInfo/Polarization");
                    polarizationList.Add(aa.InnerText.Replace("/", "").ToString());
                }
            }
            return polarizationList.ToArray();
        }

        private double GetGroundSampleDistance(SAOCOM_XMLProduct metadata, XEMT manifest)
        {
            AcquisitionParameters parameters = null;

            if (manifest != null && manifest.Product != null && manifest.Product.Features != null && 
                manifest.Product.Features.Acquisition != null && manifest.Product.Features.Acquisition.Parameters != null)
            {
                parameters = manifest.Product.Features.Acquisition.Parameters;

                if (parameters.AcquisitionMode == "SM")
                {
                    return 10;
                }
                if (parameters.AcquisitionMode == "TN")
                {
                    if (parameters.PolarizationMode == "SP" || parameters.PolarizationMode == "DP") return 30;
                    if (parameters.PolarizationMode == "QP") return 50;
                }
                if (parameters.AcquisitionMode == "TW")
                {
                    if (parameters.PolarizationMode == "SP" || parameters.PolarizationMode == "DP") return 50;
                    if (parameters.PolarizationMode == "QP") return 100;
                }
                return 0;
            }
            else
            {
                string fileName = GetInstrumentMode(metadata);
                string acquisitionMode = fileName.Split('-')[3].ToUpper().Substring(0, 2);
                if (acquisitionMode == "SM") {
                    return 10;
                }
                return 0;
            }
        }

        protected virtual IAsset GetMetadataAsset(IItem item)
        {
            IAsset metadataAsset = null;
            metadataAsset = FindFirstAssetFromFileNameRegex(item, @"(slc|di|gec|gtc)-.*\.xml");

            return metadataAsset;
        }


        protected virtual IAsset GetManifestAsset(IItem item)
        {
            IAsset manifestAsset = null;
            manifestAsset = FindFirstAssetFromFileNameRegex(item, @".*\.xemt");
            return manifestAsset;
        }


        protected virtual IAsset GetZipAsset(IItem item)
        {
            IAsset zipAsset = null;
            zipAsset = FindFirstAssetFromFileNameRegex(item, @"S.*\.zip");
            return zipAsset;
        }


        public virtual async Task<SAOCOM_XMLProduct> ReadMetadata(IAsset metadataAsset)
        {
            logger.LogDebug("Opening metadata file {0}", metadataAsset.Uri);

            using (var stream = await resourceServiceProvider.GetAssetStreamAsync(metadataAsset, System.Threading.CancellationToken.None))
            {
                var reader = XmlReader.Create(stream);
                logger.LogDebug("Deserializing metadata file {0}", metadataAsset.Uri);
                return (SAOCOM_XMLProduct)metadataSerializer.Deserialize(reader);
            }
        }

        public virtual async Task<XEMT> ReadManifest(IAsset manifestAsset)
        {
            if (manifestAsset == null)
            {
                logger.LogDebug("No manifest file (XEMT) available");
                return null;
            }
            logger.LogDebug("Opening manifest {0}", manifestAsset.Uri);

            using (var stream = await resourceServiceProvider.GetAssetStreamAsync(manifestAsset, System.Threading.CancellationToken.None))
            {
                //return null;
                var reader = XmlReader.Create(stream);
                logger.LogDebug("Deserializing manifest {0}", manifestAsset.Uri);
                return (XEMT)manifestSerializer.Deserialize(reader);
            }
        }

        public virtual async Task ReadAssetsFromZip(IAsset zipAsset)
        {
            logger.LogDebug("Extracting ZIP archive", zipAsset.Uri);

            Dictionary<string, IAsset> assetsExtracted = new Dictionary<string, IAsset>();
            Ionic.Zip.ZipFile zipFile = Ionic.Zip.ZipFile.Read(zipAsset.Uri.AbsolutePath);
        }

    }
}