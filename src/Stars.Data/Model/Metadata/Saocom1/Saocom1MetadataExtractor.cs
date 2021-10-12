using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using GeoJSON.Net.Geometry;
using Microsoft.Extensions.Logging;
using ProjNet.CoordinateSystems;
using Stac;
using Stac.Extensions.Processing;
using Stac.Extensions.Projection;
using Stac.Extensions.Sat;
using Stac.Extensions.Sar;
using Stac.Extensions.View;
using Stac;
using Terradue.OpenSearch.Sentinel.Data.Safe;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services;
using Terradue.Stars.Services.Model.Stac;
using System.Text;
using System.Xml.Linq;
namespace Terradue.Stars.Data.Model.Metadata.Saocom1
{
    public class Saocom1MetadataExtractor : MetadataExtraction
    {
        public static XmlSerializer metadataSerializer = new XmlSerializer(typeof(SAOCOM_XMLProduct));

        public override string Label => "SAR Observation & Communications Satellite (CONAE) constellation product metadata extractor";

        public Saocom1MetadataExtractor(ILogger<Saocom1MetadataExtractor> logger) : base(logger)
        {
        }


        public override bool CanProcess(IResource route, IDestination destination)
        {
            IItem item = route as IItem;
            if (item == null) return false;
            try
            {
                IAsset metadataAsset = GetMetadataAsset(item);
                SAOCOM_XMLProduct metadata = ReadMetadata(metadataAsset).GetAwaiter().GetResult();
                return true;
            }
            catch
            {
                return false;
            }
        }


        protected override async Task<StacNode> ExtractMetadata(IItem item, string suffix)
        {
            IAsset metadataAsset = GetMetadataAsset(item);
            SAOCOM_XMLProduct metadata = await ReadMetadata(metadataAsset);

            StacItem stacItem = CreateStacItem(metadata, item);
            await AddAssets(stacItem, item, metadata);

            var stacNode = StacItemNode.CreateUnlocatedNode(stacItem);

            return stacNode;
        }

        internal virtual StacItem CreateStacItem(SAOCOM_XMLProduct metadata, IItem item)
        {
            StacItem stacItem = new StacItem(ReadFilename(item), GetGeometry(metadata), GetCommonMetadata(metadata, item));
            AddSatStacExtension(metadata, stacItem);
            AddProjStacExtension(metadata, stacItem);
            AddViewStacExtension(metadata, stacItem);
            AddSarStacExtension(metadata, stacItem, item);
            AddProcessingStacExtension(metadata, stacItem);
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
            proj.Epsg = 32652;
        }

        private void AddViewStacExtension(SAOCOM_XMLProduct metadata, StacItem stacItem)
        {
            var view = new ViewStacExtension(stacItem);
            view.OffNadir = (15 + 50) / 2;
            view.IncidenceAngle = (24.9 + 48.7) / 2;
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
            sar.InstrumentMode = fileName.Split('-')[3].ToUpper().Substring(0,2);
            sar.LooksEquivalentNumber = 3;

            string acquisitionMode = fileName.Split('-')[3].ToUpper().Substring(0,2);
            int resolutionRange;
            int resolutionAzimuth;
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
            sar.ResolutionRange = resolutionRange;
            sar.ResolutionAzimuth = resolutionAzimuth;
            
        }

        private IDictionary<string, object> GetCommonMetadata(SAOCOM_XMLProduct metadata, IItem item)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();

            FillDateTimeProperties(metadata, properties);
            FillInstrument(metadata, properties);
            FillBasicsProperties(metadata, properties, item);

            return properties;
        }

        private void FillBasicsProperties(SAOCOM_XMLProduct metadata, IDictionary<String, object> properties, IItem item)
        {
            CultureInfo culture = new CultureInfo("fr-FR");
            // title
            properties.Remove("title");
            properties.Add("title", string.Format("{0} {1} {2} {3} {4}",
                                                  StylePlatform(properties.GetProperty<string>("platform")),
                                                  metadata.Channel[0].DataSetInfo.SensorName,
                                                  GetProductType(metadata),
                                                  string.Join("/", GetPolarizations(item)),
                                                  properties.GetProperty<DateTime>("datetime").ToUniversalTime().ToString("G", culture)));
        }

