using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Mime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using ProjNet.CoordinateSystems;
using Stac;
using Stac.Extensions.Sar;
using Stac.Extensions.Processing;
using Stac.Extensions.Projection;
using Stac.Extensions.Sat;
using Stac.Extensions.View;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Model.Stac;
using Stac.Extensions.Raster;

namespace Terradue.Stars.Data.Model.Metadata.CosmoSkymed
{
    public class CosmoSkymedMetadataExtractor : MetadataExtraction
    {
        // Possible identifiers:
        // CSKS4_SCS_B_HI_16_HH_RA_FF_20211016045150_20211016045156
        private Regex identifierRegex = new Regex(@"(?'id'CSKS(?'i'\d)_(?'pt'RAW_B|SCS_B|SCS_U|DGM_B|GEC_B|GTC_B)_(?'mode'HI|PP|WR|HR|S2)_(?'swath'..)_(?'pol'HH|VV|HV|VH|CO|CH|CV)_(?'look'L|R)(?'dir'A|D)_.._\d{14}_\d{14})");
        private Regex coordinateRegex = new Regex(@"(?'lat'[^ ]+) (?'lon'[^ ]+)");
        private static Regex h5dumpValueRegex = new Regex(@".*\(0\): *(?'value'.*)");

        public static XmlSerializer metadataSerializer = new XmlSerializer(typeof(Schemas.Metadata));
 
        public override string Label => "COSMO SkyMed (ASI) mission product metadata extractor";

        public CosmoSkymedMetadataExtractor(ILogger<CosmoSkymedMetadataExtractor> logger) : base(logger)
        {
        }

