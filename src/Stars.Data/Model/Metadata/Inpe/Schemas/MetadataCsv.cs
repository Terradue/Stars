using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Terradue.Stars.Data.Model.Metadata.Inpe.Schemas {
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

            string pattern = @"(CBERS|AMAZONIA)(.*)";
            Match match = Regex.Match(SATELLITE, pattern);
            if (match.Success) {
                metadata.satellite.name = match.Groups[1].Value;
                metadata.satellite.number = match.Groups[2].Value;
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
                    UL = new UL
                    {
                        latitude = bboxArray[0, 1].ToString(),
                        longitude = bboxArray[0, 0].ToString()
                    },
                    UR = new UR
                    {
                        latitude = bboxArray[1, 1].ToString(),
                        longitude = bboxArray[1, 0].ToString()
                    },
                    LR = new LR
                    {
                        latitude = bboxArray[2, 1].ToString(),
                        longitude = bboxArray[2, 0].ToString()
                    },
                    LL = new LL
                    {
                        latitude = bboxArray[3, 1].ToString(),
                        longitude = bboxArray[3, 0].ToString()
                    }
                },

                // SUN_AZIMUTH  SUN_ELEVATION
                sunPosition = new prdfImageSunPosition {
                    elevation = SUN_ELEVATION,
                    sunAzimuth = SUN_AZIMUTH
                },

                // OFF_NADIR
                offNadirAngle = OFF_NADIR,
                
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

            // EPSG
            metadata.image.epsg = EPSG;


            
            // PROCESSING_LEVEL_DESCRIPTION
            // PROJECTION
            // MODE
            // PRODUCT_URL
            // AOI
            // FORMAT
            // AGENCY
            
            return metadata;
        }


        private static double[,] FindBoundingBox(string[] singleCoordinates) {
            // Assumes bounding box has only 4 unique points
            // position (upper/lower and left/right) is not considered
            // (unimportant for geometry generation)
            if (singleCoordinates == null)
                throw new ArgumentNullException(nameof(singleCoordinates));
            if (singleCoordinates.Length < 8 || singleCoordinates.Length % 2 != 0)
                throw new ArgumentException("Invalid number of coordinates.");

            int len = singleCoordinates.Length / 2;
            if (len == 5 && singleCoordinates[0] == singleCoordinates[8] && singleCoordinates[1] == singleCoordinates[9])
            {
                len = 4;
            }

            double[,] coordinates = new double[len, 2];
            for (int i = 0; i < len; i++)
            {
                // Latitude comes before longitude in original string -> invert
                coordinates[i, 0] = Double.Parse(singleCoordinates[2 * i + 1]);
                coordinates[i, 1] = Double.Parse(singleCoordinates[2 * i]);
            }

            return coordinates;

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
                    attributeValues.Add(new band() { Value = match.Groups[1].Value} );
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