        private void FillInstrument(SAOCOM_XMLProduct metadata, Dictionary<string, object> properties)
        {
            properties.Remove("platform");
            properties.Add("platform", "saocom-1a");
            properties.Remove("mission");
            properties.Add("mission", "saocom-1");

            properties.Remove("instruments");

            properties.Add("instruments", new string[] { metadata.Channel[0].DataSetInfo.SensorName.ToLower() });
            properties.Remove("gsd");
            properties.Add("gsd", GetGroundSampleDistance(metadata));
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

        private GeoJSON.Net.Geometry.IGeometryObject GetGeometry(SAOCOM_XMLProduct metadata)
        {
            GeoJSON.Net.Geometry.LineString lineString = new GeoJSON.Net.Geometry.LineString(
                new GeoJSON.Net.Geometry.Position[5]{

                        new GeoJSON.Net.Geometry.Position(double.Parse(metadata.Channel[0].GroundCornerPoints.NorthWest.Point.Val[0].Text),
                        double.Parse(metadata.Channel[0].GroundCornerPoints.NorthWest.Point.Val[1].Text)),

                        new GeoJSON.Net.Geometry.Position(double.Parse(metadata.Channel[0].GroundCornerPoints.NorthEast.Point.Val[0].Text),
                        double.Parse(metadata.Channel[0].GroundCornerPoints.NorthEast.Point.Val[1].Text)),

                        new GeoJSON.Net.Geometry.Position(double.Parse(metadata.Channel[0].GroundCornerPoints.SouthEast.Point.Val[0].Text),
                        double.Parse(metadata.Channel[0].GroundCornerPoints.SouthEast.Point.Val[1].Text)),

                        new GeoJSON.Net.Geometry.Position(double.Parse(metadata.Channel[0].GroundCornerPoints.SouthWest.Point.Val[0].Text),
                        double.Parse(metadata.Channel[0].GroundCornerPoints.SouthWest.Point.Val[1].Text)),

                        new GeoJSON.Net.Geometry.Position(double.Parse(metadata.Channel[0].GroundCornerPoints.NorthWest.Point.Val[0].Text),
                        double.Parse(metadata.Channel[0].GroundCornerPoints.NorthWest.Point.Val[1].Text)),
                }
            );


            return new GeoJSON.Net.Geometry.Polygon(new GeoJSON.Net.Geometry.LineString[] { lineString });
        }

        private string ReadFilename(IItem item)
        {
            var parameterFile = FindFirstAssetFromFileNameRegex(item, @".*parameter.*\.xml");
            var xDoc = XDocument.Load(parameterFile.GetStreamable().GetStreamAsync().GetAwaiter().GetResult());
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

                    if (nameField.Contains("L1") && minimumChar.CompareTo(processingLevelChar) < 0)
                    {
                        var input = node.Descendants(nValue).FirstOrDefault().Value;
                        output = input.Substring(input.LastIndexOf('/') + 1, input.IndexOf(".") - input.IndexOf('/') - 1);
                    }
                }
            }
            return output;
        }



        protected async Task AddAssets(StacItem stacItem, IItem item, SAOCOM_XMLProduct metadata)
        {

            logger.LogDebug("Retrieving the metadata files");
            IAsset dataAsset, metadataAsset;
            KeyValuePair<string, StacAsset> bandStacAsset;
            StacAsset stacAsset;
            foreach (var val in new string[] { "vv", "vh", "hh", "hv" })
            {
                metadataAsset = FindFirstAssetFromFileNameRegex(item, @"(di-|gec|gtc).*" + val + @".*\.xml");
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

            var overview = FindFirstAssetFromFileNameRegex(item, @".*gtc-acqId.*\.png");
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
                metadataAsset = FindFirstAssetFromFileNameRegex(item, @"(di-|gec|gtc).*" + val + @".*\.xml");
                if (metadataAsset != null)
                {
                    L1BFileData = new XmlDocument();
                    L1BFileData.Load(metadataAsset.GetStreamable().GetStreamAsync().GetAwaiter().GetResult());
                    var aa = L1BFileData.SelectSingleNode("/SAOCOM_XMLProduct/Channel/SwathInfo/Polarization");
                    polarizationList.Add(aa.InnerText.Replace("/", "").ToString());
                }
            }
            return polarizationList.ToArray();
        }

        private double GetGroundSampleDistance(SAOCOM_XMLProduct metadata)
        {
            return 50;
        }

        protected virtual IAsset GetMetadataAsset(IItem item)
        {
            IAsset manifestAsset = null;
            manifestAsset = FindFirstAssetFromFileNameRegex(item, @"(di-|gec|gtc).*\.xml");
            if (manifestAsset == null)
                throw new FileNotFoundException(string.Format("Unable to find the metadata file asset"));

            return manifestAsset;
        }


        public virtual async Task<SAOCOM_XMLProduct> ReadMetadata(IAsset manifestAsset)
        {
            logger.LogDebug("Opening Manifest {0}", manifestAsset.Uri);

            using (var stream = await manifestAsset.GetStreamable().GetStreamAsync())
            {
                var reader = XmlReader.Create(stream);
                logger.LogDebug("Deserializing Manifest {0}", manifestAsset.Uri);
                return (SAOCOM_XMLProduct)metadataSerializer.Deserialize(reader);
            }
        }
    }
}