        public override bool CanProcess(IResource route, IDestination destination)
        {
            IItem item = route as IItem;
            if (item == null) return false;
            try
            {   
                IAsset metadataAsset = GetMetadataAsset(item);
                Schemas.Metadata metadata = ReadMetadata(metadataAsset).GetAwaiter().GetResult();

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        protected override async Task<StacNode> ExtractMetadata(IItem item, string suffix)
        {
            IAsset metadataAsset = GetMetadataAsset(item);
            Schemas.Metadata metadata = await ReadMetadata(metadataAsset);

            Match identifierMatch = identifierRegex.Match(metadata.ProductInfo.ProductName);
            if (!identifierMatch.Success)
            {
                throw new InvalidOperationException(String.Format("Identifier not recognised from image name: {0}", metadata.ProductInfo.ProductName));
            }

            StacItem stacItem = CreateStacItem(metadata, identifierMatch);

            AddAssets(stacItem, item, metadata, identifierMatch);

            AddSarStacExtension(stacItem, metadata, identifierMatch);
            AddSatStacExtension(stacItem, metadata, identifierMatch);
            AddProjStacExtension(stacItem, metadata, identifierMatch);
            AddViewStacExtension(stacItem, metadata, identifierMatch);
            AddProcessingStacExtension(stacItem, metadata, identifierMatch);
            // FillAdditionalSarProperties(stacItem.Properties, metadata, identifierMatch);
            FillAdditionalSatProperties(stacItem, item, metadata, identifierMatch);
            //FillBasicsProperties(stacItem.Properties, metadata);

            return StacItemNode.Create(stacItem, item.Uri);;
        }

        internal virtual StacItem CreateStacItem(Schemas.Metadata metadata, Match identifierMatch)
        {

            string identifier = identifierMatch.Groups["id"].Value;
            StacItem stacItem = new StacItem(identifier, GetGeometry(metadata), GetCommonMetadata(metadata));
            
            return stacItem;
        }

        private GeoJSON.Net.Geometry.IGeometryObject GetGeometry(Schemas.Metadata metadata)
        {
            double[] bottomLeft, bottomRight, topRight, topLeft;

            // Coordinates are given as lat/lon
            bottomLeft = GetCoordinates(metadata.ProductDefinitionData.GeoCoordBottomLeft);
            bottomRight = GetCoordinates(metadata.ProductDefinitionData.GeoCoordBottomRight);
            topRight = GetCoordinates(metadata.ProductDefinitionData.GeoCoordTopRight);
            topLeft = GetCoordinates(metadata.ProductDefinitionData.GeoCoordTopLeft);
            GeoJSON.Net.Geometry.LineString lineString = new GeoJSON.Net.Geometry.LineString(
                new GeoJSON.Net.Geometry.Position[5]{
                    new GeoJSON.Net.Geometry.Position(bottomLeft[0], bottomLeft[1]),
                    new GeoJSON.Net.Geometry.Position(bottomRight[0], bottomRight[1]),
                    new GeoJSON.Net.Geometry.Position(topRight[0], topRight[1]),
                    new GeoJSON.Net.Geometry.Position(topLeft[0], topLeft[1]),
                    new GeoJSON.Net.Geometry.Position(bottomLeft[0], bottomLeft[1])
                }
            );
            return new GeoJSON.Net.Geometry.Polygon(new GeoJSON.Net.Geometry.LineString[] { lineString });
        }


        private double[] GetCoordinates(string input)
        {
            Match coordinateMatch = coordinateRegex.Match(input);
            if (!coordinateMatch.Success)
            {
                throw new InvalidOperationException(String.Format("Invalid coordinate input: {0}", input));
            }

            if (!Double.TryParse(coordinateMatch.Groups["lat"].Value, out double lat) || !Double.TryParse(coordinateMatch.Groups["lon"].Value, out double lon))
            {
                throw new InvalidOperationException(String.Format("Invalid coordinate value: {0}", input));
            }

            return new double[] { lat, lon };

        }


        protected virtual IAsset GetMetadataAsset(IItem item)
        {
            IAsset metadataAsset = FindFirstAssetFromFileNameRegex(item, @"DFDN.*\.h5\.xml$");
            if (metadataAsset == null)
            {
                throw new FileNotFoundException(String.Format("Unable to find the metadata file asset"));
            }
            return metadataAsset;
        }

        public virtual async Task<Schemas.Metadata> ReadMetadata(IAsset metadataAsset)
        {
            logger.LogDebug("Opening metadata file {0}", metadataAsset.Uri);

            using (var stream = await metadataAsset.GetStreamable().GetStreamAsync())
            {
                XmlReaderSettings settings = new XmlReaderSettings() { DtdProcessing = DtdProcessing.Ignore };
                var reader = XmlReader.Create(stream, settings);
                
                logger.LogDebug("Deserializing metadata file {0}", metadataAsset.Uri);

                return (Schemas.Metadata)metadataSerializer.Deserialize(reader);
            }
        }


        private IDictionary<string, object> GetCommonMetadata(Schemas.Metadata metadata)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();

            FillDateTimeProperties(properties, metadata);
            // TODO Licensing
            // TODO Provider
            FillInstrument(properties, metadata);
            FillBasicsProperties(properties, metadata);

            return properties;
        }

        private void FillDateTimeProperties(Dictionary<string, object> properties, Schemas.Metadata metadata)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            DateTime startDate = DateTime.MinValue;
            bool hasStartDate = DateTime.TryParse(metadata.ProductDefinitionData.SceneSensingStartUTC, null, DateTimeStyles.AssumeUniversal, out startDate);
            DateTime endDate = startDate;
            bool hasEndDate = DateTime.TryParse(metadata.ProductDefinitionData.SceneSensingStopUTC, null, DateTimeStyles.AssumeUniversal, out endDate);

            if (hasStartDate && hasEndDate)
            {
                properties["start_datetime"] = startDate.ToUniversalTime();
                properties["end_datetime"] = endDate.ToUniversalTime();
                properties["datetime"] = startDate.ToUniversalTime();
            }
            else if (hasStartDate)
            {
                properties["datetime"] = startDate.ToUniversalTime();
            }

            DateTime createdDate = DateTime.MinValue;
            bool hasCreatedDate = DateTime.TryParse(metadata.ProductInfo.ProductGenerationDate, null, DateTimeStyles.AssumeUniversal, out createdDate);

            if (hasCreatedDate)
            {
                properties["created"] = createdDate.ToUniversalTime();
            }
        }


        private void FillInstrument(Dictionary<string, object> properties, Schemas.Metadata metadata)
        {
            // platform & constellation
            
            properties["platform"] = metadata.ProductDefinitionData.SatelliteId.ToLower().Replace("csks", "csk");
            properties["mission"] = metadata.ProductInfo.MissionId.ToLower();
            properties["instruments"] = new string[] { "sar-x" };
            properties["sensor_type"] = "radar";
        }

        private void FillBasicsProperties(IDictionary<String, object> properties, Schemas.Metadata metadata)
        {
            CultureInfo culture = new CultureInfo("fr-FR");
            properties["title"] = String.Format("{0} {1} {2}",
                metadata.ProductDefinitionData.SatelliteId,
                metadata.ProcessingInfo.ProcessingLevel,
                properties.GetProperty<DateTime>("datetime").ToUniversalTime().ToString("G", culture)
            );
        }

