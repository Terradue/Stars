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
using Stac;
using Stac.Extensions.Processing;
using Stac.Extensions.Projection;
using Stac.Extensions.Sar;
using Stac.Extensions.Sat;
using Stac.Extensions.View;
using Terradue.Stars.Geometry.GeoJson;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Model.Stac;

namespace Terradue.Stars.Data.Model.Metadata.CosmoSkymed
{
    public class CosmoSkymedMetadataExtractor : MetadataExtraction
    {
        // Possible identifiers:
        // CSKS4_SCS_B_HI_16_HH_RA_FF_20211016045150_20211016045156
        private Regex identifierRegex = new Regex(@"(?'id'CSKS(?'i'\d)_(?'pt'RAW_B|SCS_B|SCS_U|DGM_B|GEC_B|GTC_B)_(?'mode'HI|PP|WR|HR|S2)_(?'swath'..)_(?'pol'HH|VV|HV|VH|CO|CH|CV)_(?'look'L|R)(?'dir'A|D)_.._\d{14}_\d{14})");
        private Regex coordinateRegex = new Regex(@"(?'lat'[^ ,]+),? (?'lon'[^ ,]+)");
        private static Regex h5dumpValueRegex = new Regex(@".*\(0\): *(?'value'.*)");

        public static XmlSerializer metadataSerializer = new XmlSerializer(typeof(Schemas.Metadata));

        public override string Label => "COSMO SkyMed (ASI) mission product metadata extractor";

        public CosmoSkymedMetadataExtractor(ILogger<CosmoSkymedMetadataExtractor> logger, IResourceServiceProvider resourceServiceProvider) : base(logger, resourceServiceProvider)
        {
        }

