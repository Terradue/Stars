// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: InpeMetadataExtractor.cs

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
using CsvHelper;
using Microsoft.Extensions.Logging;
using Stac;
using Stac.Extensions.Eo;
using Stac.Extensions.Processing;
using Stac.Extensions.Projection;
using Stac.Extensions.Raster;
using Stac.Extensions.Sat;
using Stac.Extensions.View;
using Terradue.Stars.Data.Model.Metadata.Inpe.Schemas;
using Terradue.Stars.Geometry.GeoJson;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Model.Stac;

namespace Terradue.Stars.Data.Model.Metadata.Inpe
{
    public class InpeMetadataExtractor : MetadataExtraction
    {
        private static readonly Regex identifierRegex = new Regex(@"(?'id1'(CBERS_4A?|AMAZONIA-1)_(?'type'[^_]+)_\d{8}_\d{3}_\d{3}_L(?'level'[^_]+))(_LEFT|RIGHT)?(?'id2'_BAND(?'band'\d+))");

        // alternative identifier regex for for filename of
        // this type 956-INPE-CBERS-4-urn_ogc_def_EOP_INPE_CBERS_4_AWFI_20220731_111_063_L4_B_compose
        private static readonly Regex identifierRegex2 = new Regex(@".*_inpe_(call[0-9]*|cbers_4a?|amazonia_1)_(?'type'[^_]+)_\d{8}_\d{3}_\d{3}_l(?'level'[^_]+)_(band|b)?(\d+)?(.+)?\.csv$");

        protected static string metadataAssetRegexPattern => @".*(CBERS_4|Call|CBERS_4A?|cbers_4|call|cbers_4A?|AMAZONIA_1|amazonia_1).*\.(xml|csv)$";
        protected static string compositeAssetRegexPattern => @".*(CBERS_4|Call|CBERS_4A?|cbers_4|call|cbers_4A?|AMAZONIA_1|amazonia_1).*\.(tif|tiff)$";

        private readonly Regex identifierInfoRegex = new Regex(@".*(?'mode'awfi|mux|pan|pan5m|pan10m|wfi|wpm)_\d{8}_\d{3}_\d{3}_l(?'level'[^_]+)_(?'rest'.*)$");

        // Dictionary containing platform international designators
        private readonly Dictionary<string, string> platformInternationalDesignators = new Dictionary<string, string> {
            {"CBERS-4", "2014-079A"},
            {"CBERS-4A", "2019-093E"},
            {"AMAZONIA-1", "2021-015A"},
        };

        // Dictionary containing the bands offered by each spectral mode
        private readonly Dictionary<string, int[]> spectralModeBandsCbers = new Dictionary<string, int[]> {
            {"AWFI", new int[] {13, 14, 15, 16}},
            {"MUX", new int[] {5, 6, 7, 8}},
            {"PAN5M", new int[] {1}},
            {"PAN10M", new int[] {2, 3, 4}},
            {"WFI", new int[] {13, 14, 15, 16}},
            {"WPM", new int[] {0, 1, 2, 3, 4}},
            {"WPM-pansharpening", new int[] {1, 2, 3, 4}},
        };

        private readonly Dictionary<string, int[]> spectralModeBandsAmazonia = new Dictionary<string, int[]> {
            {"WFI", new int[] {1, 2, 3, 4}},
        };

        private readonly Regex bandKeyRegex = new Regex(@"band-\d+");
        private readonly Regex utmZoneRegex = new Regex(@"(?'num'\d+)(?'hem'[NS])");

        public static XmlSerializer metadataSerializer = new XmlSerializer(typeof(Schemas.Metadata));

        public override string Label =>
            "China-Brazil Earth Resources Satellite-4/4A (INPE) mission product metadata extractor";

        public InpeMetadataExtractor(ILogger<InpeMetadataExtractor> logger,
            IResourceServiceProvider resourceServiceProvider) : base(logger, resourceServiceProvider)
        {
        }

        public override bool CanProcess(IResource route, IDestination destination)
        {
            if (!(route is IItem item)) return false;
            try
            {
                IAsset metadataAsset = GetMetadataAsset(item);
                Schemas.Metadata metadata = ReadMetadata(metadataAsset).GetAwaiter().GetResult();
                return metadata != null;

            }
            catch (Exception e)
            {
                //Console.WriteLine("CAN NOT PROCESS: {0}\n{1}", e.Message, e.StackTrace);
                return false;
            }
        }

