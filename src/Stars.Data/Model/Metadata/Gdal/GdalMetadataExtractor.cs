using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Mime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MaxRev.Gdal.Core;
using Microsoft.Extensions.Logging;
using OSGeo.GDAL;
using ProjNet.CoordinateSystems;
using Stac;
using Stac.Extensions.Processing;
using Stac.Extensions.Projection;
using Stac.Extensions.Sar;
using Stac.Extensions.Sat;
using Stac.Extensions.View;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Model.Stac;

namespace Terradue.Stars.Data.Model.Metadata.Alos2
{
    public class GdalMetadataExtractor : MetadataExtraction
    {
        private readonly string GDALFILE_REGEX = @".*\.(tif|tiff)$";

        public override string Label => "Advanced Land Observing Satellite-2 (JAXA) mission product metadata extractor";

        public GdalMetadataExtractor(ILogger<GdalMetadataExtractor> logger) : base(logger)
        {
            GdalBase.ConfigureAll();
        }

        public override bool CanProcess(IResource route, IDestination destination)
        {
            IItem item = route as IItem;
            if (item == null) return false;
            try
            {
                var gdalAsset = GetGdalAsset(item);
                OSGeo.GDAL.Dataset dataset = LoadGdalAsset(gdalAsset).GetAwaiter().GetResult();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        protected override async Task<StacNode> ExtractMetadata(IItem item, string suffix)
        {
            KeyValuePair<string, IAsset> gdalAsset = GetGdalAsset(item);
            Dataset dataset = await LoadGdalAsset(gdalAsset);

            StacItem stacItem = CreateStacItem(gdalAsset, dataset, item);

            AddAssets(stacItem, gdalAsset, dataset);

            return StacItemNode.Create(stacItem, item.Uri); ;
        }

        internal virtual StacItem CreateStacItem(KeyValuePair<string, IAsset> gdalAsset, Dataset dataset, IItem item)
        {
            string identifier = gdalAsset.Key;
            StacItem stacItem = new StacItem(identifier, GetGeometry(dataset));
            FillCommonMetadata(stacItem, dataset, item);
            // AddSatStacExtension(metadata, stacItem);
            // AddSarStacExtension(metadata, stacItem);
            // AddProjStacExtension(metadata, stacItem);
            // AddViewStacExtension(metadata, stacItem);
            // AddProcessingStacExtension(metadata, stacItem);
            //FillBasicsProperties(metadata, stacItem.Properties);
            return stacItem;
        }

        // private void AddSatStacExtension(Alos2Metadata metadata, StacItem stacItem)
        // {
        //     var sat = new SatStacExtension(stacItem);
        //     sat.AbsoluteOrbit = metadata.GetInt32("Ext_OrbitNumber");
        //     sat.RelativeOrbit = sat.AbsoluteOrbit;   // TODO remove?
        //     sat.OrbitState = metadata.GetString("Ext_OrbitDirection").ToLower();
        // }

        // private void AddSarStacExtension(Alos2Metadata metadata, StacItem stacItem)
        // {
        //     SarStacExtension sar = stacItem.SarExtension();
        //     sar.Required(metadata.GetString("Ext_ObservationMode"),
        //         SarCommonFrequencyBandName.L,
        //         metadata.GetString("Ext_Polarizations").Split('/'),
        //         metadata.GetString("Ext_ObservationMode")
        //     );

        //     sar.ObservationDirection = ParseObservationDirection(metadata.GetString("Ext_ObservationDirection"));
        // }

        // private void AddProjStacExtension(Alos2Metadata metadata, StacItem stacItem)
        // {
        //     ProjectionStacExtension proj = stacItem.ProjectionExtension();
        //     try
        //     {
        //         int utmZone = metadata.GetInt32("Pds_UTM_ZoneNo");
        //         bool north = metadata.GetString("Pds_MapDirection").Contains("MapNorth");
        //         ProjectedCoordinateSystem utm = ProjectedCoordinateSystem.WGS84_UTM(utmZone, north);
        //         proj.SetCoordinateSystem(utm);
        //     }
        //     catch { }
        //     try
        //     {
        //         stacItem.ProjectionExtension().Shape = new int[2] { metadata.GetInt32("Pdi_NoOfPixels_0"), metadata.GetInt32("Pdi_NoOfLines_0") };
        //     }
        //     catch { }
        // }

        // private void AddViewStacExtension(Alos2Metadata metadata, StacItem stacItem)
        // {
        //     var view = new ViewStacExtension(stacItem);
        //     view.OffNadir = metadata.GetDouble("Img_OffNadirAngle");
        // }

        // private void AddProcessingStacExtension(Alos2Metadata metadata, StacItem stacItem)
        // {
        //     var proc = stacItem.ProcessingExtension();
        //     // proc.Level = GetProcessingLevel(metadata);
        // }

        private void FillCommonMetadata(StacItem stacItem, Dataset dataset, IItem item)
        {
            FillDateTimeProperties(stacItem, dataset, item);
            // TODO Licensing
            // TODO Provider
            FillInstrument(stacItem, dataset, item);
        }

        private void FillDateTimeProperties(StacItem stacItem, Dataset dataset, IItem item)
        {
            var dateTime = GetDateTime(dataset, item);
            stacItem.DateTime = dateTime.HasValue ? new Itenso.TimePeriod.TimeInterval(dateTime.Value) : null;

            DateTime? createdDate = GetCreatedDateTime(dataset, item);
            if (createdDate.HasValue)
                stacItem.Created = createdDate.Value;
        }

        private DateTime? GetDateTime(Dataset dataset, IItem item)
        {
            if (!string.IsNullOrEmpty(dataset.GetMetadataItem("DateTimeOriginal", "EXIF")))
            {
                try
                {
                    return DateTime.Parse(dataset.GetMetadataItem("DateTimeOriginal", "EXIF"), null, DateTimeStyles.AssumeUniversal);
                }
                catch { }
            }
            if (item is StacItem)
                return (item as StacItem).DateTime?.Start;

            return null;
        }

        private DateTime? GetCreatedDateTime(Dataset dataset, IItem item)
        {
            if (!string.IsNullOrEmpty(dataset.GetMetadataItem("DateTime", "EXIF")))
            {
                try
                {
                    return DateTime.Parse(dataset.GetMetadataItem("DateTime", "EXIF"), null, DateTimeStyles.AssumeUniversal);
                }
                catch { }
            }
            if (item is StacItem && (item as StacItem).Created.Ticks > 0)
                return (item as StacItem).Created;

            return null;
        }

        private void FillInstrument(StacItem stacItem, Dataset dataset, IItem item)
        {
            // platform & constellation
            stacItem.Instruments = GetInstrument(dataset, item);
        }

        private IEnumerable<string> GetInstrument(Dataset dataset, IItem item)
        {
            if (!string.IsNullOrEmpty(dataset.GetMetadataItem("CameraLabel", "EXIF")))
            {
                try
                {
                    return new string[1] { dataset.GetMetadataItem("CameraLabel", "EXIF") };
                }
                catch { }
            }
            if (item is StacItem && (item as StacItem).Instruments != null)
                return (item as StacItem).Instruments;

            return null;
        }


        private GeoJSON.Net.Geometry.IGeometryObject GetGeometry(Dataset dataset)
        {
            var box = GetRasterExtent(dataset);

            if (box != null && !(box.MinX == 0 && box.MinY == 0))
            {
                GeoJSON.Net.Geometry.LineString lineString = new GeoJSON.Net.Geometry.LineString(
                    new GeoJSON.Net.Geometry.Position[] {
                        new GeoJSON.Net.Geometry.Position(
                            box.MaxY,
                            box.MinX
                        ),
                        new GeoJSON.Net.Geometry.Position(
                            box.MinY,
                            box.MinX
                        ),
                        new GeoJSON.Net.Geometry.Position(
                            box.MinY,
                            box.MaxX
                        ),
                        new GeoJSON.Net.Geometry.Position(
                            box.MaxY,
                            box.MaxX
                        ),
                        new GeoJSON.Net.Geometry.Position(
                            box.MaxY,
                            box.MinX
                        )
                    });
                return new GeoJSON.Net.Geometry.Polygon(new GeoJSON.Net.Geometry.LineString[] { lineString });
            }
            return null;


        }

        public OSGeo.OGR.Envelope GetRasterExtent(Dataset dataset, string proj4 = "+proj=latlong +datum=WGS84 +no_defs")
        {

            OSGeo.OSR.SpatialReference srcSRS = new OSGeo.OSR.SpatialReference(dataset.GetProjection());
            OSGeo.OGR.Envelope extent;

            if (dataset.RasterCount == 0)
                return null;

            extent = GetBaseRasterExtent(dataset);

            if (string.IsNullOrEmpty(dataset.GetProjection()))
            {
                srcSRS = new OSGeo.OSR.SpatialReference("");
                srcSRS.ImportFromProj4(proj4);
            }

            if (srcSRS.__str__().Contains("AUTHORITY[\"EPSG\",\"3857\"]") || srcSRS.__str__().Contains("LOCAL_CS[\"WGS 84 / Pseudo-Mercator\""))
            {
                srcSRS.ImportFromProj4("+proj=merc +a=6378137 +b=6378137 +lat_ts=0.0 +lon_0=0.0 +x_0=0.0 +y_0=0 +k=1.0 +units=m +nadgrids=@null +wktext  +no_defs");
            }

            OSGeo.OSR.SpatialReference dstSRS = new OSGeo.OSR.SpatialReference("");
            dstSRS.ImportFromProj4(proj4);

            if (dstSRS.IsSame(srcSRS, null) == 1)
            {
                return extent;
            }

            OSGeo.OSR.CoordinateTransformation ct;

            ct = new OSGeo.OSR.CoordinateTransformation(srcSRS, dstSRS);

            double[] newXs = new double[] { extent.MaxX, extent.MinX };
            double[] newYs = new double[] { extent.MaxY, extent.MinY };

            ct.TransformPoints(2, newXs, newYs, new double[] { 0, 0 });

            extent.MaxX = newXs[0];
            extent.MinX = newXs[1];
            extent.MaxY = newYs[0];
            extent.MinY = newYs[1];

            return extent;
        }

        public OSGeo.OGR.Envelope GetBaseRasterExtent(Dataset dataset)
        {

            if (dataset.RasterCount > 0)
            {
                OSGeo.OGR.Envelope extent = new OSGeo.OGR.Envelope();
                double[] geoTransform = new double[6];
                dataset.GetGeoTransform(geoTransform);

                extent.MinX = geoTransform[0];
                extent.MinY = geoTransform[3];
                extent.MaxX = geoTransform[0] + (dataset.RasterXSize * geoTransform[1]) + (geoTransform[2] * dataset.RasterYSize);
                extent.MaxY = geoTransform[3] + (dataset.RasterYSize * geoTransform[5]) + (geoTransform[4] * dataset.RasterXSize); ;

                return extent;
            }

            return null;

        }

        protected void AddAssets(StacItem stacItem, Dataset gdalDataset, KeyValuePair<string, IAsset> gdalAsset)
        {
            string identifier = metadata.GetString("Ext_Identifier");

            IAsset metadataAsset = GetGdalAsset(item);
            stacItem.Assets.Add("metadata", StacAsset.CreateMetadataAsset(stacItem, metadataAsset.Uri, new ContentType("text/plain"), "Metadata summary file"));
            stacItem.Assets["metadata"].Properties.AddRange(metadataAsset.Properties);

            foreach (var fileName in metadata.Assets)
            {
                IAsset dataAsset = FindFirstAssetFromFileNameRegex(item, String.Format("{0}$", fileName.Replace(".", "\\.")));
                if (dataAsset == null)
                    throw new FileNotFoundException(string.Format("Data file '{0}' declared in summary, but not present", fileName));
                string polarization = fileName.Substring(4, 2);
                string key, contentType, title;
                if (fileName.EndsWith(".tif"))   // (IMG_*.tif)
                {
                    key = String.Format("amplitude-{0}", polarization.ToLower());
                    contentType = "image/tiff; application=geotiff";
                    title = "GeoTIFF data file";
                }
                else   // (LUT_*.txt)
                {
                    key = String.Format("lut-{0}", polarization.ToLower());
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

        protected virtual KeyValuePair<string, IAsset> GetGdalAsset(IItem item)
        {
            var gdalAsset = FindFirstKeyAssetFromFileNameRegex(item, GDALFILE_REGEX);
            if (gdalAsset.Key == null)
            {
                throw new FileNotFoundException(String.Format("Unable to find the summary file asset"));
            }
            return gdalAsset;
        }

        public virtual async Task<OSGeo.GDAL.Dataset> LoadGdalAsset(KeyValuePair<string, IAsset> gdalAsset)
        {
            OSGeo.GDAL.Dataset dataset = Gdal.Open(gdalAsset.Value.Uri.ToString(), Access.GA_ReadOnly);

            dataset.GetDriver();

            return dataset;
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
        private Regex lineRegex = new Regex("^ *([^= ]+) *= *(.*)$");
        private Regex quotedValueRegex = new Regex("^\"(.*)\"$");
        private Regex fileNameKeyRegex = new Regex("Pdi_L\\d{2}ProductFileName\\d{2}");   // e.g. Pdi_L15ProductFileName01
        private Regex fileNameValueRegex = new Regex("(?'type'IMG|LUT)-(?'pol'HH|HV|VH|VV)-(?'id'.{32})\\.(tif|txt)");   // e.g. IMG-HH-ALOS2146686640-170209-FBDR1.5GUA.tif
        private Regex identifierRegex = new Regex("ALOS2(?'orbit'\\d{5})(?'frame'\\d{4})-(?'date'\\d{6})-(?'mode'(SBS|UBS|UBD|HBS|HBD|HBQ|FBS|FBD|FBQ|WBS|WBD|WWS|WWD|VBS|VBD))(?'obsdir'[LR])(?'level'.{3})(?'proc'[GR_])(?'proj'[UPML_])(?'orbitdir'[AD])");  // e.g. ALOS2146686640-170209-FBDR1.5GUA
        private Regex siteRegex = new Regex(".* (?'dt'\\d{8} \\d{6})$");
        private IAsset summaryAsset;

        private Dictionary<string, string> properties { get; set; }

        public List<string> Assets { get; set; }

        public Alos2Metadata(IAsset summaryAsset)
        {
            this.summaryAsset = summaryAsset;
            this.properties = new Dictionary<string, string>();
            Assets = new List<string>();
        }

        public async Task ReadMetadata()
        {
            //logger.LogDebug("Opening Manifest {0}", manifestAsset.Uri);

            List<string> polarizations = new List<string>();

            using (var stream = await summaryAsset.GetStreamable().GetStreamAsync())
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

                    properties["Ext_Identifier"] = String.Format("{0}-{1}", properties["Scs_SceneID"], properties["Pds_ProductID"]);

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
                    properties["Ext_Polarizations"] = String.Join("/", polarizations);

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
            if (throwIfMissing) throw new Exception(String.Format("No value for key '{0}'", key));
            return null;
        }

        public int GetInt32(string key, bool throwIfMissing = true)
        {
            if (properties.ContainsKey(key))
            {
                if (Int32.TryParse(properties[key], out int value))
                    return value;
                else
                    throw new FormatException(String.Format("Invalid value for key '{0}' (not an int)", key));
            }
            if (throwIfMissing) throw new Exception(String.Format("No value for key '{0}'", key));
            return 0;
        }

        public long GetInt64(string key, bool throwIfMissing = true)
        {
            if (properties.ContainsKey(key))
            {
                if (Int64.TryParse(properties[key], out long value))
                    return value;
                else
                    throw new FormatException(String.Format("Invalid value for key '{0}' (not a long)", key));
            }
            if (throwIfMissing) throw new Exception(String.Format("No value for key '{0}'", key));
            return 0;
        }

        public double GetDouble(string key, bool throwIfMissing = true)
        {
            if (properties.ContainsKey(key))
            {
                if (Double.TryParse(properties[key], out double value))
                    return value;
                else
                    throw new FormatException(String.Format("Invalid value for key '{0}' (not a double)", key));
            }
            if (throwIfMissing) throw new Exception(String.Format("No value for key '{0}'", key));
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
                    input = String.Format("{0}-{1}-{2}T{3}:{4}:{5}Z",
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
                    throw new FormatException(String.Format("Invalid format expectation for key '{0}' (not a date/time)", key));
                }
                if (DateTime.TryParse(input, null, DateTimeStyles.AssumeUniversal, out DateTime value))
                    return value;
                else
                    throw new FormatException(String.Format("Invalid value for key '{0}' (not a date/time)", key));
            }
            if (throwIfMissing) throw new Exception(String.Format("No value for key '{0}'", key));
            return DateTime.MinValue;
        }
    }
}
