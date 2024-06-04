// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: Alos2MetadataExtractor.cs

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Mime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ProjNet.CoordinateSystems;
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

namespace Terradue.Stars.Data.Model.Metadata.Alos2
{
    public class Alos2MetadataExtractor : MetadataExtraction
    {

        public override string Label => "Advanced Land Observing Satellite-2 (JAXA) mission product metadata extractor";

        public Alos2MetadataExtractor(ILogger<Alos2MetadataExtractor> logger, IResourceServiceProvider resourceServiceProvider) : base(logger, resourceServiceProvider)
        {
        }

        public override bool CanProcess(IResource route, IDestination destination)
        {
            if (!(route is IItem item)) return false;
            try
            {
                IAsset metadataAsset = GetMetadataAsset(item);
                Alos2Metadata metadata = ReadMetadata(metadataAsset).GetAwaiter().GetResult();
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
            Alos2Metadata metadata = await ReadMetadata(metadataAsset);

            StacItem stacItem = CreateStacItem(metadata);

            AddAssets(stacItem, item, metadata);

            return StacNode.Create(stacItem, item.Uri); ;
        }

        internal virtual StacItem CreateStacItem(Alos2Metadata metadata)
        {
            string identifier = metadata.GetString("Ext_Identifier");
            StacItem stacItem = new StacItem(identifier, GetGeometry(metadata), GetCommonMetadata(metadata));
            AddSatStacExtension(metadata, stacItem);
            AddSarStacExtension(metadata, stacItem);
            AddProjStacExtension(metadata, stacItem);
            AddViewStacExtension(metadata, stacItem);
            AddProcessingStacExtension(metadata, stacItem);
            AddOtherProperties(metadata, stacItem);
            //FillBasicsProperties(metadata, stacItem.Properties);
            return stacItem;
        }

        private void AddSatStacExtension(Alos2Metadata metadata, StacItem stacItem)
        {
            var sat = new SatStacExtension(stacItem);
            sat.AbsoluteOrbit = metadata.GetInt32("Ext_OrbitNumber");
            sat.RelativeOrbit = sat.AbsoluteOrbit;   // TODO remove?
            sat.OrbitState = metadata.GetString("Ext_OrbitDirection").ToLower();
        }

        private void AddSarStacExtension(Alos2Metadata metadata, StacItem stacItem)
        {
            SarStacExtension sar = stacItem.SarExtension();
            sar.Required(metadata.GetString("Ext_ObservationMode"),
                SarCommonFrequencyBandName.L,
                metadata.GetString("Ext_Polarizations").Split('/'),
                metadata.GetString("Ext_ObservationMode")
            );

            sar.ObservationDirection = ParseObservationDirection(metadata.GetString("Ext_ObservationDirection"));
        }

        private void AddProjStacExtension(Alos2Metadata metadata, StacItem stacItem)
        {
            ProjectionStacExtension proj = stacItem.ProjectionExtension();
            try
            {
                int utmZone = metadata.GetInt32("Pds_UTM_ZoneNo");
                bool north = metadata.GetString("Pds_MapDirection").Contains("MapNorth");
                ProjectedCoordinateSystem utm = ProjectedCoordinateSystem.WGS84_UTM(utmZone, north);
                proj.SetCoordinateSystem(utm);
            }
            catch { }
            try
            {
                stacItem.ProjectionExtension().Shape = new int[2] { metadata.GetInt32("Pdi_NoOfPixels_0"), metadata.GetInt32("Pdi_NoOfLines_0") };
            }
            catch { }
        }

        private void AddViewStacExtension(Alos2Metadata metadata, StacItem stacItem)
        {
            var view = new ViewStacExtension(stacItem);
            view.OffNadir = metadata.GetDouble("Img_OffNadirAngle");
        }

        private void AddProcessingStacExtension(Alos2Metadata metadata, StacItem stacItem)
        {
            var proc = stacItem.ProcessingExtension();
            proc.Level = GetProcessingLevel(metadata);
        }

        private string GetProcessingLevel(Alos2Metadata metadata)
        {
            return string.Format("L{0}", metadata.GetString("Lbi_ProcessLevel"));
        }

        private IDictionary<string, object> GetCommonMetadata(Alos2Metadata metadata)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();

            FillDateTimeProperties(metadata, properties);
            // TODO Licensing
            // TODO Provider
            FillInstrument(metadata, properties);
            FillBasicsProperties(metadata, properties);

            return properties;
        }

