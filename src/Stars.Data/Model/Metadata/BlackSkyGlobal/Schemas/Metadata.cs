using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Terradue.Stars.Data.Model.Metadata.BlackSkyGlobal.Schemas {


    public partial class Metadata {

        private bool isFromText = false;

        public static Regex keyValueRegex = new Regex(@"(?'key'[^= ]+) *= *(?'value'.*)");
        public static string[] cornerCoordinateNames = new string[] {
            "LL_LONG", "LL_LAT", "LR_LONG", "LR_LAT", "UR_LONG", "UR_LAT", "UL_LONG", "UL_LAT"
        };

        public bool IsFromText {
            get { return isFromText; }
            set { isFromText = value; }
        }

        public string id { get; set; }
        public string acquisitionDate { get; set; }
        public string sensorName { get; set; }
        public string spectralMode { get; set; }
        public double? gsd { get; set; }
        public object geometry { get; set; }
        public double? cloudCoverPercent { get; set; }
        public double? offNadirAngle { get; set; }
        public double? sunElevation { get; set; }
        public double? sunAzimuth { get; set; }
        public double? satelliteElevation { get; set; }
        public double? satelliteAzimuth { get; set; }
        public bool? georeferenced { get; set; }
        public bool? orthorectified { get; set; }
        public int? width { get; set; }
        public int? height { get; set; }
        public string processingVersion { get; set; }
        public string targetType { get; set; }
        public double? estimatedCE90 { get; set; }
        public int? bitsPerPixel { get; set; }

        public string imageSpecVersion { get; set; }
        public double? fractionSaturated { get; set; }
        public object numberRegistrationPoints { get; set; }

        public GeminiType gemini { get; set; }

        // From text file
        public string processLevel { get; set; }

        public static Metadata FromTextFile(StreamReader reader)
        {
            Metadata metadata = new Metadata
            {
                IsFromText = true
            };

            Dictionary<string, string> textMetadata = new Dictionary<string, string>();

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                Match match = keyValueRegex.Match(line);
                if (match.Success)
                {
                    string key = match.Groups["key"].Value;
                    string value = match.Groups["value"].Value;
                    textMetadata[key] = value;
                }
            }

            metadata.id = GetStringValue(textMetadata, "VENDOR_SCENE_ID");
            string platformNumber = GetStringValue(textMetadata, "PLATFORM_NUMBER");
            if (platformNumber == null)
            {
                string fileName = GetStringValue(textMetadata, "ENTITY_ID");
                if (fileName != null && fileName.StartsWith("BS")) platformNumber = fileName.Substring(2, 2).TrimStart('0');
            }
            metadata.sensorName = String.Format("Global-{0}", platformNumber);
            metadata.spectralMode = GetStringValue(textMetadata, "SENSOR_TYPE");
            string acquisitionDate = GetStringValue(textMetadata, "ACQUISITION_DATE");
            if (!String.IsNullOrEmpty(acquisitionDate))
            {
                if (acquisitionDate.Length == 8) acquisitionDate = String.Format("{0}-{1}-{2}", acquisitionDate.Substring(0, 4), acquisitionDate.Substring(4, 2), acquisitionDate.Substring(6, 2));
                string acquisitionTime = GetStringValue(textMetadata, "ACQUISITION_TIME");
                if (String.IsNullOrEmpty(acquisitionTime))
                {
                    acquisitionTime = "00:00:00";
                }
                else
                {
                    if (acquisitionTime.Length == 6) acquisitionTime = String.Format("{0}:{1}:{2}", acquisitionTime.Substring(0, 2), acquisitionTime.Substring(2, 2), acquisitionTime.Substring(4, 2));
                }
                acquisitionTime = acquisitionTime.Replace("Z", "");
                metadata.acquisitionDate = String.Format("{0}T{1}Z", acquisitionDate, acquisitionTime);
            }
            metadata.gsd = 1;   // hardcoded

            List<double> coordinates = new List<double>();
            foreach (string name in cornerCoordinateNames)
            {
                if (Double.TryParse(GetStringValue(textMetadata, name), out double value)) coordinates.Add(value);
            }
            if (coordinates.Count == 8)
            {
                GeoJSON.Net.Geometry.Position[] lineStringPositions = new GeoJSON.Net.Geometry.Position[]
                {
                    new GeoJSON.Net.Geometry.Position(coordinates[1], coordinates[0]),
                    new GeoJSON.Net.Geometry.Position(coordinates[3], coordinates[2]),
                    new GeoJSON.Net.Geometry.Position(coordinates[5], coordinates[4]),
                    new GeoJSON.Net.Geometry.Position(coordinates[7], coordinates[6]),
                    new GeoJSON.Net.Geometry.Position(coordinates[1], coordinates[0])
                };
                GeoJSON.Net.Geometry.LineString lineString = new GeoJSON.Net.Geometry.LineString(lineStringPositions);
                metadata.geometry = new GeoJSON.Net.Geometry.Polygon(new GeoJSON.Net.Geometry.LineString[] { lineString });
            }

            metadata.cloudCoverPercent = GetDoubleValue(textMetadata, "CLOUD_COVER");
            metadata.offNadirAngle = GetDoubleValue(textMetadata, "OFF_NADIR");
            metadata.sunElevation = GetDoubleValue(textMetadata, "SUN_ELEVATION");
            metadata.sunAzimuth = GetDoubleValue(textMetadata, "SUN_AZIMUTH");
            metadata.satelliteElevation = GetDoubleValue(textMetadata, "SAT_ELEVATION");
            metadata.satelliteAzimuth= GetDoubleValue(textMetadata, "SAT_AZIMUTH");
            //metadata.georeferenced= "";
            //metadata.orthorectified= "";
            //metadata.width= "";
            //metadata.height= "";
            //metadata.processingVersion= "";
            //metadata.targetType= "";
            //metadata.estimatedCE90= "";
            //metadata.bitsPerPixel= "";

            //metadata.imageSpecVersion= "";
            //metadata.fractionSaturated= "";
            //metadata.numberRegistrationPoints= "";

            metadata.processLevel = GetStringValue(textMetadata, "PROCESS_LEVEL");


            return metadata;
        }

        public static string GetStringValue(Dictionary<string, string> dict, string key)
        {
            if (dict.ContainsKey(key)) return dict[key];
            return null;
        }

        public static double? GetDoubleValue(Dictionary<string, string> dict, string key)
        {
            if (dict.ContainsKey(key))
            {
                if (Double.TryParse(dict[key], out double value)) return value;
            }
            return null;
        }


    }

    public partial class GeminiType {
        public string catalogImageId { get; set; }
        public string imageId { get; set; }
    }


}