        private void FillAdditionalSarProperties(IDictionary<String, object> properties, Schemas.Metadata metadata, Match identifierMatch) {
            int? multiLookSpacingRange = null;

            switch (identifierMatch.Groups["mode"].Value)
            {
                case "HI":
                    multiLookSpacingRange = 5;
                    break;
                case "PP":
                    multiLookSpacingRange = 20;
                    break;
                case "WR":
                    multiLookSpacingRange = 30;
                    break;
                case "HR":
                    multiLookSpacingRange = 100;
                    break;
                case "S2":
                    multiLookSpacingRange = 1;
                    break;
            }

            if (multiLookSpacingRange != null)
            {
                properties["sar:multilook_spacing_range"] = multiLookSpacingRange;
                properties["sar:multilook_spacing_azimuth"] = multiLookSpacingRange;
            }
        }


        private void AddSatStacExtension(StacItem stacItem, Schemas.Metadata metadata, Match identifierMatch)
        {
            var sat = new SatStacExtension(stacItem);
            sat.OrbitState = (identifierMatch.Groups["dir"].Value == "A" ? "ascending" : "descending");
        }


        private void FillAdditionalSatProperties(StacItem stacItem, IItem item, Schemas.Metadata metadata, Match identifierMatch)
        {
            IAsset imageAsset = FindFirstAssetFromFileNameRegex(item, String.Format("{0}$", metadata.ProductInfo.ProductName));

            if (imageAsset == null) return;

            string hdf5File = imageAsset.Uri.AbsolutePath;

            string orbitNumberStr = GetHdf5Value(hdf5File, "Orbit Number");
            if (orbitNumberStr == null || !Int32.TryParse(orbitNumberStr, out int orbitNumber)) return;

            var sat = stacItem.SatExtension();

            sat.AbsoluteOrbit = orbitNumber;
            sat.RelativeOrbit = Int32.Parse(identifierMatch.Groups["i"].Value) * 1000 + orbitNumber % 237;
        }


        private void AddProjStacExtension(StacItem stacItem, Schemas.Metadata metadata, Match identifierMatch)
        {
            ProjectionStacExtension proj = stacItem.ProjectionExtension();

            string ellipsoidDesignator = GetTagValue(metadata.AncillaryDataReference.Tag, "Ellipsoid Designator");
            if (ellipsoidDesignator == "WGS84")
            {
                proj.Epsg = 4326;
            }
        }


        private void AddViewStacExtension(StacItem stacItem, Schemas.Metadata metadata, Match identifierMatch)
        {
            var view = new ViewStacExtension(stacItem);
            if (metadata.ProductDefinitionData.NearLookAngle != null && Double.TryParse(metadata.ProductDefinitionData.NearLookAngle, out double nearLookAngle))
            {
                view.IncidenceAngle = Math.Abs(nearLookAngle);
                // Note: according to README it should be an array of these:
                // [
                //    /DeliveryNote/ProductDefinitionData/NearLookAngle,
                //    /DeliveryNote/ProductDefinitionData/FarLookAngle
                // ]
            } 
        }


        private void AddSarStacExtension(StacItem stacItem, Schemas.Metadata metadata, Match identifierMatch)
        {
            SarStacExtension sar = stacItem.SarExtension();
            if (identifierMatch.Groups["look"].Value == "L")
                sar.ObservationDirection = ObservationDirection.Left;
            else if (identifierMatch.Groups["look"].Value == "R")
                sar.ObservationDirection = ObservationDirection.Right;
            sar.InstrumentMode = metadata.ProductDefinitionData.AcquisitionMode;
            sar.FrequencyBand = SarCommonFrequencyBandName.X;
            sar.CenterFrequency = 9.6;

            sar.Polarizations = new string[] {
                identifierMatch.Groups["pol"].Value
            };

            sar.ProductType = metadata.ProductDefinitionData.ProductType;
            if (metadata.ProductCharacteristics.GroundRangeGeometricResolution != null && Double.TryParse(metadata.ProductCharacteristics.GroundRangeGeometricResolution, out double resolutionRange))
            {
                sar.ResolutionRange = resolutionRange;
                sar.PixelSpacingRange = resolutionRange / 2;
                sar.PixelSpacingAzimuth = resolutionRange / 2;
            }
            if (metadata.ProductCharacteristics.AzimuthGeometricResolution != null && Double.TryParse(metadata.ProductCharacteristics.AzimuthGeometricResolution, out double resolutionAzimuth))
            {
                sar.ResolutionAzimuth = resolutionAzimuth;
            }

            string looksRangeStr = GetTagValue(metadata.Algorithms.Tag, "Range Processing Number of Looks");
            string looksAzimuthStr = GetTagValue(metadata.Algorithms.Tag, "Azimuth Processing Number of Looks");
            string looksEquivalentNumberStr = GetTagValue(metadata.Algorithms.Tag, "Equivalent Number of Looks");
            
            if (Double.TryParse(looksRangeStr, out double looksRange))
            {
                sar.LooksRange = looksRange;
            }
            if (Double.TryParse(looksAzimuthStr, out double looksAzimuth))
            {
                sar.LooksAzimuth = looksAzimuth;
            }
            if (Double.TryParse(looksEquivalentNumberStr, out double looksEquivalentNumber))
            {
                sar.LooksEquivalentNumber = looksEquivalentNumber;
            }
        }


