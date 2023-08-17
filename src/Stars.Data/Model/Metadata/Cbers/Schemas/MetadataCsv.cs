using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Terradue.Stars.Data.Model.Metadata.Cbers.Schemas {
    public class MetadataCsv {
        public string AGENCY { get; set; }
        public string SATELLITE { get; set; }
        public string INSTRUMENT { get; set; }
        public string MODE { get; set; }
        public string IDENTIFIER { get; set; }
        public string START_DATE { get; set; }
        public string COMPLETE_DATE { get; set; }
        public string FOOTPRINT { get; set; }
        public string PRODUCT_URL { get; set; }
        public string AOI { get; set; }
        public string FORMAT { get; set; }
        public string PROCESSING_LEVEL { get; set; }
        public string PROCESSING_LEVEL_DESCRIPTION { get; set; }
        public string PROJECTION { get; set; }
        public string EPSG { get; set; }
        public string PATH { get; set; }
        public string ROW { get; set; }
        public string OFF_NADIR { get; set; }
        public string SUN_AZIMUTH { get; set; }
        public string SUN_ELEVATION { get; set; }
        public string ABSOLUTE_CALIBRATION_COEFFICIENTS { get; set; }

        public Metadata getMetadata() {
            Metadata metadata = new Metadata();
            // SATELLITE
            metadata.satellite = new prdfSatellite();
            metadata.satellite.name = "CBERS";

            string pattern = @"CBERS(\d+)";
            Match match = Regex.Match(SATELLITE, pattern);
            if (match.Success) {
                string numberString = match.Groups[1].Value;
                metadata.satellite.number = numberString;
            }
            else {
                throw new Exception("No match found");
            }

            // INSTRUMENT
            metadata.satellite.instrument = new prdfSatelliteInstrument
            {
                Value = INSTRUMENT
            };
            
            // IDENTIFIER
            metadata.identifier = IDENTIFIER;

            var footprintArray = FOOTPRINT.Trim().Split(' ');
            var bboxArray = FindBoundingBox(footprintArray);
            
            // IMAGE
            metadata.image = new prdfImage
            {
                // START_DATE COMPLETE_DATE CENTER_DATE
                timeStamp = new prdfImageTimeStamp
                {
                    begin = START_DATE,
                    center =  CalculateMeanDate(START_DATE, COMPLETE_DATE),
                    end = COMPLETE_DATE
                },
                // FOOTPRINT
                boundingBox = new prdfImageBoundingBox
                {
                    UR = new UR
                    {
                        latitude = bboxArray[0],
                        longitude = bboxArray[1]
                    },
                    UL = new UL
                    {
                        latitude = bboxArray[2],
                        longitude = bboxArray[3]
                    },
                    LR = new LR
                    {
                        latitude = bboxArray[4],
                        longitude = bboxArray[5]
                    },
                    LL = new LL
                    {
                        latitude = bboxArray[6],
                        longitude = bboxArray[7]
                    }
                },

                // SUN_AZIMUTH  SUN_ELEVATION
                sunPosition = new prdfImageSunPosition {
                    elevation = SUN_ELEVATION,
                    sunAzimuth = SUN_AZIMUTH
                },
                
                // PATH ROW
                row = ROW,
                path = PATH,
                
                absoluteCalibrationCoefficient = GetBandValuesFromAbsoluteCalibrationCoefficients(ABSOLUTE_CALIBRATION_COEFFICIENTS),
                
            };
            
            // PROCESSING_LEVEL
            pattern = @"L(\d+)";
            match = Regex.Match(PROCESSING_LEVEL, pattern);
            if (match.Success) {
                string level = match.Groups[1].Value;
                metadata.image.level = level;
            }
            else {
                throw new Exception("No match found");
            }
            
            // PROCESSING_LEVEL_DESCRIPTION
            // PROJECTION
            // EPSG
            // OFF_NADIR
            // MODE
            // PRODUCT_URL
            // AOI
            // FORMAT
            // AGENCY
            
            return metadata;
        }


        private static string[] FindBoundingBox(string[] uniqueCoordinates) {
            if (uniqueCoordinates == null)
                throw new ArgumentNullException(nameof(uniqueCoordinates));
            if (uniqueCoordinates.Length < 4 || uniqueCoordinates.Length % 2 != 0)
                throw new ArgumentException("Invalid number of coordinates.");

            var numericCoordinates = new List<double>();
            foreach (string coordinate in uniqueCoordinates) {
                if (!double.TryParse(coordinate, out double numericValue))
                    throw new ArgumentException("Invalid numeric value in the coordinates.");

                numericCoordinates.Add(numericValue);
            }

            var xMin = numericCoordinates[0];
            var xMax = numericCoordinates[0];
            var yMin = numericCoordinates[1];
            var yMax = numericCoordinates[1];

            for (var i = 2; i < numericCoordinates.Count; i += 2) {
                var xCoordinate = numericCoordinates[i];
                var yCoordinate = numericCoordinates[i + 1];

                xMin = Math.Min(xMin, xCoordinate);
                xMax = Math.Max(xMax, xCoordinate);
                yMin = Math.Min(yMin, yCoordinate);
                yMax = Math.Max(yMax, yCoordinate);
            }

            return new[]
            {
                xMin.ToString(), yMax.ToString(),
                xMax.ToString(), yMax.ToString(),
                xMax.ToString(), yMin.ToString(),
                xMin.ToString(), yMin.ToString()
            };
        }


        private static band[] GetBandValuesFromAbsoluteCalibrationCoefficients(string input)
        {
            List<band> attributeValues = new List<band>();

            // Regular expression to match each attribute and its value
            string pattern = @"band\d+:(\d+\.\d+)";
            MatchCollection matches = Regex.Matches(input, pattern);

            foreach (Match match in matches)
            {
                if (match.Groups.Count >= 2)
                {
                    attributeValues.Add(new band() {  Value = match.Groups[1].Value} );
                }
            }

            return attributeValues.ToArray();
        }
        
        
        private static string CalculateMeanDate(string dateString1, string dateString2)
        {
            DateTime date1 = DateTime.Parse(dateString1, null, DateTimeStyles.AssumeUniversal).ToUniversalTime();
            DateTime date2 = DateTime.Parse(dateString2, null, DateTimeStyles.AssumeUniversal).ToUniversalTime();

            // Calculate the total ticks (nanoseconds) from both dates
            long totalTicks = (date1.Ticks + date2.Ticks) / 2;

            // Create the mean date
            DateTime meanDate = new DateTime(totalTicks, DateTimeKind.Utc).ToUniversalTime();

            return meanDate.ToString("yyyy-MM-ddTHH:mm:ssZ");
        }
    }
}