        private void FillDateTimeProperties(Alos2Metadata metadata, Dictionary<string, object> properties)
        {
            DateTime startDate = metadata.GetDateTime("Img_SceneStartDateTime", new int[] { 0, 4, 6, 9, 12, 15, 18 });
            DateTime endDate = metadata.GetDateTime("Img_SceneEndDateTime", new int[] { 0, 4, 6, 9, 12, 15, 18 });
            DateTime centerDate = metadata.GetDateTime("Img_SceneCenterDateTime", new int[] { 0, 4, 6, 9, 12, 15, 18 });

            properties["datetime"] = centerDate.ToUniversalTime();
            properties["start_datetime"] = startDate.ToUniversalTime();
            properties["end_datetime"] = endDate.ToUniversalTime();

            DateTime createdDate = metadata.GetDateTime("Ext_ProcessedTime", new int[] { 0, 4, 6, 9, 11, 13 });

            if (createdDate.Ticks != 0)
            {
                properties["created"] = createdDate.ToUniversalTime();
            }

            properties["updated"] = DateTime.UtcNow;
        }

        private void FillInstrument(Alos2Metadata metadata, Dictionary<string, object> properties)
        {
            // platform & constellation
            properties.Remove("platform");
            properties.Add("platform", "alos-2");

            properties.Remove("constellation");
            properties.Add("constellation", "alos-2");

            properties.Remove("mission");
            properties.Add("mission", "alos-2");

            // instruments
            properties.Remove("instruments");
            properties.Add("instruments", new string[] { "palsar-2" });

            properties.Remove("sensor_type");
            properties.Add("sensor_type", "radar");

        }

        private void FillBasicsProperties(Alos2Metadata metadata, IDictionary<string, object> properties)
        {
            CultureInfo culture = new CultureInfo("fr-FR");
            // title
            properties["title"] = string.Format("ALOS-2 PALSAR-2 {0} L{1} {2} {3}",
                metadata.GetString("Ext_ObservationMode"),
                metadata.GetString("Lbi_ProcessLevel"),
                metadata.GetString("Ext_Polarizations"),
                properties.GetProperty<DateTime>("datetime").ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss", culture)
            );
        }

        private void AddOtherProperties(Alos2Metadata metadata, StacItem stacItem)
        {
            stacItem.Properties.Add("product_type", metadata.GetString("Ext_ObservationMode"));
            if (IncludeProviderProperty)
            {
                AddSingleProvider(
                    stacItem.Properties,
                    "JAXA",
                    "The Advanced Land Observing Satellite-2 is a follow-on mission from the ALOS. ALOS has contributed to cartography, regional observation, disaster monitoring, and resource surveys, since its launch in 2006. ALOS-2 will succeed this mission with enhanced capabilities.",
                    new StacProviderRole[] { StacProviderRole.producer, StacProviderRole.processor, StacProviderRole.licensor },
                    new Uri("https://www.eorc.jaxa.jp/ALOS/en/alos-2/a2_about_e.htm")
                );
            }
        }