        private void AddProcessingStacExtension(StacItem stacItem, Schemas.Metadata metadata, Match identifierMatch)
        {
            var proc = stacItem.ProcessingExtension();
            string level = metadata.ProcessingInfo.ProcessingLevel;
            if (level != null) level = level.Replace("Level-", "L");
            proc.Level = level;
        }


        private string GetTagValue(Schemas.Tag[] tags, string tagName)
        {
            foreach (Schemas.Tag tag in tags)
            {
                if (tag.TagName == tagName) return tag.TagValue;
            }
            return null;
        }



        protected void AddAssets(StacItem stacItem, IItem item, Schemas.Metadata metadata, Match identifierMatch)
        {
            string imageFile = metadata.ProductInfo.ProductName;

            IAsset imageAsset = FindFirstAssetFromFileNameRegex(item, String.Format("{0}$", metadata.ProductInfo.ProductName));
            
            stacItem.Assets.Add("image", StacAsset.CreateDataAsset(stacItem, imageAsset.Uri, new ContentType("application/x-hdf5"), "Image file"));
            stacItem.Assets["image"].Properties.AddRange(imageAsset.Properties);
            stacItem.Assets["image"].Properties["sar:polarizations"] = new string[] {
                identifierMatch.Groups["pol"].Value
            };
            
            IAsset metadataAsset = GetMetadataAsset(item);
            stacItem.Assets.Add("metadata", StacAsset.CreateMetadataAsset(stacItem, metadataAsset.Uri, new ContentType(MimeTypes.GetMimeType(metadataAsset.Uri.OriginalString)), "Metadata file"));
            stacItem.Assets["metadata"].Properties.AddRange(metadataAsset.Properties);

            IAsset additionalMetadataAsset = FindFirstAssetFromFileNameRegex(item, String.Format("DFAS_{0}_CSK_AccompanyingSheet\\.xml$", metadata.ProductInfo.UserRequestId));
            if (additionalMetadataAsset != null)
            {
                stacItem.Assets.Add("metadata-acc", StacAsset.CreateMetadataAsset(stacItem, additionalMetadataAsset.Uri, new ContentType(MimeTypes.GetMimeType(additionalMetadataAsset.Uri.OriginalString)), "Accompanying sheet"));
                stacItem.Assets["metadata-acc"].Properties.AddRange(additionalMetadataAsset.Properties);
            }
        }


        private string GetHdf5Value(string hdf5File, string attribute) {

            Process h5dumpProcess = new Process();
            ProcessStartInfo h5dumpStartInfo = new ProcessStartInfo();


            h5dumpStartInfo.FileName = "h5dump";
            h5dumpStartInfo.RedirectStandardError = true;
            h5dumpStartInfo.RedirectStandardOutput = true;
            h5dumpStartInfo.UseShellExecute = false;
            // ncDumpStartInfo.Arguments = @"-c 'ncdump -v lat_01,lon_01 " + ncFile + "'";
            h5dumpStartInfo.Arguments = String.Format("-a \"Orbit Number\" \"{0}\"", hdf5File);
            h5dumpProcess.EnableRaisingEvents = true;
            h5dumpProcess.StartInfo = h5dumpStartInfo;

            string errorMessage = String.Empty;
            string value = null;

            h5dumpProcess.ErrorDataReceived += new DataReceivedEventHandler(
                delegate (object sender, DataReceivedEventArgs e) {
                    if (e.Data == null) return;

                    errorMessage += e.Data;
                }
            );

            h5dumpProcess.OutputDataReceived += new DataReceivedEventHandler(
                delegate (object sender, DataReceivedEventArgs e) {
                    if (e.Data == null) return;

                    string line = e.Data.Trim();
                    Match match = h5dumpValueRegex.Match(line);
                    if (match.Success)
                    {
                        value = match.Groups["value"].Value;
                        return;
                    }
                }
            );

            h5dumpProcess.Start();
            h5dumpProcess.BeginOutputReadLine();
            h5dumpProcess.WaitForExit();

            int exitCode = h5dumpProcess.ExitCode;

            //Now we need to see if the process was successful
            if (exitCode > 0 & !h5dumpProcess.HasExited) {
                h5dumpProcess.Kill();
            }

            return value;
        }

    }

}