        public override bool CanProcess(IResource route, IDestination destination)
        {
            if (!(route is IItem item)) return false;
            try
            {
                IAsset dataAsset = GetDataAsset(item);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        protected override async Task<StacNode> ExtractMetadata(IItem item, string suffix)
        {
            IAsset dataAsset = GetDataAsset(item);
            Dictionary<string, string> hdfAttributes = ReadHdfMetadata(dataAsset).GetAwaiter().GetResult();
            IAsset metadataAsset = GetMetadataAsset(item);
            Schemas.Metadata metadata = await ReadXmlMetadata(metadataAsset);

            string imageFileName = (metadata == null ? dataAsset.Uri.AbsolutePath : metadata.ProductInfo.ProductName);
            Match identifierMatch = identifierRegex.Match(imageFileName);
            if (!identifierMatch.Success)
            {
                throw new InvalidOperationException(string.Format("Identifier not recognised from image file name: {0}", imageFileName));
            }

            StacItem stacItem = CreateStacItem(hdfAttributes, metadata, identifierMatch);

            AddAssets(stacItem, item, hdfAttributes, metadata, identifierMatch);

            AddSarStacExtension(stacItem, hdfAttributes, metadata, identifierMatch);
            AddSatStacExtension(stacItem, hdfAttributes, metadata, identifierMatch);
            AddProjStacExtension(stacItem, hdfAttributes, metadata, identifierMatch);
            AddViewStacExtension(stacItem, hdfAttributes, metadata, identifierMatch);
            AddProcessingStacExtension(stacItem, hdfAttributes, metadata, identifierMatch);
            // FillAdditionalSarProperties(stacItem.Properties, metadata, identifierMatch);
            //FillBasicsProperties(stacItem.Properties, metadata);

            return StacItemNode.Create(stacItem, item.Uri);
        }

        internal virtual StacItem CreateStacItem(Dictionary<string, string> hdfAttributes, Schemas.Metadata metadata, Match identifierMatch)
        {

            string identifier = identifierMatch.Groups["id"].Value;
            StacItem stacItem = new StacItem(identifier, GetGeometry(hdfAttributes, metadata), GetCommonMetadata(hdfAttributes, metadata));

            return stacItem;
        }

        private GeoJSON.Net.Geometry.IGeometryObject GetGeometry(Dictionary<string, string> hdfAttributes, Schemas.Metadata metadata)
        {
            double[] bottomLeft, bottomRight, topRight, topLeft;

            hdfAttributes.TryGetValue("Bottom Left Geodetic Coordinates", out string bottomLeftStr);
            hdfAttributes.TryGetValue("Bottom Right Geodetic Coordinates", out string bottomRightStr);
            hdfAttributes.TryGetValue("Top Right Geodetic Coordinates", out string topRightStr);
            hdfAttributes.TryGetValue("Top Left Geodetic Coordinates", out string topLeftStr);

            if ((bottomLeftStr == null || bottomRightStr == null || topRightStr == null || topLeftStr == null) && metadata != null)
            {
                bottomLeftStr = metadata.ProductDefinitionData.GeoCoordBottomLeft;
                bottomRightStr = metadata.ProductDefinitionData.GeoCoordBottomRight;
                topRightStr = metadata.ProductDefinitionData.GeoCoordTopRight;
                topLeftStr = metadata.ProductDefinitionData.GeoCoordTopLeft;
            }

            if (bottomLeftStr != null && bottomRightStr != null && topRightStr != null && topLeftStr != null)
            {
                // Coordinates are given as lat/lon
                bottomLeft = GetCoordinates(bottomLeftStr);
                bottomRight = GetCoordinates(bottomRightStr);
                topRight = GetCoordinates(topRightStr);
                topLeft = GetCoordinates(topLeftStr);
                GeoJSON.Net.Geometry.LineString lineString = new GeoJSON.Net.Geometry.LineString(
                    new GeoJSON.Net.Geometry.Position[5]{
                        new GeoJSON.Net.Geometry.Position(bottomLeft[0], bottomLeft[1]),
                        new GeoJSON.Net.Geometry.Position(bottomRight[0], bottomRight[1]),
                        new GeoJSON.Net.Geometry.Position(topRight[0], topRight[1]),
                        new GeoJSON.Net.Geometry.Position(topLeft[0], topLeft[1]),
                        new GeoJSON.Net.Geometry.Position(bottomLeft[0], bottomLeft[1])
                    }
                );
                return new GeoJSON.Net.Geometry.Polygon(new GeoJSON.Net.Geometry.LineString[] { lineString }).NormalizePolygon();
            }
            return null;
        }


        private double[] GetCoordinates(string input)
        {
            Match coordinateMatch = coordinateRegex.Match(input);
            if (!coordinateMatch.Success)
            {
                throw new InvalidOperationException(string.Format("Invalid coordinate input: {0}", input));
            }

            if (!double.TryParse(coordinateMatch.Groups["lat"].Value, out double lat) || !double.TryParse(coordinateMatch.Groups["lon"].Value, out double lon))
            {
                throw new InvalidOperationException(string.Format("Invalid coordinate value: {0}", input));
            }

            return new double[] { lat, lon };

        }


        protected virtual IAsset GetMetadataAsset(IItem item)
        {
            IAsset metadataAsset = FindFirstAssetFromFileNameRegex(item, @"DFDN.*\.h5\.xml$");
            return metadataAsset;
        }

        protected virtual IAsset GetDataAsset(IItem item)
        {
            IAsset dataAsset = FindFirstAssetFromFileNameRegex(item, @".*\.h5$") ?? throw new FileNotFoundException(string.Format("Unable to find the HDF5 file asset"));
            return dataAsset;
        }

        public virtual async Task<Schemas.Metadata> ReadXmlMetadata(IAsset metadataAsset)
        {
            if (metadataAsset == null)
            {
                logger.LogDebug("No metadata file available");
                return null;
            }

            logger.LogDebug("Opening metadata file {0}", metadataAsset.Uri);

            using (var stream = await resourceServiceProvider.GetAssetStreamAsync(metadataAsset, System.Threading.CancellationToken.None))
            {
                XmlReaderSettings settings = new XmlReaderSettings() { DtdProcessing = DtdProcessing.Ignore };
                var reader = XmlReader.Create(stream, settings);

                logger.LogDebug("Deserializing metadata file {0}", metadataAsset.Uri);

                return (Schemas.Metadata)metadataSerializer.Deserialize(reader);
            }
        }


        public virtual async Task<Dictionary<string, string>> ReadHdfMetadata(IAsset dataAsset)
        {
            Dictionary<string, string> attributes = new Dictionary<string, string>();
            logger.LogDebug("Reading HDF5 attributes from file {0}", dataAsset.Uri);

            string hdf5File = dataAsset.Uri.AbsolutePath;
            string[] attributeNames = new string[]
            {
                "Scene Sensing Start UTC",
                "Scene Sensing Stop UTC",
                "Product Generation UTC",
                "Satellite ID",
                "Mission ID",
                "Look Side",
                "Acquisition Mode",
                "Radar Frequency",
                "Polarisation",
                "Product Type",
                "Ground Range Geometric Resolution",
                "Azimuth Geometric Resolution",
                "Range Processing Number of Looks",
                "Azimuth Processing Number of Looks",
                "Equivalent Number of Looks",
                "Orbit Number",
                "Orbit Direction",
                "Near Incidence Angle",
                "Far Incidence Angle",
                "Near Look Angle",
                "Processing Centre",
                "L0 Software Version",
                "L1A Software Version",
                "L1B Software Version",
                "L1C Software Version",
                "L1D Software Version",
                "Ellipsoid Designator",
                "Bottom Left Geodetic Coordinates",
                "Bottom Right Geodetic Coordinates",
                "Top Right Geodetic Coordinates",
                "Top Left Geodetic Coordinates",
            };

            foreach (string attributeName in attributeNames)
            {
                string value = GetHdf5Value(hdf5File, attributeName);
                if (value == null) continue;

                attributes.Add(attributeName, value);
            }

            return attributes;
        }


        private IDictionary<string, object> GetCommonMetadata(Dictionary<string, string> hdfAttributes, Schemas.Metadata metadata)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();

            FillDateTimeProperties(properties, hdfAttributes, metadata);
            // TODO Licensing
            // TODO Provider
            FillInstrument(properties, hdfAttributes, metadata);
            FillBasicsProperties(properties, hdfAttributes, metadata);
            AddOtherProperties(properties, hdfAttributes, metadata);

            return properties;
        }

        private void FillDateTimeProperties(Dictionary<string, object> properties, Dictionary<string, string> hdfAttributes, Schemas.Metadata metadata)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;

            bool complete = true;
            DateTime startDate = DateTime.MinValue;
            DateTime endDate = startDate;
            complete &= hdfAttributes.TryGetValue("Scene Sensing Start UTC", out string startDateStr);
            complete &= hdfAttributes.TryGetValue("Scene Sensing Stop UTC", out string endDateStr);

            if (complete)
            {
                complete &= DateTime.TryParse(startDateStr, provider, DateTimeStyles.AssumeUniversal, out startDate);
                complete &= DateTime.TryParse(endDateStr, provider, DateTimeStyles.AssumeUniversal, out endDate);
            }
            else if (metadata != null)
            {
                complete = true;
                complete &= DateTime.TryParse(metadata.ProductDefinitionData.SceneSensingStartUTC, provider, DateTimeStyles.AssumeUniversal, out startDate);
                complete &= DateTime.TryParse(metadata.ProductDefinitionData.SceneSensingStopUTC, provider, DateTimeStyles.AssumeUniversal, out endDate);
            }

            if (complete)
            {
                properties["start_datetime"] = startDate.ToUniversalTime();
                properties["end_datetime"] = endDate.ToUniversalTime();
                properties["datetime"] = startDate.ToUniversalTime();
            }
            else if (startDateStr != null && DateTime.TryParse(startDateStr, provider, DateTimeStyles.AssumeUniversal, out startDate))
            {
                properties["datetime"] = startDate.ToUniversalTime();
            }

            complete = hdfAttributes.TryGetValue("Product Generation UTC", out string createdDateStr);
            complete &= DateTime.TryParse(createdDateStr, provider, DateTimeStyles.AssumeUniversal, out DateTime createdDate);

            if (!complete && metadata != null)
            {
                complete = DateTime.TryParse(metadata.ProductInfo.ProductGenerationDate, null, DateTimeStyles.AssumeUniversal, out createdDate);
            }

            if (complete)
            {
                properties["created"] = createdDate.ToUniversalTime();
            }
        }


