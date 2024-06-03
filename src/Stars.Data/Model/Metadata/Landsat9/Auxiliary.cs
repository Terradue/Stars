using System;
using System.Collections.Generic;
using System.Linq;
using Humanizer;
using Kajabity.Tools.Java;

namespace Terradue.Stars.Data.Model.Metadata.Landsat9
{
    public class Auxiliary : JavaProperties
    {
        public new string GetProperty(string key)
        {
            var r = base.GetProperty(key);
            if (!string.IsNullOrEmpty(r)) r = r.Trim('"');
            return r;
        }

        public bool IsLandsat9()
        {
            return SpaceCraftID == "LANDSAT_9";
        }
        //properties
        public string SpaceCraftID => GetProperty("SPACECRAFT_ID");
        public string SceneId => GetProperty("LANDSAT_SCENE_ID");
        public string CreationDateTime => GetProperty("FILE_DATE") ?? GetProperty("DATE_PRODUCT_GENERATED");
        public string AcquisitionDateTime => GetProperty("DATE_ACQUIRED") + "T" + GetProperty("SCENE_CENTER_TIME");

        //platform
        public string[] Instruments => GetProperty("SENSOR_ID").ToLower().Split('_');
        public string CollectionNumber => GetProperty("COLLECTION_NUMBER");

        public string Platform
        {
            get
            {
                return SpaceCraftID.ToLower().Hyphenate();
            }
        }
        public double GSD
        {
            get
            {
                var cells = new List<double>{
                    double.Parse(GetProperty("GRID_CELL_SIZE_PANCHROMATIC")),
                    double.Parse(GetProperty("GRID_CELL_SIZE_REFLECTIVE")),
                    double.Parse(GetProperty("GRID_CELL_SIZE_THERMAL"))
                };
                return cells.Min();
            }
        }

        // projection
        public string MapProjection => GetProperty("MAP_PROJECTION");
        public string Orientation => GetProperty("ORIENTATION");
        public int UTM_zone => int.Parse(GetProperty("UTM_ZONE"));

        // orbit
        public int OrbitNumber => int.Parse(GetProperty("WRS_PATH"));
        public int Row => int.Parse(GetProperty("WRS_ROW"));
        public int Path => int.Parse(GetProperty("WRS_PATH"));
        public string OrbitDirection => Row <= 122 ? "descending" : "ascending";

        // images attributes
        public double SunAzimuth => double.Parse(GetProperty("SUN_AZIMUTH"));
        public double SunElevation => double.Parse(GetProperty("SUN_ELEVATION"));
        public readonly double OffNadirAngle = 0;
        internal string ProductId => GetProperty("LANDSAT_PRODUCT_ID") ?? GetProperty("METADATA_FILE_NAME").Replace("_MTL.txt", "");

        public int Panchromatic_Lines => int.Parse(GetProperty("PANCHROMATIC_LINES"));
        public int Panchromatic_Samples => int.Parse(GetProperty("PANCHROMATIC_SAMPLES"));
        public int Reflective_Lines => int.Parse(GetProperty("REFLECTIVE_LINES"));
        public int Reflective_Samples => int.Parse(GetProperty("REFLECTIVE_SAMPLES"));
        public int Thermal_Lines => int.Parse(GetProperty("THERMAL_LINES"));
        public int Thermal_Samples => int.Parse(GetProperty("THERMAL_SAMPLES"));

        public string CollectionCategory => GetProperty("COLLECTION_CATEGORY");

        public double IncidenceAngle
        {
            get
            {
                switch (GetProperty("NADIR_OFFNADIR"))
                {
                    case "NADIR":
                        return 0;
                    case "OFF_NADIR":
                        return double.Parse(GetProperty("ROLL_ANGLE"));
                    default:
                        return 0;
                }
            }
        }

        // EO
        public double CloudCover
        {
            get
            {
                try
                {
                    return double.Parse(GetProperty("CLOUD_COVER"));
                }
                catch (Exception)
                {
                    return 0;
                };
            }
        }

        public double CloudCoverLand
        {
            get
            {
                try
                {
                    return double.Parse(GetProperty("CLOUD_COVER_LAND"));
                }
                catch (Exception)
                {
                    return 0;
                };
            }
        }

        // processing
        public string DataType => GetProperty("DATA_TYPE");

        //Geometry
        public double LowerLeftCoordinateLat => double.Parse(GetProperty("CORNER_LL_LAT_PRODUCT"));
        public double LowerLeftCoordinateLon => double.Parse(GetProperty("CORNER_LL_LON_PRODUCT"));
        public double LowerRightCoordinateLat => double.Parse(GetProperty("CORNER_LR_LAT_PRODUCT"));
        public double LowerRightCoordinateLon => double.Parse(GetProperty("CORNER_LR_LON_PRODUCT"));
        public double UpperLeftCoordinateLat => double.Parse(GetProperty("CORNER_UL_LAT_PRODUCT"));
        public double UpperLeftCoordinateLon => double.Parse(GetProperty("CORNER_UL_LON_PRODUCT"));
        public double UpperRightCoordinateLat => double.Parse(GetProperty("CORNER_UR_LAT_PRODUCT"));
        public double UpperRightCoordinateLon => double.Parse(GetProperty("CORNER_UR_LON_PRODUCT"));

        // Band
        public string Filename_Band1 => GetProperty("FILE_NAME_BAND_1");
        public string Filename_Band2 => GetProperty("FILE_NAME_BAND_2");
        public string Filename_Band3 => GetProperty("FILE_NAME_BAND_3");
        public string Filename_Band4 => GetProperty("FILE_NAME_BAND_4");
        public string Filename_Band5 => GetProperty("FILE_NAME_BAND_5");
        public string Filename_Band6 => GetProperty("FILE_NAME_BAND_6");
        public string Filename_Band7 => GetProperty("FILE_NAME_BAND_7");
        public string Filename_Band8 => GetProperty("FILE_NAME_BAND_8");
        public string Filename_Band9 => GetProperty("FILE_NAME_BAND_9");
        public string Filename_Band10 => GetProperty("FILE_NAME_BAND_10");
        public string Filename_Band11 => GetProperty("FILE_NAME_BAND_11");

    }

}