        protected override async Task<StacNode> ExtractMetadata(IItem item, string suffix)
        {
            IAsset metadataAsset = GetMetadataAsset(item);
            Schemas.Metadata metadata = await ReadMetadata(metadataAsset);

            Dictionary<string, string> bands = new Dictionary<string, string>();
            FindBands(metadata.image, bands);
            if (metadata.leftCamera != null) FindBands(metadata.leftCamera.image, bands, "-left");
            if (metadata.rightCamera != null) FindBands(metadata.rightCamera.image, bands, "-right");

            StacItem stacItem = CreateStacItem(metadata, bands);

            IAsset compositeAsset = null;
            if (bands.Count == 0)
            {
                compositeAsset = GetCompositeAsset(item);
            }

            AddAssets(stacItem, item, metadata, bands, compositeAsset);

            AddEoStacExtension(metadata, stacItem);
            AddSatStacExtension(metadata, stacItem);
            AddProjStacExtension(metadata, stacItem);
            AddViewStacExtension(metadata, stacItem);
            AddProcessingStacExtension(metadata, stacItem);
            FillBasicsProperties(metadata, stacItem.Properties);
            AddOtherProperties(metadata, stacItem.Properties);

            return StacNode.Create(stacItem, item.Uri);
            ;
        }

        internal virtual StacItem CreateStacItem(Schemas.Metadata metadata, Dictionary<string, string> bands)
        {
            string identifier = null;


            if (metadata.identifier != null)
            {
                // cbers products with csv metadata file have the identifier attribute in it
                identifier = metadata.identifier;
            }
            else if (bands.Count == 0)
            {
                throw new InvalidOperationException("No band information found");
            }
            else
            {
                bool single = true; // single has to be true if single band or left/right pair of same band
                if (bands.Count == 1) single = true;
                else
                {
                    string b = null;
                    foreach (string key in bands.Keys)
                    {
                        Match bandKeyMatch = bandKeyRegex.Match(key);
                        if (b == null)
                        {
                            b = bandKeyMatch.Value;
                        }
                        else if (bandKeyMatch.Value != b)
                        {
                            single = false;
                            break;
                        }
                    }
                }

                if (single)
                {
                    string bandName = bands.First().Value;
                    Match identifierMatch = identifierRegex.Match(bandName);
                    if (!identifierMatch.Success)
                    {
                        throw new InvalidOperationException(
                            string.Format("Identifier not recognised from band name: {0}", bandName));
                    }

                    identifier = string.Format("{0}{1}", identifierMatch.Groups["id1"].Value,
                        identifierMatch.Groups["id2"].Value);
                }
                else
                {
                    foreach (string key in bands.Keys)
                    {
                        string bandName = bands[key];
                        Match identifierMatch = identifierRegex.Match(bandName);
                        if (!identifierMatch.Success)
                        {
                            throw new InvalidOperationException(
                                string.Format("Identifier not recognised from band name: {0}", bandName));
                        }

                        identifier = identifierMatch.Groups["id1"].Value;
                        break;
                    }
                }
            }

            StacItem stacItem = new StacItem(identifier, GetGeometry(metadata), GetCommonMetadata(metadata));
            return stacItem;
        }

        private void FindBands(prdfImage image, Dictionary<string, string> bands, string suffix = "")
        {
            if (image == null) return;

            if (image.band0 != null) bands[string.Format("band-0{0}", suffix)] = image.band0;
            if (image.band1 != null) bands[string.Format("band-1{0}", suffix)] = image.band1;
            if (image.band2 != null) bands[string.Format("band-2{0}", suffix)] = image.band2;
            if (image.band3 != null) bands[string.Format("band-3{0}", suffix)] = image.band3;
            if (image.band4 != null) bands[string.Format("band-4{0}", suffix)] = image.band4;
            if (image.band5 != null) bands[string.Format("band-5{0}", suffix)] = image.band5;
            if (image.band6 != null) bands[string.Format("band-6{0}", suffix)] = image.band6;
            if (image.band7 != null) bands[string.Format("band-7{0}", suffix)] = image.band7;
            if (image.band8 != null) bands[string.Format("band-8{0}", suffix)] = image.band8;
            if (image.band13 != null) bands[string.Format("band-13{0}", suffix)] = image.band13;
            if (image.band14 != null) bands[string.Format("band-14{0}", suffix)] = image.band14;
            if (image.band15 != null) bands[string.Format("band-15{0}", suffix)] = image.band15;
            if (image.band16 != null) bands[string.Format("band-16{0}", suffix)] = image.band16;
        }


        protected virtual IAsset GetCompositeAsset(IItem item)
        {
            IAsset compositeAsset = FindFirstAssetFromFileNameRegex(item, compositeAssetRegexPattern);
            return compositeAsset;
        }