        private void FillInstrument(Dictionary<string, object> properties, Dictionary<string, string> hdfAttributes, Schemas.Metadata metadata)
        {
            // platform & constellation
            bool complete = true;
            complete &= hdfAttributes.TryGetValue("Satellite ID", out string platform);
            complete &= hdfAttributes.TryGetValue("Mission ID", out string mission);
            properties["instruments"] = new string[] { "sar-x" };
            properties["sensor_type"] = "radar";

            if (!complete && metadata != null)
            {
                platform = metadata.ProductDefinitionData.SatelliteId;
                mission = (metadata.ProductInfo.MissionId ?? "csk");
            }

            if (platform != null) properties["platform"] = platform.ToLower().Replace("csks", "csk");
            if (mission != null) properties["mission"] = mission.ToLower();
        }

        private void FillBasicsProperties(IDictionary<string, object> properties, Dictionary<string, string> hdfAttributes, Schemas.Metadata metadata)
        {
            CultureInfo culture = new CultureInfo("fr-FR");
            bool complete = true;
            complete &= hdfAttributes.TryGetValue("Satellite ID", out string platform);
            string processingLevel = GetProcessingLevel(hdfAttributes);
            complete &= processingLevel != null;
            if (complete) processingLevel = processingLevel.Replace("L", "Level-");
            if (!complete && metadata != null)
            {
                platform = metadata.ProductDefinitionData.SatelliteId;
                processingLevel = metadata.ProcessingInfo.ProcessingLevel;
            }
            properties["title"] = string.Format("{0} {1} {2}",
                platform,
                processingLevel,
                properties.GetProperty<DateTime>("datetime").ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss", culture)
            );
        }