        private GeoJSON.Net.Geometry.IGeometryObject GetGeometry(Alos2Metadata metadata)
        {
            GeoJSON.Net.Geometry.LineString lineString = new GeoJSON.Net.Geometry.LineString(
                new GeoJSON.Net.Geometry.Position[] {
                    new GeoJSON.Net.Geometry.Position(
                        metadata.GetDouble("Img_ImageSceneLeftTopLatitude"),
                        metadata.GetDouble("Img_ImageSceneLeftTopLongitude")
                    ),
                    new GeoJSON.Net.Geometry.Position(
                        metadata.GetDouble("Img_ImageSceneLeftBottomLatitude"),
                        metadata.GetDouble("Img_ImageSceneLeftBottomLongitude")
                    ),
                    new GeoJSON.Net.Geometry.Position(
                        metadata.GetDouble("Img_ImageSceneRightBottomLatitude"),
                        metadata.GetDouble("Img_ImageSceneRightBottomLongitude")
                    ),
                    new GeoJSON.Net.Geometry.Position(
                        metadata.GetDouble("Img_ImageSceneRightTopLatitude"),
                        metadata.GetDouble("Img_ImageSceneRightTopLongitude")
                    ),
                    new GeoJSON.Net.Geometry.Position(
                        metadata.GetDouble("Img_ImageSceneLeftTopLatitude"),
                        metadata.GetDouble("Img_ImageSceneLeftTopLongitude")
                    )
                }
            );

            return new GeoJSON.Net.Geometry.Polygon(new GeoJSON.Net.Geometry.LineString[] { lineString }).NormalizePolygon();
        }

        protected void AddAssets(StacItem stacItem, IItem item, Alos2Metadata metadata)
        {
            string identifier = metadata.GetString("Ext_Identifier");

            IAsset metadataAsset = GetMetadataAsset(item);
            stacItem.Assets.Add("metadata", StacAsset.CreateMetadataAsset(stacItem, metadataAsset.Uri, new ContentType("text/plain"), "Metadata summary file"));
            stacItem.Assets["metadata"].Properties.AddRange(metadataAsset.Properties);

            foreach (var fileName in metadata.Assets)
            {
                IAsset dataAsset = FindFirstAssetFromFileNameRegex(item, string.Format("{0}$", fileName.Replace(".", "\\."))) ?? throw new FileNotFoundException(string.Format("Data file '{0}' declared in summary, but not present", fileName));
                string polarization = fileName.Substring(4, 2);
                string key, contentType, title;
                if (fileName.EndsWith(".tif"))   // (IMG_*.tif)
                {
                    key = string.Format("amplitude-{0}", polarization.ToLower());
                    contentType = "image/tiff; application=geotiff";
                    title = "GeoTIFF data file";
                }
                else   // (LUT_*.txt)
                {
                    key = string.Format("lut-{0}", polarization.ToLower());
                    contentType = "text/plain";
                    title = "Pixel value to Sigma Naught Conversion factors";
                }
                StacAsset dataStacAsset = StacAsset.CreateDataAsset(stacItem, dataAsset.Uri, new ContentType(contentType), title);
                dataStacAsset.Properties.Add("sar:polarizations", new string[] { polarization });
                dataStacAsset.Properties.AddRange(dataAsset.Properties);
                stacItem.Assets.Add(key, dataStacAsset);
            }

            IAsset kmlAsset = FindFirstAssetFromFileNameRegex(item, "\\.kml$");
            if (kmlAsset != null)
            {
                StacAsset kmlStacAsset = StacAsset.CreateDataAsset(stacItem, kmlAsset.Uri, new ContentType("application/vnd.google-earth.kml+xml"), "Geometry (KML)");
                kmlStacAsset.Roles.Add("geometry");
                kmlStacAsset.Properties.AddRange(kmlAsset.Properties);
                stacItem.Assets.Add("kml", kmlStacAsset);
            }
        }

        private double GetGroundSampleDistance(Alos2Metadata metadata)
        {
            return 0;
        }

        private double GetGroundSampleDistance(string mode)
        {
            return 0;
        }

        protected virtual IAsset GetMetadataAsset(IItem item)
        {
            IAsset manifestAsset = FindFirstAssetFromFileNameRegex(item, @"summary\.txt$") ?? throw new FileNotFoundException(string.Format("Unable to find the summary file asset"));
            return manifestAsset;
        }

        public virtual async Task<Alos2Metadata> ReadMetadata(IAsset manifestAsset)
        {
            //logger.LogDebug("Opening metadata summary {0}", manifestAsset.Uri);

            Alos2Metadata metadata = new Alos2Metadata(manifestAsset);

            await metadata.ReadMetadata(resourceServiceProvider);

            return metadata;

        }