        private GeoJSON.Net.Geometry.IGeometryObject GetGeometry(Schemas.Metadata metadata)
        {
            GeoJSON.Net.Geometry.LineString lineString = new GeoJSON.Net.Geometry.LineString(
                new GeoJSON.Net.Geometry.Position[]
                {
                    new GeoJSON.Net.Geometry.Position(double.Parse(metadata.image.boundingBox.LL.latitude),
                        double.Parse(metadata.image.boundingBox.LL.longitude)),
                    new GeoJSON.Net.Geometry.Position(double.Parse(metadata.image.boundingBox.LR.latitude),
                        double.Parse(metadata.image.boundingBox.LR.longitude)),
                    new GeoJSON.Net.Geometry.Position(double.Parse(metadata.image.boundingBox.UR.latitude),
                        double.Parse(metadata.image.boundingBox.UR.longitude)),
                    new GeoJSON.Net.Geometry.Position(double.Parse(metadata.image.boundingBox.UL.latitude),
                        double.Parse(metadata.image.boundingBox.UL.longitude)),
                    new GeoJSON.Net.Geometry.Position(double.Parse(metadata.image.boundingBox.LL.latitude),
                        double.Parse(metadata.image.boundingBox.LL.longitude)),
                }
            );
            return new GeoJSON.Net.Geometry.Polygon(new GeoJSON.Net.Geometry.LineString[] { lineString })
                .NormalizePolygon();
        }


        protected virtual IAsset GetMetadataAsset(IItem item)
        {
            IAsset metadataAsset = FindFirstAssetFromFileNameRegex(item, metadataAssetRegexPattern) ?? throw new FileNotFoundException(string.Format("Unable to find the metadata file asset"));
            return metadataAsset;
        }

        public virtual async Task<Schemas.Metadata> ReadMetadata(IAsset metadataAsset)
        {
            string originalAssetName = Path.GetFileName(metadataAsset.Uri.OriginalString);
            Match identifierMatch = identifierRegex.Match(originalAssetName);
            Match identifierMatch2 = identifierRegex2.Match(originalAssetName.ToLower());

            string typeStr;
            string level;
            if (identifierMatch.Success)
            {
                typeStr = identifierMatch.Groups["type"].Value.ToUpper();
                level = identifierMatch.Groups["level"].Value;
            }
            else if (identifierMatch2.Success)
            {
                typeStr = identifierMatch2.Groups["type"].Value.ToUpper();
                level = identifierMatch2.Groups["level"].Value;
            }
            else
            {
                throw new Exception("No metadata file found");
            }

            if (typeStr == "PAN")
            {
                if (originalAssetName.ToLower().Contains("_compose")) typeStr = "PAN10M";
                else typeStr = "PAN5M";
            }
            switch (typeStr)
            {
                case "AWFI":
                case "MUX":
                case "PAN5M":
                case "PAN10M":
                case "WFI":
                case "WPM":
                    break;
                default:
                    throw new InvalidOperationException(string.Format("Unknown metadata/band type: {0}", typeStr));
            }

            logger.LogDebug("Opening metadata file {0}", metadataAsset.Uri);

            Schemas.Metadata metadata = null;

            if (metadataAsset.Uri.AbsolutePath.EndsWith(".csv"))
            {
                using (var stream =
                       await resourceServiceProvider.GetAssetStreamAsync(metadataAsset,
                           System.Threading.CancellationToken.None))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                        {
                            var metadataCsv = csv.GetRecords<MetadataCsv>().First();
                            metadata = metadataCsv.getMetadata();
                        }
                    }
                }
                (metadata.leftCamera == null ? metadata.image : metadata.leftCamera.image).level = level;
            }
            else
            {
                using (var stream =
                       await resourceServiceProvider.GetAssetStreamAsync(metadataAsset,
                           System.Threading.CancellationToken.None))
                {
                    var reader = XmlReader.Create(stream);
                    logger.LogDebug("Deserializing metadata file {0}", metadataAsset.Uri);

                    metadata = (Schemas.Metadata)metadataSerializer.Deserialize(reader);
                    if (metadata.leftCamera != null)
                    {
                        metadata.image = metadata.leftCamera.image;
                        metadata.satellite = metadata.leftCamera.satellite;
                    }
                }
            }