        private void AddOtherProperties(IDictionary<string, object> properties, Dictionary<string, string> hdfAttributes, Schemas.Metadata metadata)
        {
            if (IncludeProviderProperty)
            {
                AddSingleProvider(
                    properties,
                    "ASI",
                    "COSMO-SkyMed (Constellation of Small Satellites for Mediterranean basin Observation) is a 4 spacecraft constellation. Each of the 4 satellites is equipped with the SAR-2000 Synthetic Aperture Radar, which observes in the X-band to provide global observation under all weather and visibility conditions.",
                    new StacProviderRole[] { StacProviderRole.producer, StacProviderRole.processor, StacProviderRole.licensor },
                    new Uri("https://www.asi.it/en/earth-science/cosmo-skymed/")
                );
            }
        }

        private void FillAdditionalSarProperties(IDictionary<string, object> properties, Dictionary<string, string> hdfAttributes, Schemas.Metadata metadata, Match identifierMatch)
        {
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


        private void AddSatStacExtension(StacItem stacItem, Dictionary<string, string> hdfAttributes, Schemas.Metadata metadata, Match identifierMatch)
        {
            var sat = new SatStacExtension(stacItem);
            if (hdfAttributes.TryGetValue("Orbit Direction", out string orbitDirection)) orbitDirection = orbitDirection.ToLower();
            else if (metadata != null) orbitDirection = (identifierMatch.Groups["dir"].Value == "A" ? "ascending" : "descending");
            sat.OrbitState = orbitDirection;

            if (hdfAttributes.TryGetValue("Orbit Number", out string orbitNumberStr) && int.TryParse(orbitNumberStr, out int orbitNumber))
            {
                sat.AbsoluteOrbit = orbitNumber;
                sat.RelativeOrbit = int.Parse(identifierMatch.Groups["i"].Value) * 1000 + orbitNumber % 237;
            }
        }

        private void AddProjStacExtension(StacItem stacItem, Dictionary<string, string> hdfAttributes, Schemas.Metadata metadata, Match identifierMatch)
        {
            ProjectionStacExtension proj = stacItem.ProjectionExtension();

            if (!hdfAttributes.TryGetValue("Ellipsoid Designator", out string ellipsoidDesignator) && metadata != null)
            {
                ellipsoidDesignator = GetTagValue(metadata.AncillaryDataReference.Tag, "Ellipsoid Designator");
            }
            if (ellipsoidDesignator == "WGS84")
            {
                proj.Epsg = 4326;
            }
        }


        private void AddViewStacExtension(StacItem stacItem, Dictionary<string, string> hdfAttributes, Schemas.Metadata metadata, Match identifierMatch)
        {
            var view = new ViewStacExtension(stacItem);

            bool complete = true;
            complete &= hdfAttributes.TryGetValue("Near Incidence Angle", out string nearAngleStr);
            complete &= hdfAttributes.TryGetValue("Far Incidence Angle", out string farAngleStr);

            if (!complete && metadata != null)
            {
                // use near look angle as before in case incidence angles are not available
                nearAngleStr = metadata.ProductDefinitionData.NearLookAngle;
                farAngleStr = nearAngleStr;
            }

            if (nearAngleStr != null && farAngleStr != null)
            {
                if (double.TryParse(nearAngleStr, out double nearAngle) && double.TryParse(farAngleStr, out double farAngle))
                {
                    view.IncidenceAngle = Math.Abs((nearAngle + farAngle) / 2);
                }
            }

            // Note: according to README it should be an array of these:
            // [
            //    /DeliveryNote/ProductDefinitionData/NearLookAngle,
            //    /DeliveryNote/ProductDefinitionData/FarLookAngle
            // ]
        }


        private void AddSarStacExtension(StacItem stacItem, Dictionary<string, string> hdfAttributes, Schemas.Metadata metadata, Match identifierMatch)
        {
            SarStacExtension sar = stacItem.SarExtension();
            if (hdfAttributes.TryGetValue("Look Side", out string lookSide))
            {
                lookSide = lookSide[0].ToString();
            }
            else
            {
                lookSide = identifierMatch.Groups["look"].Value;
            }
            if (lookSide == "L")
                sar.ObservationDirection = ObservationDirection.Left;
            else if (lookSide == "R")
                sar.ObservationDirection = ObservationDirection.Right;

            if (!hdfAttributes.TryGetValue("Acquisition Mode", out string instrumentMode) && metadata != null)
            {
                instrumentMode = metadata.ProductDefinitionData.AcquisitionMode;
            }
            sar.InstrumentMode = instrumentMode;

            sar.FrequencyBand = SarCommonFrequencyBandName.X;

            if (hdfAttributes.TryGetValue("Radar Frequency", out string radarFrequencyStr) && double.TryParse(radarFrequencyStr, out double radarFrequency))
            {
                sar.CenterFrequency = radarFrequency / 1000000000;
            }
            else
            {
                sar.CenterFrequency = 9.6;
            }

            if (hdfAttributes.TryGetValue("", out string polarisationsStr) && polarisationsStr != null)
            {
                sar.Polarizations = polarisationsStr.Split(' ');
            }
            else
            {
                sar.Polarizations = new string[] {
                    identifierMatch.Groups["pol"].Value
                };
            }

            if (!hdfAttributes.TryGetValue("Product Type", out string productType) && metadata != null)
            {
                productType = metadata.ProductDefinitionData.ProductType;
            }
            sar.ProductType = productType;

            if (!hdfAttributes.TryGetValue("Ground Range Geometric Resolution", out string resolutionRangeStr) || !hdfAttributes.TryGetValue("Azimuth Geometric Resolution", out string resolutionAzimuthStr) && metadata != null)
            {
                resolutionRangeStr = metadata.ProductCharacteristics.GroundRangeGeometricResolution;
                resolutionAzimuthStr = metadata.ProductCharacteristics.AzimuthGeometricResolution;
            }

            if (double.TryParse(resolutionRangeStr, out double resolutionRange) && double.TryParse(resolutionAzimuthStr, out double resolutionAzimuth))
            {
                sar.ResolutionRange = resolutionRange;
                sar.ResolutionAzimuth = resolutionAzimuth;
                sar.PixelSpacingRange = resolutionRange / 2;
                sar.PixelSpacingAzimuth = resolutionRange / 2;
            }

            if (!hdfAttributes.TryGetValue("Range Processing Number of Looks", out string looksRangeStr) && metadata != null)
            {
                looksRangeStr = GetTagValue(metadata.Algorithms.Tag, "Range Processing Number of Looks");
            }
            if (!hdfAttributes.TryGetValue("Azimuth Processing Number of Looks", out string looksAzimuthStr) && metadata != null)
            {
                looksAzimuthStr = GetTagValue(metadata.Algorithms.Tag, "Azimuth Processing Number of Looks");
            }
            if (!hdfAttributes.TryGetValue("Equivalent Number of Looks", out string looksEquivalentNumberStr) && metadata != null)
            {
                looksEquivalentNumberStr = GetTagValue(metadata.Algorithms.Tag, "Equivalent Number of Looks");
            }

            if (double.TryParse(looksRangeStr, out double looksRange))
            {
                sar.LooksRange = looksRange;
            }
            if (double.TryParse(looksAzimuthStr, out double looksAzimuth))
            {
                sar.LooksAzimuth = looksAzimuth;
            }
            if (double.TryParse(looksEquivalentNumberStr, out double looksEquivalentNumber))
            {
                sar.LooksEquivalentNumber = looksEquivalentNumber;
            }
        }


        private void AddProcessingStacExtension(StacItem stacItem, Dictionary<string, string> hdfAttributes, Schemas.Metadata metadata, Match identifierMatch)
        {
            var proc = stacItem.ProcessingExtension();
            string processingLevel = GetProcessingLevel(hdfAttributes);
            if (processingLevel == null && metadata != null)
            {
                processingLevel = metadata.ProcessingInfo.ProcessingLevel;
            }
            if (processingLevel != null)
            {
                proc.Level = processingLevel.Replace("Level-", "L");
            }
            if (!hdfAttributes.TryGetValue("Processing Centre", out string processingCentre) && metadata != null)
            {
                processingCentre = metadata.OtherInfo.ProcessingCentre;
            }
            proc.Facility = processingCentre;
        }


        private string GetProcessingLevel(Dictionary<string, string> hdfAttributes)
        {
            string[] possibleLevels = new string[] { "L0", "L1A", "L1B", "L1C", "L1D" };
            string processingLevel = null;
            foreach (string level in possibleLevels)
            {
                if (hdfAttributes.TryGetValue(string.Format("{0} Software Version", level), out string version) && version != null)
                {
                    processingLevel = level;
                }
            }
            return processingLevel;
        }


        private string GetTagValue(Schemas.Tag[] tags, string tagName)
        {
            foreach (Schemas.Tag tag in tags)
            {
                if (tag.TagName == tagName) return tag.TagValue;
            }
            return null;
        }



        protected void AddAssets(StacItem stacItem, IItem item, Dictionary<string, string> hdfAttributes, Schemas.Metadata metadata, Match identifierMatch)
        {
            IAsset imageAsset = GetDataAsset(item);

            stacItem.Assets.Add("image", StacAsset.CreateDataAsset(stacItem, imageAsset.Uri, new ContentType("application/x-hdf5"), "Image file"));
            stacItem.Assets["image"].Properties.AddRange(imageAsset.Properties);
            stacItem.Assets["image"].Properties["sar:polarizations"] = new string[] {
                identifierMatch.Groups["pol"].Value
            };

            IAsset metadataAsset = GetMetadataAsset(item);
            if (metadataAsset != null)
            {
                stacItem.Assets.Add("metadata", StacAsset.CreateMetadataAsset(stacItem, metadataAsset.Uri, new ContentType(MimeTypes.GetMimeType(metadataAsset.Uri.OriginalString)), "Metadata file"));
                stacItem.Assets["metadata"].Properties.AddRange(metadataAsset.Properties);
            }

            IAsset additionalMetadataAsset = FindFirstAssetFromFileNameRegex(item, "DFAS_.*_CSK_AccompanyingSheet\\.xml$");
            if (additionalMetadataAsset != null)
            {
                stacItem.Assets.Add("metadata-acc", StacAsset.CreateMetadataAsset(stacItem, additionalMetadataAsset.Uri, new ContentType(MimeTypes.GetMimeType(additionalMetadataAsset.Uri.OriginalString)), "Accompanying sheet"));
                stacItem.Assets["metadata-acc"].Properties.AddRange(additionalMetadataAsset.Properties);
            }
        }


        private string GetHdf5Value(string hdf5File, string attribute)
        {

            Process h5dumpProcess = new Process();
            ProcessStartInfo h5dumpStartInfo = new ProcessStartInfo();


            h5dumpStartInfo.FileName = "h5dump";
            h5dumpStartInfo.RedirectStandardError = true;
            h5dumpStartInfo.RedirectStandardOutput = true;
            h5dumpStartInfo.UseShellExecute = false;
            // ncDumpStartInfo.Arguments = @"-c 'ncdump -v lat_01,lon_01 " + ncFile + "'";
            h5dumpStartInfo.Arguments = string.Format("-N \"{1}\" \"{0}\"", hdf5File, attribute);
            h5dumpProcess.EnableRaisingEvents = true;
            h5dumpProcess.StartInfo = h5dumpStartInfo;

            string errorMessage = string.Empty;
            string value = null;

            h5dumpProcess.ErrorDataReceived += new DataReceivedEventHandler(
                delegate (object sender, DataReceivedEventArgs e)
                {
                    if (e.Data == null) return;

                    errorMessage += e.Data;
                }
            );

            h5dumpProcess.OutputDataReceived += new DataReceivedEventHandler(
                delegate (object sender, DataReceivedEventArgs e)
                {
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
            if (exitCode > 0 & !h5dumpProcess.HasExited)
            {
                h5dumpProcess.Kill();
            }

            if (value != null && value.StartsWith("\"") && value.EndsWith("\""))
            {
                value = value.Substring(1, value.Length - 2);
            }

            return value;
        }

    }

}