        protected KeyValuePair<string, StacAsset> CreateManifestAsset(IStacObject stacObject, IAsset asset)
        {
            StacAsset stacAsset = StacAsset.CreateMetadataAsset(stacObject, asset.Uri, new ContentType("text/xml"), "SAFE Manifest");
            stacAsset.Properties.AddRange(asset.Properties);
            return new KeyValuePair<string, StacAsset>("manifest", stacAsset);
        }

    }


    public class Alos2Metadata
    {

        // Regular expressions should normally be static but creates problem with async methods
        private readonly Regex lineRegex = new Regex("^ *([^= ]+) *= *(.*)$");
        private readonly Regex quotedValueRegex = new Regex("^\"(.*)\"$");
        private readonly Regex fileNameKeyRegex = new Regex("Pdi_L\\d{2}ProductFileName\\d{2}");   // e.g. Pdi_L15ProductFileName01
        private readonly Regex fileNameValueRegex = new Regex("(?'type'IMG|LUT)-(?'pol'HH|HV|VH|VV)-(?'id'.{32})\\.(tif|txt)");   // e.g. IMG-HH-ALOS2146686640-170209-FBDR1.5GUA.tif
        private readonly Regex identifierRegex = new Regex("ALOS2(?'orbit'\\d{5})(?'frame'\\d{4})-(?'date'\\d{6})-(?'mode'(SBS|UBS|UBD|HBS|HBD|HBQ|FBS|FBD|FBQ|WBS|WBD|WWS|WWD|VBS|VBD))(?'obsdir'[LR])(?'level'.{3})(?'proc'[GR_])(?'proj'[UPML_])(?'orbitdir'[AD])");  // e.g. ALOS2146686640-170209-FBDR1.5GUA
        private readonly Regex siteRegex = new Regex(".* (?'dt'\\d{8} \\d{6})$");
        private readonly IAsset summaryAsset;

        private Dictionary<string, string> properties { get; set; }

        public List<string> Assets { get; set; }

        public Alos2Metadata(IAsset summaryAsset)
        {
            this.summaryAsset = summaryAsset;
            properties = new Dictionary<string, string>();
            Assets = new List<string>();
        }

        public async Task ReadMetadata(IResourceServiceProvider resourceServiceProvider)
        {
            //logger.LogDebug("Opening Manifest {0}", manifestAsset.Uri);

            List<string> polarizations = new List<string>();

            using (var stream = await resourceServiceProvider.GetAssetStreamAsync(summaryAsset, System.Threading.CancellationToken.None))
            {
                using (StreamReader reader = new StreamReader(stream))
                {

                    while (!reader.EndOfStream)
                    {
                        string line = await reader.ReadLineAsync();
                        //Console.WriteLine(line);
                        Match match = lineRegex.Match(line);
                        if (!match.Success) continue;

                        string key = match.Groups[1].Value;
                        string value = match.Groups[2].Value.Trim();
                        match = quotedValueRegex.Match(value);
                        if (match.Success) value = match.Groups[1].Value;

                        if (fileNameKeyRegex.Match(key).Success)
                        {
                            Match fileNameValueMatch = fileNameValueRegex.Match(value);
                            if (fileNameValueMatch.Success)
                            {
                                string polarization = fileNameValueMatch.Groups["pol"].Value;
                                if (!polarizations.Contains(polarization)) polarizations.Add(polarization);
                                Assets.Add(value);
                            }
                        }
                        else
                        {
                            properties[key] = value;
                        }
                    }

                    properties["Ext_Identifier"] = string.Format("{0}-{1}", properties["Scs_SceneID"], properties["Pds_ProductID"]);

                    Match identifierMatch = identifierRegex.Match(properties["Ext_Identifier"]);
                    if (!identifierMatch.Success)
                    {
                        throw new Exception("Non-compliant ALOS2 scene ID and/or product ID");
                    }

                    // The following are extension properties (self-defined names, starting with 'Ext_')
                    // derived from scene ID and product ID
                    properties["Ext_OrbitNumber"] = identifierMatch.Groups["orbit"].Value;
                    properties["Ext_SceneFrameNumber"] = identifierMatch.Groups["frame"].Value;
                    properties["Ext_ObservationDate"] = identifierMatch.Groups["date"].Value;   // Lbi_ObservationDate more useful
                    properties["Ext_ObservationMode"] = identifierMatch.Groups["mode"].Value;
                    properties["Ext_ObservationDirection"] = identifierMatch.Groups["obsdir"].Value;
                    properties["Ext_ProcessingLevel"] = identifierMatch.Groups["level"].Value;   // Lbi_ProcessLevel more explicit
                    properties["Ext_ProcessingOption"] = identifierMatch.Groups["proc"].Value;
                    properties["Ext_MapProjection"] = identifierMatch.Groups["proj"].Value;
                    properties["Ext_OrbitDirection"] = identifierMatch.Groups["orbitdir"].Value == "A" ? "ASCENDING" : "DESCENDING";

                    // Polarization(s), combined by slash(es) if multiple
                    properties["Ext_Polarizations"] = string.Join("/", polarizations);

                    Match siteMatch = siteRegex.Match(properties["Odi_SiteDateTime"]);
                    if (siteMatch.Success)
                    {
                        properties["Ext_ProcessedTime"] = siteMatch.Groups["dt"].Value;
                    }

                    /*foreach (string key in properties.Keys)
                    {
                        Console.WriteLine("PROPERTY {0} = {1}", key, properties[key]);
                    }
                    foreach (string asset in Assets)
                    {
                        Console.WriteLine("ASSET {0}", asset);
                    }*/
                }
            }
        }