            metadata.spectralMode = typeStr;
            return metadata;
        }


        private string GetProcessingLevel(Schemas.Metadata metadata)
        {
            return string.Format("L{0}", metadata.image.level);
        }

        private IDictionary<string, object> GetCommonMetadata(Schemas.Metadata metadata)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            FillDateTimeProperties(metadata, properties);
            FillInstrument(metadata, properties);
            FillBasicsProperties(metadata, properties);

            return properties;
        }

        private void FillDateTimeProperties(Schemas.Metadata metadata, Dictionary<string, object> properties)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            DateTime startDate = DateTime.MinValue;
            bool hasStartDate =
                DateTime.TryParse(metadata.image.timeStamp.begin, null, DateTimeStyles.AssumeUniversal, out startDate);
            DateTime endDate = startDate;
            bool hasEndDate = DateTime.TryParse(metadata.image.timeStamp.end, null, DateTimeStyles.AssumeUniversal, out endDate);
            DateTime centerDate = startDate;
            bool hasCenterDate =
                DateTime.TryParse(metadata.image.timeStamp.center, null, DateTimeStyles.AssumeUniversal, out centerDate);

            if (hasStartDate && hasEndDate)
            {
                properties["start_datetime"] = startDate.ToUniversalTime();
                properties["end_datetime"] = endDate.ToUniversalTime();
                properties["datetime"] = centerDate.ToUniversalTime();
            }
            else if (hasStartDate)
            {
                properties["datetime"] = startDate.ToUniversalTime();
            }

            DateTime createdDate = DateTime.MinValue;

            bool hasCreatedDate =
                DateTime.TryParse(metadata.image.processingTime, null, DateTimeStyles.AssumeUniversal, out createdDate);

            if (hasCreatedDate)
            {
                properties["created"] = createdDate.ToUniversalTime();
            }

            properties["updated"] = DateTime.UtcNow;
        }


        private void FillInstrument(Schemas.Metadata metadata, Dictionary<string, object> properties)
        {
            // platform & constellation
            properties["constellation"] = string.Format("{0}-{1}", metadata.satellite.name, metadata.satellite.number).ToLower();
            properties["platform"] = string.Format("{0}-{1}", metadata.satellite.name, metadata.satellite.number).ToLower();
            properties["mission"] = properties["platform"];
            properties["instruments"] = new string[] { metadata.satellite.instrument.Value.ToLower() };
            properties["sensor_type"] = "optical";
            if (double.TryParse(metadata.image.verticalPixelSize, out double gsd))
            {
                properties["gsd"] = gsd;
            }
            else
            {
                switch (metadata.spectralMode)
                {
                    case "PAN5M":
                        gsd = 5;
                        break;
                    case "PAN10M":
                        gsd = 10;
                        break;
                    case "MUX":
                        if ("4" == "4") gsd = 16.5;
                        else if ("4a" == "4a") gsd = 20;
                        break;
                    case "AWFI":
                        gsd = 64;
                        break;
                    case "WPM":
                        gsd = 2;
                        break;
                    case "WFI":
                        if (metadata.satellite.name == "CBERS") gsd = 55;
                        else if (metadata.satellite.name == "AMAZONIA") gsd = 64;
                        break;
                }
                if (gsd != 0)
                {
                    properties["gsd"] = gsd;
                }

            }
        }

        private void FillBasicsProperties(Schemas.Metadata metadata, IDictionary<string, object> properties)
        {
            CultureInfo culture = new CultureInfo("fr-FR");
            properties["title"] = string.Format("{0} {1} {2} {3}",
                string.Format("{0}-{1}", metadata.satellite.name.ToUpper(), metadata.satellite.number.ToUpper()),
                metadata.spectralMode,
                GetProcessingLevel(metadata),
                properties.GetProperty<DateTime>("datetime").ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss", culture)
            );
        }

        private void AddOtherProperties(Schemas.Metadata metadata, IDictionary<string, object> properties)
        {
            if (IncludeProviderProperty)
            {
                string name = "INPE";
                string description = null;
                string url = "http://www.inpe.br";
                if (metadata.satellite.name == "CBERS")
                {
                    name = "INPE/CAST";
                    description = "The China-Brazil Earth Resources Satellite mission is to provide remote sensing images to observe and monitor vegetation - especially deforestation in the Amazon region - the monitoring of water resources, agriculture, urban growth, land use and education.";
                    url = "http://www.cbers.inpe.br/sobre/cbers3-4.php";
                }
                else if (metadata.satellite.name == "AMAZONIA")
                {
                    description = "Amazonia-1 is an Earth observation minisatellite mission of the Brazilian Space Agency (AEB), developed at the National Institute for Space Research (INPE) in Brazil. Its main goal is to monitor global deforestation, with a focus on the Brazilian Amazon rainforest.";
                    url = "http://www.inpe.br/amazonia1/";
                }
                AddSingleProvider(
                    properties,
                    name,
                    description,
                    new StacProviderRole[]
                        { StacProviderRole.producer, StacProviderRole.processor, StacProviderRole.licensor },
                    new Uri(url)
                );
            }
            properties["licence"] = "proprietary";
        }


        private void AddEoStacExtension(Schemas.Metadata metadata, StacItem stacItem)
        {
            var eo = stacItem.EoExtension();
            eo.Bands = stacItem.Assets.Values.Where(a => a.EoExtension().Bands != null)
                .SelectMany(a => a.EoExtension().Bands).ToArray();
        }


        private void AddSatStacExtension(Schemas.Metadata metadata, StacItem stacItem)
        {
            var sat = new SatStacExtension(stacItem);

            // only add if we have a valid orbit direction
            if (metadata.image.orbitDirection != null)
            {
                sat.OrbitState = metadata.image.orbitDirection.ToLower();
            }

            if (long.TryParse(metadata.image.path, out long path) && long.TryParse(metadata.image.row, out long row))
            {
                stacItem.Properties["cbers:path"] = path;
                stacItem.Properties["cbers:row"] = row;
                //sat.AbsoluteOrbit = Convert.ToInt32(1000 * path + row);
            }
            // sat.RelativeOrbit = 

            string platformFullName = string.Format("{0}-{1}", metadata.satellite.name, metadata.satellite.number);
            if (platformInternationalDesignators.ContainsKey(platformFullName))
            {
                sat.PlatformInternationalDesignator = platformInternationalDesignators[platformFullName];
            }
        }


        private void AddProjStacExtension(Schemas.Metadata metadata, StacItem stacItem)
        {
            if (metadata.image != null && !string.IsNullOrEmpty(metadata.image.epsg) && long.TryParse(metadata.image.epsg, out long epsg))
            {
                ProjectionStacExtension proj = stacItem.ProjectionExtension();
                proj.Epsg = epsg;
            }

            /*
            if (metadata.mapProjection != "UTM" || metadata.Zone_Number == null) return;
            Match utmZoneMatch = utmZoneRegex.Match(metadata.image.projectionName);
            Console.WriteLine("ZONE: {0} {1}", metadata.Zone_Number, utmZoneMatch.Success);
            if (!utmZoneMatch.Success) return;


            ProjectionStacExtension proj = stacItem.ProjectionExtension();
            //proj.Wkt2 = ProjNet.CoordinateSystems.GeocentricCoordinateSystem.WGS84.WKT;

            int zone = Int32.Parse(utmZoneMatch.Groups["num"].Value);
            bool north = utmZoneMatch.Groups["hem"].Value == "N";
            ProjectedCoordinateSystem utm = ProjectedCoordinateSystem.WGS84_UTM(zone, north);
            proj.SetCoordinateSystem(utm);
            */
        }


        private void AddViewStacExtension(Schemas.Metadata metadata, StacItem stacItem)
        {
            var view = new ViewStacExtension(stacItem);
            if (double.TryParse(metadata.image.offNadirAngle, out double offNadir))
            {
                view.OffNadir = offNadir / 1000;
            }

            if (double.TryParse(metadata.image.sunPosition.sunAzimuth, out double sunAzimuth))
            {
                view.SunAzimuth = sunAzimuth;
            }

            if (double.TryParse(metadata.image.sunPosition.elevation, out double sunElevation))
            {
                view.SunElevation = sunElevation;
            }
        }


        private void AddProcessingStacExtension(Schemas.Metadata metadata, StacItem stacItem)
        {
            var proc = stacItem.ProcessingExtension();
            proc.Level = GetProcessingLevel(metadata);
        }


        protected void AddAssets(StacItem stacItem, IItem item, Schemas.Metadata metadata,
            Dictionary<string, string> bands, IAsset compositeAsset = null)
        {
            if (compositeAsset == null)
            {
                foreach (string key in bands.Keys)
                {
                    string bandFile = Path.GetFileName(bands[key]);

                    IAsset bandAsset = FindFirstAssetFromFileNameRegex(item, string.Format("{0}$", bandFile)) ?? throw new FileNotFoundException(string.Format(
                            "Band file declared in metadata, but not present '{0}'",
                            bandFile)); //.Replace(".", @"\.")

                    AddBandAsset(stacItem, key, bandAsset, metadata, null, null);
                }
            }
            else
            {
                Match match = identifierInfoRegex.Match(metadata.identifier.ToLower());
                int[] defaultCompositeBands = null;
                int[] compositeBands = null;

                if (match.Success)
                {
                    string mode = match.Groups["mode"].Value.ToUpper();
                    string rest = match.Groups["rest"].Value;
                    if (mode == "PAN")
                    {
                        if (rest.Contains("compose")) mode = "PAN10M";
                        else mode = "PAN5M";
                    }

                    if (metadata.satellite.name == "CBERS")
                    {
                        if (mode == "WPM" && rest.Contains("pansharpening"))
                        {
                            defaultCompositeBands = spectralModeBandsCbers["WPM-pansharpening"];
                        }
                        else
                        {
                            defaultCompositeBands = spectralModeBandsCbers[mode];
                        }
                    }
                    else if (metadata.satellite.name == "AMAZONIA")
                    {
                        defaultCompositeBands = spectralModeBandsAmazonia[mode];
                    }

                    if (rest.Contains("compose"))
                    {
                        compositeBands = defaultCompositeBands;
                    }
                    else
                    {
                        // Identifier contains band names
                        string[] bandsStrs = new string[defaultCompositeBands.Length];
                        for (int i = 0; i < bandsStrs.Length; i++) bandsStrs[i] = defaultCompositeBands[i].ToString();
                        Regex bandsRegex = new Regex(string.Format(@"(band|b)(?'bands'{0})+", string.Join("|", bandsStrs)));
                        Match bandMatch = bandsRegex.Match(rest);
                        compositeBands = new int[bandMatch.Groups["bands"].Captures.Count];
                        for (int i = 0; i < bandMatch.Groups["bands"].Captures.Count; i++)
                        {
                            compositeBands[i] = int.Parse(bandMatch.Groups["bands"].Captures[i].Value);
                        }
                    }

                    string s = "";
                    foreach (int b in compositeBands) s += string.Format(" {0}", b);
                }
                if (compositeBands == null || compositeBands.Length == 0)
                {
                    throw new Exception(string.Format("Contained bands not detectable from identifier \"{0}\"", metadata.identifier));
                }

                StacAsset stacAsset = StacAsset.CreateDataAsset(stacItem, compositeAsset.Uri,
                    new ContentType(MimeTypes.GetMimeType(compositeAsset.Uri.OriginalString)), string.Format("{0} {1} COMPOSE", metadata.spectralMode, GetProcessingLevel(metadata))
                );
                stacAsset.Roles.Add("dn");
                stacAsset.Properties.AddRange(compositeAsset.Properties);
                stacItem.Assets.Add("compose", stacAsset);
                foreach (int band in compositeBands)
                {
                    AddBandAsset(stacItem, band, compositeAsset, metadata, stacAsset, defaultCompositeBands);
                }
            }

            IAsset metadataAsset = GetMetadataAsset(item);
            stacItem.Assets.Add("metadata",
                StacAsset.CreateMetadataAsset(stacItem, metadataAsset.Uri,
                    new ContentType(MimeTypes.GetMimeType(metadataAsset.Uri.OriginalString)), "Metadata file"));
            stacItem.Assets["metadata"].Properties.AddRange(metadataAsset.Properties);

            IAsset additionalMetadataAsset = FindFirstAssetFromFileNameRegex(item, @"VRSS-[12]_.*ADDITION\.xml$");
            if (additionalMetadataAsset != null)
            {
                stacItem.Assets.Add("metadata-addition",
                    StacAsset.CreateMetadataAsset(stacItem, additionalMetadataAsset.Uri,
                        new ContentType(MimeTypes.GetMimeType(additionalMetadataAsset.Uri.OriginalString)),
                        "Additional metadata"));
                stacItem.Assets["metadata-addition"].Properties.AddRange(additionalMetadataAsset.Properties);
            }

            IAsset overviewAsset = FindFirstAssetFromFileNameRegex(item, @".*\.png");
            if (overviewAsset != null)
            {
                stacItem.Assets.Add("overview", StacAsset.CreateOverviewAsset(stacItem, overviewAsset.Uri,
                            new ContentType(MimeTypes.GetMimeType(overviewAsset.Uri.ToString()))));
                stacItem.Assets["overview"].Properties.AddRange(overviewAsset.Properties);
            }

            IAsset thumbnailAsset = FindFirstAssetFromFileNameRegex(item, @".*\.jpg");
            if (thumbnailAsset != null)
            {
                stacItem.Assets.Add("thumbnail", StacAsset.CreateThumbnailAsset(stacItem, thumbnailAsset.Uri,
                            new ContentType(MimeTypes.GetMimeType(thumbnailAsset.Uri.ToString()))));
                stacItem.Assets["thumbnail"].Properties.AddRange(thumbnailAsset.Properties);
            }
        }

        private void AddBandAsset(StacItem stacItem, int bandNumber, IAsset imageAsset, Schemas.Metadata metadata, StacAsset stacAsset = null, int[] defaultCompositeBands = null)
        {
            AddBandAsset(stacItem, string.Format("band-{0}", bandNumber), imageAsset, metadata, stacAsset, defaultCompositeBands);
        }

        private void AddBandAsset(StacItem stacItem, string bandId, IAsset imageAsset, Schemas.Metadata metadata, StacAsset stacAsset = null, int[] defaultCompositeBands = null)
        {
            double? waveLength = null;
            double? fullWidthHalfMax = null;
            double? solarIllumination = null;
            Stac.Common.DataType? dataType = null;
            int? bitsPerSample = null;
            double? scale = null;

            EoBandCommonName commonName = new EoBandCommonName();
            bool notFound = false;

            if (metadata.satellite.instrument.Value == "WPM")
            {
                switch (bandId)
                {
                    case "band-0": // WPM
                        waveLength = 0.675;
                        fullWidthHalfMax = 0.45;
                        solarIllumination = 1258.38;
                        commonName = EoBandCommonName.pan;
                        break;
                    case "band-1": // WPM
                        waveLength = 0.485;
                        fullWidthHalfMax = 0.07;
                        solarIllumination = 1984.65;
                        dataType = Stac.Common.DataType.int16;
                        bitsPerSample = 12;
                        commonName = EoBandCommonName.blue;
                        break;
                    case "band-2": // WPM
                        waveLength = 0.555;
                        fullWidthHalfMax = 0.07;
                        solarIllumination = 1823.40;
                        dataType = Stac.Common.DataType.int16;
                        bitsPerSample = 12;
                        commonName = EoBandCommonName.green;
                        break;
                    case "band-3": // WPM
                        waveLength = 0.660;
                        fullWidthHalfMax = 0.06;
                        solarIllumination = 1536.38;
                        dataType = Stac.Common.DataType.int16;
                        bitsPerSample = 12;
                        commonName = EoBandCommonName.red;
                        break;
                    case "band-4": // WPM
                        waveLength = 0.830;
                        fullWidthHalfMax = 0.12;
                        solarIllumination = 981.91;
                        dataType = Stac.Common.DataType.int16;
                        bitsPerSample = 12;
                        commonName = EoBandCommonName.nir;
                        break;
                }
            }
            else if (metadata.satellite.name == "AMAZONIA" && metadata.satellite.instrument.Value == "WFI")
            {
                switch (bandId)
                {
                    case "band-1": // WPM
                        waveLength = 0.485;
                        fullWidthHalfMax = 0.07;
                        solarIllumination = 1984.65;
                        dataType = Stac.Common.DataType.int16;
                        bitsPerSample = 12;
                        commonName = EoBandCommonName.blue;
                        break;
                    case "band-2": // WPM
                        waveLength = 0.555;
                        fullWidthHalfMax = 0.07;
                        solarIllumination = 1823.40;
                        dataType = Stac.Common.DataType.int16;
                        bitsPerSample = 12;
                        commonName = EoBandCommonName.green;
                        break;
                    case "band-3": // WPM
                        waveLength = 0.660;
                        fullWidthHalfMax = 0.06;
                        solarIllumination = 1536.38;
                        dataType = Stac.Common.DataType.int16;
                        bitsPerSample = 12;
                        commonName = EoBandCommonName.red;
                        break;
                    case "band-4": // WPM
                        waveLength = 0.830;
                        fullWidthHalfMax = 0.12;
                        solarIllumination = 981.91;
                        dataType = Stac.Common.DataType.int16;
                        bitsPerSample = 12;
                        commonName = EoBandCommonName.nir;
                        break;
                }
            }
            else
            {
                switch (bandId)
                {
                    case "band-1": // PAN5M
                        waveLength = 0.700;
                        fullWidthHalfMax = 0.38;
                        solarIllumination = 1259.85;
                        commonName = EoBandCommonName.pan;
                        break;
                    case "band-2": // PAN10M
                        waveLength = 0.555;
                        fullWidthHalfMax = 0.07;
                        solarIllumination = 1823.40;
                        commonName = EoBandCommonName.green;
                        break;
                    case "band-3": // PAN10M
                        waveLength = 0.660;
                        fullWidthHalfMax = 0.06;
                        solarIllumination = 1536.38;
                        commonName = EoBandCommonName.red;
                        break;
                    case "band-4": // PAN10M
                        waveLength = 0.830;
                        fullWidthHalfMax = 0.12;
                        solarIllumination = 981.91;
                        commonName = EoBandCommonName.nir;
                        break;
                    case "band-5": // MUX
                        waveLength = 0.485;
                        fullWidthHalfMax = 0.07;
                        solarIllumination = 1984.65;
                        commonName = EoBandCommonName.blue;
                        break;
                    case "band-6": // MUX
                        waveLength = 0.555;
                        fullWidthHalfMax = 0.07;
                        solarIllumination = 1823.40;
                        commonName = EoBandCommonName.green;
                        break;
                    case "band-7": // MUX
                        waveLength = 0.660;
                        fullWidthHalfMax = 0.06;
                        solarIllumination = 1536.38;
                        commonName = EoBandCommonName.red;
                        break;
                    case "band-8": // MUX
                        waveLength = 0.830;
                        fullWidthHalfMax = 0.12;
                        solarIllumination = 981.91;
                        commonName = EoBandCommonName.nir;
                        break;
                    case "band-13": // AWFI
                    case "band-13-left": // WFI
                    case "band-13-right": // WFI
                        waveLength = 0.485;
                        fullWidthHalfMax = 0.07;
                        solarIllumination = 1984.65;
                        dataType = Stac.Common.DataType.int16;
                        bitsPerSample = 12;
                        commonName = EoBandCommonName.blue;
                        break;
                    case "band-14": // AWFI
                    case "band-14-left": // WFI
                    case "band-14-right": // WFI
                        waveLength = 0.555;
                        fullWidthHalfMax = 0.07;
                        solarIllumination = 1823.40;
                        dataType = Stac.Common.DataType.int16;
                        bitsPerSample = 12;
                        commonName = EoBandCommonName.green;
                        break;
                    case "band-15": // AWFI
                    case "band-15-left": // WFI
                    case "band-15-right": // WFI
                        waveLength = 0.66;
                        fullWidthHalfMax = 0.06;
                        solarIllumination = 1536.38;
                        dataType = Stac.Common.DataType.int16;
                        bitsPerSample = 12;
                        commonName = EoBandCommonName.red;
                        break;
                    case "band-16": // AWFI
                    case "band-16-left": // WFI
                    case "band-16-right": // WFI
                        waveLength = 0.83;
                        fullWidthHalfMax = 0.12;
                        solarIllumination = 981.91;
                        dataType = Stac.Common.DataType.int16;
                        bitsPerSample = 12;
                        commonName = EoBandCommonName.nir;
                        break;
                    default:
                        notFound = true;
                        break;
                }
            }

            if (notFound)
            {
                throw new InvalidOperationException(string.Format("Band information not found for {0}", bandId));
            }

            // Find absolute calibration coefficients
            scale = GetAbsoluteCalibrationCoefficient(bandId, metadata.image?.absoluteCalibrationCoefficient, defaultCompositeBands);

            EoBandObject eoBandObject = new EoBandObject(bandId, commonName)
            {
                CenterWavelength = waveLength,
                FullWidthHalfMax = fullWidthHalfMax,
                SolarIllumination = solarIllumination
            };

            RasterBand rasterBand = null;
            // data type and bits per sample can't be reliably set (defaults from above may not always apply).
            rasterBand = new RasterBand()
            {
                //DataType = dataType,
                //BitsPerSample = bitsPerSample,
                Scale = scale,
                Offset = 0
            };

            if (stacAsset == null)
            {
                stacAsset = StacAsset.CreateDataAsset(stacItem, imageAsset.Uri,
                    new ContentType(MimeTypes.GetMimeType(imageAsset.Uri.OriginalString)), string.Format("{0} {1} {2}", metadata.spectralMode, GetProcessingLevel(metadata), bandId.ToUpper()));
                stacAsset.Roles.Add("dn");
                stacAsset.Properties.AddRange(imageAsset.Properties);
                stacAsset.EoExtension().Bands = new EoBandObject[] { eoBandObject };

                if (rasterBand != null)
                {
                    stacAsset.RasterExtension().Bands = new RasterBand[] { rasterBand };
                }

                stacItem.Assets.Add(bandId, stacAsset);
            }
            else
            {
                EoStacExtension eo = stacAsset.EoExtension();
                if (eo.Bands == null)
                {
                    eo.Bands = new EoBandObject[] { eoBandObject };
                }
                else
                {
                    List<EoBandObject> bands = new List<EoBandObject>(eo.Bands)
                    {
                        eoBandObject
                    };
                    eo.Bands = bands.ToArray();
                }

                if (rasterBand != null)
                {
                    RasterStacExtension raster = stacAsset.RasterExtension();
                    if (raster.Bands == null)
                    {
                        raster.Bands = new RasterBand[] { rasterBand };
                    }
                    else
                    {
                        List<RasterBand> bands = new List<RasterBand>(raster.Bands)
                        {
                            rasterBand
                        };
                        raster.Bands = bands.ToArray();
                    }
                }
            }
        }

        private double? GetAbsoluteCalibrationCoefficient(string bandId, band[] coefficients, int[] defaultCompositeBands)
        {
            string bandIdNumber = bandId.Replace("band-", string.Empty);
            if (coefficients != null)
            {
                if (defaultCompositeBands == null)
                {
                    // In case of properly provided metadata with asset path (via XML), get coefficient from band with same ID
                    foreach (band coefficient in coefficients)
                    {
                        if (coefficient.name.Replace("band-", string.Empty) == bandIdNumber)
                        {
                            if (double.TryParse(coefficient.Value, out double scale)) return scale;
                        }
                    }
                }
                else
                {
                    // In case manually created package (with CSV-provided metadata),
                    // get coefficient from the values originating from CSV assuming ascending order of bands
                    // from terms like this:
                    // band0:0.235 band1:0.267 band2:0.218 band3:0.189
                    // (band0 stands for the band with the lowest number, normally the first in the XML),
                    // e.g. BAND13 (blue) in case of AWFI, regardless of band order in actual image
                    for (int i = 0; i < defaultCompositeBands.Length; i++)
                    {
                        if (defaultCompositeBands[i].ToString() == bandIdNumber && i < coefficients.Length)
                        {
                            if (double.TryParse(coefficients[i].Value, out double scale)) return scale;
                        }
                    }
                }
            }
            return null;
        }
    }

}
