using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Humanizer;
using Stac;
using Stac.Extensions.Eo;
using Stac.Extensions.Projection;
using Stac.Extensions.Raster;
using Terradue.Stars.Data.Model.Metadata.Airbus.Schemas;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Data.Model.Metadata.Airbus
{
    public abstract class AirbusProfiler
    {
        public Schemas.Dimap_Document Dimap { get; protected set; }

        public AirbusProfiler(Dimap_Document dimap)
        {
            Dimap = dimap;
        }

        internal Schemas.Source_Identification GetSourceIdentification()
        {
            return Dimap.Dataset_Sources.Source_Identification
                    .Where(ds => ds.Strip_Source != null)
                    .FirstOrDefault(ds => !string.IsNullOrEmpty(ds.Strip_Source.MISSION));
        }

        internal virtual string GetPlatform()
        {
            return GetMission();
        }

        internal virtual string GetMission()
        {
            var ss = GetSourceIdentification();
            if (ss == null) return null;
            var mission = ss.Strip_Source.MISSION
                          + (string.IsNullOrEmpty(ss.Strip_Source.MISSION_INDEX) ? "" : "-" + ss.Strip_Source.MISSION_INDEX);
            return mission.Replace(" ", "-");
        }

        internal virtual DateTime GetAcquisitionTime()
        {
            var ss = GetSourceIdentification();
            if (ss == null) return DateTime.MinValue;
            return DateTime.Parse(string.Format("{0}T{1}",
                                                ss.Strip_Source.IMAGING_DATE,
                                                ss.Strip_Source.IMAGING_TIME),
                                  null,
                                  DateTimeStyles.AssumeUniversal);
        }

        internal virtual DateTime GetProcessingTime()
        {
            DateTime createdDate = DateTime.MinValue;
            DateTime.TryParse(Dimap.Product_Information.Delivery_Identification.PRODUCTION_DATE,
                              null,
                              DateTimeStyles.AssumeUniversal,
                              out createdDate);
            return createdDate.ToUniversalTime();
        }

        internal virtual string GetProductType()
        {
            return Dimap.Dataset_Identification.DATASET_TYPE;
        }

        internal virtual string GetProcessingLevel()
        {
            return Dimap.Processing_Information.Product_Settings.PROCESSING_LEVEL
                    + "/" + Dimap.Processing_Information.Product_Settings.Radiometric_Settings.RADIOMETRIC_PROCESSING;
        }

        public virtual string GetTitle(IDictionary<string, object> properties)
        {
            CultureInfo culture = new CultureInfo("fr-FR");
            return string.Format("{0} {1} {2} {3}",
                                                  GetPlatform().ToUpper(),
                                                  string.Join("/", GetInstruments()),
                                                  GetProductType(),
                                                  properties.GetProperty<DateTime>("datetime").ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss", culture));
        }

        public virtual string[] GetInstruments()
        {
            var ss = GetSourceIdentification();
            if (ss == null) return null;
            return new string[1] { ss.Strip_Source.INSTRUMENT + (string.IsNullOrEmpty(ss.Strip_Source.INSTRUMENT_INDEX) ? "" : "-" + ss.Strip_Source.INSTRUMENT_INDEX) };
        }

        internal int? GetAbsoluteOrbit()
        {
            return null;
        }

        internal int? GetRelativeOrbit()
        {
            return null;
        }

        internal void AddProcessingSoftware(IDictionary<string, string> software)
        {
            try
            {
                software.Add(Dimap.Processing_Information.Production_Facility.SOFTWARE.Text,
                             Dimap.Processing_Information.Production_Facility.SOFTWARE.Version);
            }
            catch { }
        }

        internal int GetEpsgProjectionCode()
        {
            try
            {
                if (Dimap.Coordinate_Reference_System.Projected_CRS.PROJECTED_CRS_CODE.Contains("EPSG"))
                    return int.Parse(Dimap.Coordinate_Reference_System.Projected_CRS.PROJECTED_CRS_NAME.Split(' ').First());
            }
            catch { }
            try
            {
                if (Dimap.Coordinate_Reference_System.Geodetic_CRS.CRS_TABLES.Contains("EPSG"))
                    return int.Parse(Dimap.Coordinate_Reference_System.Geodetic_CRS.GEODETIC_CRS_CODE.Replace("urn:ogc:def:crs:EPSG::", ""));
            }
            catch { }
            return 0;
        }

        internal double GetViewingAngle()
        {
            try
            {
                return double.Parse(Dimap.Geometric_Data.Use_Area.Located_Geometric_Values
                                    .FirstOrDefault(l => l.LOCATION_TYPE.Equals("center",
                                                                                StringComparison.InvariantCultureIgnoreCase))
                                                                                .Acquisition_Angles.AZIMUTH_ANGLE);
            }
            catch
            {
                return 0;
            }
        }

        internal double GetIndidenceAngle()
        {
            try
            {
                return double.Parse(Dimap.Geometric_Data.Use_Area.Located_Geometric_Values
                                    .FirstOrDefault(l => l.LOCATION_TYPE.Equals("center",
                                                                                StringComparison.InvariantCultureIgnoreCase))
                                                                                .Acquisition_Angles.INCIDENCE_ANGLE);
            }
            catch
            {
                return 0;
            }
        }

        internal double GetResolution()
        {
            try
            {
                return double.Parse(Dimap.Geometric_Data.Use_Area.Located_Geometric_Values
                                    .FirstOrDefault(l => l.LOCATION_TYPE.Equals("center",
                                                                                StringComparison.InvariantCultureIgnoreCase))
                                                                                .Ground_Sample_Distance.GSD_ACROSS_TRACK);
            }
            catch
            {
                return 0;
            }
        }

        internal double GetSunElevation()
        {
            try
            {
                return double.Parse(Dimap.Geometric_Data.Use_Area.Located_Geometric_Values
                                    .FirstOrDefault(l => l.LOCATION_TYPE.Equals("center",
                                                                                StringComparison.InvariantCultureIgnoreCase))
                                                                                .Solar_Incidences.SUN_ELEVATION);
            }
            catch
            {
                return 0;
            }
        }

        internal double GetSunAngle()
        {
            try
            {
                return double.Parse(Dimap.Geometric_Data.Use_Area.Located_Geometric_Values
                                    .FirstOrDefault(l => l.LOCATION_TYPE.Equals("center",
                                                                                StringComparison.InvariantCultureIgnoreCase))
                                                                                .Solar_Incidences.SUN_AZIMUTH);
            }
            catch
            {
                return 0;
            }
        }

        internal int[] GetShape()
        {
            try
            {
                return new int[] { int.Parse(Dimap.Raster_Data.Raster_Dimensions.NCOLS), int.Parse(Dimap.Raster_Data.Raster_Dimensions.NROWS) };
            }
            catch
            {
                return null;
            }
        }

        internal void CompleteAsset(StacAsset stacAsset, StacItem stacItem)
        {
            List<EoBandObject> eoBandObjects = GetEoBandObjects(Dimap.Radiometric_Data.Radiometric_Calibration.Instrument_Calibration.Band_Measurement_List,
                                                                Dimap.Processing_Information.Product_Settings.Radiometric_Settings);
            if (eoBandObjects.Count > 0)
            {
                stacAsset.EoExtension().Bands = eoBandObjects.ToArray();
            }
            List<RasterBand> rasterBandObjects = GetRasterBandObjects(Dimap.Radiometric_Data.Radiometric_Calibration.Instrument_Calibration.Band_Measurement_List,
                                                                    Dimap.Processing_Information.Product_Settings.Radiometric_Settings);

            if (rasterBandObjects.Count > 0)
                stacAsset.RasterExtension().Bands = rasterBandObjects.ToArray();
            stacAsset.Properties.Add("product_type", Dimap.Processing_Information.Product_Settings.SPECTRAL_PROCESSING);
            switch (Dimap.Processing_Information.Product_Settings.Radiometric_Settings.RADIOMETRIC_PROCESSING)
            {
                case "BASIC":
                case "LINEAR_STRETCH":
                    stacAsset.Roles.Add("dn");
                    break;
                case "REFLECTANCE":
                    stacAsset.Roles.Add("reflectance");
                    break;
                case "DISPLAY":
                case "SEAMLESS":
                    stacAsset.Roles.Add("visual");
                    break;
            }

            try
            {
                stacAsset.ProjectionExtension().Shape = new int[] { int.Parse(Dimap.Raster_Data.Raster_Dimensions.NCOLS), int.Parse(Dimap.Raster_Data.Raster_Dimensions.NROWS) };
            }
            catch
            {
            }
        }

        public abstract string GetPlatformInternationalDesignator();

        private List<EoBandObject> GetEoBandObjects(Schemas.Band_Measurement_List spectralBandInfos, Radiometric_Settings radiometric_Settings)
        {
            List<EoBandObject> eoBandObjects = new List<EoBandObject>();
            for (int i = 0; i < spectralBandInfos.Band_Radiance.Count(); i++)
            {
                var bandInfo = spectralBandInfos.Band_Radiance[i];
                var bandSolarIrradiance = spectralBandInfos.Band_Solar_Irradiance[i];
                eoBandObjects.Add(GetEoBandRadianceObject(bandInfo, bandSolarIrradiance));
            }
            return eoBandObjects.OrderBy(eob => BandOrders[eob.CommonName]).ToList();
        }

        private List<RasterBand> GetRasterBandObjects(Schemas.Band_Measurement_List spectralBandInfos, Radiometric_Settings radiometric_Settings)
        {
            List<RasterBand> rasterBandObjects = new List<RasterBand>();
            for (int i = 0; i < spectralBandInfos.Band_Radiance.Count(); i++)
            {
                var bandInfo = spectralBandInfos.Band_Radiance[i];
                var bandSolarIrradiance = spectralBandInfos.Band_Solar_Irradiance[i];
                rasterBandObjects.Add(GetRasterBandObject(bandInfo, bandSolarIrradiance, radiometric_Settings));
            }
            rasterBandObjects = rasterBandObjects.OrderBy(rb => BandOrders[rb.GetProperty<EoBandCommonName>("bcn")]).ToList();
            foreach (var rb in rasterBandObjects)
                rb.RemoveProperty("bcn");
            return rasterBandObjects;
        }

        protected abstract IDictionary<EoBandCommonName?, int> BandOrders { get; }

        protected virtual EoBandObject GetEoBandRadianceObject(Schemas.Band_Radiance bandInfo, Schemas.Band_Solar_Irradiance bandSolarIrradiance)
        {
            EoBandObject eoBandObject = new EoBandObject(bandInfo.BAND_ID,
                                                GetEoCommonName(bandInfo));
            eoBandObject.Description = bandInfo.MEASURE_DESC;
            eoBandObject.SolarIllumination = double.Parse(bandSolarIrradiance.VALUE);

            return eoBandObject;
        }

        protected virtual RasterBand GetRasterBandObject(Schemas.Band_Radiance bandInfo, Schemas.Band_Solar_Irradiance bandSolarIrradiance, Radiometric_Settings radiometric_Settings)
        {
            RasterBand rasterBand = new RasterBand();
            rasterBand.Statistics = new Stac.Common.Statistics(0, 4096, null, null, null);
            switch (radiometric_Settings.RADIOMETRIC_PROCESSING)
            {
                case "BASIC":
                    rasterBand.DataType = Stac.Common.DataType.uint16;
                    rasterBand.BitsPerSample = 12;

                    break;
                case "LINEAR_STRETCH":
                    rasterBand.DataType = Stac.Common.DataType.uint8;
                    rasterBand.Statistics.Maximum = 255;
                    break;
                case "REFLECTANCE":
                    rasterBand.DataType = Stac.Common.DataType.uint8;
                    rasterBand.Statistics.Maximum = 10000;
                    break;
                case "DISPLAY":
                case "SEAMLESS":
                    rasterBand.DataType = Stac.Common.DataType.uint8;
                    rasterBand.Statistics.Maximum = 255;
                    break;
            }
            rasterBand.Scale = 1 / double.Parse(bandInfo.GAIN);
            rasterBand.Offset = double.Parse(bandInfo.BIAS);
            rasterBand.SetProperty("bcn", GetEoCommonName(bandInfo));
            return rasterBand;
        }

        private EoBandCommonName GetEoCommonName(Band_Radiance bandInfo)
        {
            if (bandInfo.BAND_ID == "P")
                return EoBandCommonName.pan;
            if (Dimap.Raster_Data.Raster_Display.Band_Display_Order.RED_CHANNEL == bandInfo.BAND_ID)
                return EoBandCommonName.red;
            if (Dimap.Raster_Data.Raster_Display.Band_Display_Order.GREEN_CHANNEL == bandInfo.BAND_ID)
                return EoBandCommonName.green;
            if (Dimap.Raster_Data.Raster_Display.Band_Display_Order.BLUE_CHANNEL == bandInfo.BAND_ID)
                return EoBandCommonName.blue;
            if (Dimap.Raster_Data.Raster_Display.Band_Display_Order.ALPHA_CHANNEL == bandInfo.BAND_ID)
                return EoBandCommonName.nir;
            return default(EoBandCommonName);
        }

        internal virtual string GetConstellation()
        {
            return GetMission();
        }

        internal virtual string GetAssetKey(IAsset bandAsset, Data_File dataFile)
        {
            string key = Dimap.Processing_Information.Product_Settings.SPECTRAL_PROCESSING;
            key += "-R" + dataFile.Tile_R + "C" + dataFile.Tile_C;
            return key;
        }

        internal virtual string GetAssetTitle(IAsset bandAsset, Data_File dataFile)
        {
            string title = string.Format("{0} {1} {2} {3} R{4} C{5}",
                Dimap.Raster_Data.Data_Access.DATA_FILE_ORGANISATION.ToLower().Titleize(),
                Dimap.Processing_Information.Product_Settings.Radiometric_Settings.RADIOMETRIC_PROCESSING.ToLower().Titleize(),
                Dimap.Processing_Information.Product_Settings.SPECTRAL_PROCESSING,
                Dimap.Processing_Information.Product_Settings.PROCESSING_LEVEL.ToLower().Titleize(),
                dataFile.Tile_R, dataFile.Tile_C);
            return title;
        }

        internal virtual StacProvider GetStacProvider()
        {
            return null;
        }
    }
}