        public string GetString(string key, bool throwIfMissing = true)
        {
            if (properties.ContainsKey(key))
            {
                return properties[key];
            }
            if (throwIfMissing) throw new Exception(string.Format("No value for key '{0}'", key));
            return null;
        }

        public int GetInt32(string key, bool throwIfMissing = true)
        {
            if (properties.ContainsKey(key))
            {
                if (int.TryParse(properties[key], out int value))
                    return value;
                else
                    throw new FormatException(string.Format("Invalid value for key '{0}' (not an int)", key));
            }
            if (throwIfMissing) throw new Exception(string.Format("No value for key '{0}'", key));
            return 0;
        }

        public long GetInt64(string key, bool throwIfMissing = true)
        {
            if (properties.ContainsKey(key))
            {
                if (long.TryParse(properties[key], out long value))
                    return value;
                else
                    throw new FormatException(string.Format("Invalid value for key '{0}' (not a long)", key));
            }
            if (throwIfMissing) throw new Exception(string.Format("No value for key '{0}'", key));
            return 0;
        }

        public double GetDouble(string key, bool throwIfMissing = true)
        {
            if (properties.ContainsKey(key))
            {
                if (double.TryParse(properties[key], out double value))
                    return value;
                else
                    throw new FormatException(string.Format("Invalid value for key '{0}' (not a double)", key));
            }
            if (throwIfMissing) throw new Exception(string.Format("No value for key '{0}'", key));
            return 0;
        }

        public DateTime GetDateTime(string key, int[] positions = null, bool throwIfMissing = true)
        {
            if (properties.ContainsKey(key))
            {
                string raw = properties[key];
                string input;
                if (positions == null)
                {
                    input = raw;
                }
                else if (positions.Length >= 3)
                {
                    input = string.Format("{0}-{1}-{2}T{3}:{4}:{5}Z",
                        raw.Substring(positions[0], 4),
                        raw.Substring(positions[1], 2),
                        raw.Substring(positions[2], 2),
                        positions.Length >= 6 ? raw.Substring(positions[3], 2) : "00",
                        positions.Length >= 6 ? raw.Substring(positions[4], 2) : "00",
                        positions.Length >= 6 ? raw.Substring(positions[5], 2) : "00"
                    );
                }
                else
                {
                    throw new FormatException(string.Format("Invalid format expectation for key '{0}' (not a date/time)", key));
                }
                if (DateTime.TryParse(input, null, DateTimeStyles.AssumeUniversal, out DateTime value))
                    return value;
                else
                    throw new FormatException(string.Format("Invalid value for key '{0}' (not a date/time)", key));
            }
            if (throwIfMissing) throw new Exception(string.Format("No value for key '{0}'", key));
            return DateTime.MinValue;
        }
    }
}
