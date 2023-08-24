// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: DimapProfiler.cs

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Humanizer;
using Stac;
using Stac.Extensions.Eo;
using Stac.Extensions.Raster;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Data.Model.Metadata.Dimap
{
    public abstract class DimapProfiler
    {
        public Schemas.t_Dimap_Document Dimap { get; protected set; }

        internal Schemas.t_Source_Information GetSceneSource()
        {
            return Dimap.Dataset_Sources
                    .Where(ds => ds.Scene_Source != null)
                    .FirstOrDefault(ds => !string.IsNullOrEmpty(ds.Scene_Source.MISSION));
        }

        internal virtual string GetPlatform()
        {
            return GetMission();
        }

        internal virtual string GetMission()
        {
            var ss = GetSceneSource();
            if (ss == null) return null;
            var mission = ss.Scene_Source.MISSION + (string.IsNullOrEmpty(ss.Scene_Source.MISSION_INDEX) ? "" : "-" + ss.Scene_Source.MISSION_INDEX);
            return mission.Replace(" ", "-");
        }

        internal virtual DateTime GetStartTime()
        {
            var ss = GetSceneSource();
            if (ss == null) return DateTime.MinValue;
            return ss.Scene_Source.IMAGING_START_TIME;
        }

        internal virtual DateTime GetEndTime()
        {
            var ss = GetSceneSource();
            if (ss == null) return DateTime.MinValue;
            return ss.Scene_Source.IMAGING_STOP_TIME;
        }

        internal virtual DateTime GetAcquisitionTime()
        {
            var ss = GetSceneSource();
            if (ss == null) return DateTime.MinValue;
            return ss.Scene_Source.IMAGING_DATE;
        }

        internal virtual DateTime GetProcessingTime()
        {
            DateTime createdDate = DateTime.MinValue;
            DateTime.TryParse(Dimap.Production.DATASET_PRODUCTION_DATE, null, DateTimeStyles.AssumeUniversal, out createdDate);
            return createdDate.ToUniversalTime();
        }

        internal virtual string GetProductType()
        {
            return Dimap.Production?.PRODUCT_TYPE;
        }

        internal virtual string GetProcessingLevel()
        {
            return Dimap.Production?.PRODUCT_TYPE;
        }

        public virtual string GetTitle(IDictionary<string, object> properties)
        {
            CultureInfo culture = new CultureInfo("fr-FR");
            return string.Format("{0} {1} {2} {3}",
                                                  GetPlatform(),
                                                  string.Join("/", GetInstruments()),
                                                  GetProcessingLevel(),
                                                  properties.GetProperty<DateTime>("datetime").ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss", culture));
        }

        public virtual string[] GetInstruments()
        {
            var ss = GetSceneSource();
            if (ss == null) return null;
            return new string[1] { ss.Scene_Source.INSTRUMENT + (string.IsNullOrEmpty(ss.Scene_Source.INSTRUMENT_INDEX) ? "" : "-" + ss.Scene_Source.INSTRUMENT_INDEX) };
        }

        internal int? GetAbsoluteOrbit()
        {
            try
            {
                var ss = GetSceneSource();
                return int.Parse(ss.Scene_Source.GRID_REFERENCE.Split('/')[0]);
            }
            catch
            {
                return null;
            }
        }

        internal int? GetRelativeOrbit()
        {
            try
            {
                var ss = GetSceneSource();
                return int.Parse(ss.Scene_Source.GRID_REFERENCE.Split('/')[1]);
            }
            catch
            {
                return null;
            }
        }

        internal abstract string GetOrbitState();

        internal void AddProcessingSoftware(IDictionary<string, string> software)
        {
            try
            {
                var procParam = Dimap.Data_Processing.Processing_Parameter.FirstOrDefault(pp => pp.PROC_PARAMETER_DESC == "SOFTWARE");
                if (procParam != null)
                    software.Add(procParam.PROC_PARAMETER_VALUE.Split(' ')[0], procParam.PROC_PARAMETER_VALUE.Split(' ')[1]);
            }
            catch { }
        }

        internal long GetProjection()
        {
            try
            {
                return long.Parse(Dimap.Coordinate_Reference_System.Horizontal_CS.Projection.PROJECTION_CODE.Replace("EPSG:", ""));
            }
            catch { }
            return 0;
        }

        internal int[] GetShape()
        {
            try
            {
                return new int[] { int.Parse(Dimap.Raster_Dimensions.NCOLS), int.Parse(Dimap.Raster_Dimensions.NROWS) };
            }
            catch
            {
                return null;
            }
        }

        internal double GetViewingAngle()
        {
            try
            {
                var ss = GetSceneSource();
                return ss.Scene_Source.VIEWING_ANGLE.Value;
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
                var ss = GetSceneSource();
                return ss.Scene_Source.INCIDENCE_ANGLE.Value;
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
                var ss = GetSceneSource();
                return ss.Scene_Source.THEORETICAL_RESOLUTION.Value;
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
                var ss = GetSceneSource();
                return ss.Scene_Source.SUN_ELEVATION.Value;
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
                var ss = GetSceneSource();
                return ss.Scene_Source.SUN_AZIMUTH.Value;
            }
            catch
            {
                return 0;
            }
        }

        public abstract string GetProductKey(IAsset bandAsset, Schemas.t_Data_File dataFile);

        internal void CompleteAsset(StacAsset stacAsset, Schemas.t_Spectral_Band_Info[] spectralBandInfos, Schemas.t_Raster_Encoding raster_Encoding)
        {
            List<EoBandObject> eoBandObjects = GetEoBandObjects(spectralBandInfos);
            if (eoBandObjects.Count > 0)
                stacAsset.EoExtension().Bands = eoBandObjects.ToArray();
            List<RasterBand> rasterBandObjects = GetRasterBandObjects(spectralBandInfos, raster_Encoding);
            if (rasterBandObjects.Count > 0)
                stacAsset.RasterExtension().Bands = rasterBandObjects.ToArray();
            if (stacAsset.MediaType.MediaType == "image/tiff")
                stacAsset.MediaType.Parameters.Add("application", "geotiff");
            stacAsset.Roles.Add("dn");
        }

        private List<EoBandObject> GetEoBandObjects(Schemas.t_Spectral_Band_Info[] spectralBandInfos)
        {
            List<EoBandObject> eoBandObjects = new List<EoBandObject>();
            List<string> labels = new List<string>();
            if (!string.IsNullOrEmpty(Dimap.Data_Processing.GEOMETRIC_PROCESSING))
                labels.Add(Dimap.Data_Processing.GEOMETRIC_PROCESSING);
            if (!string.IsNullOrEmpty(Dimap.Data_Processing.RADIOMETRIC_PROCESSING))
                labels.Add(Dimap.Data_Processing.RADIOMETRIC_PROCESSING);
            if (!string.IsNullOrEmpty(Dimap.Data_Processing.SPECTRAL_PROCESSING))
                labels.Add(Dimap.Data_Processing.SPECTRAL_PROCESSING);
            if (!string.IsNullOrEmpty(Dimap.Data_Processing.THEMATIC_PROCESSING))
                labels.Add(Dimap.Data_Processing.THEMATIC_PROCESSING);
            foreach (var bandInfo in spectralBandInfos)
            {
                switch (bandInfo.PHYSICAL_UNIT)
                {
                    case "W/m2/sr/m-6":
                        eoBandObjects.Add(GetEoBandObject(bandInfo, string.Join(" ", labels)));
                        break;
                }
            }
            return eoBandObjects;
        }

        internal abstract string GetSensorMode();

        private List<RasterBand> GetRasterBandObjects(Schemas.t_Spectral_Band_Info[] spectralBandInfos, Schemas.t_Raster_Encoding raster_Encoding)
        {
            List<RasterBand> rasterBandObjects = new List<RasterBand>();
            foreach (var bandInfo in spectralBandInfos)
            {
                rasterBandObjects.Add(GetRasterBandObject(bandInfo, raster_Encoding));
            }
            return rasterBandObjects;
        }

        protected abstract EoBandObject GetEoBandObject(Schemas.t_Spectral_Band_Info bandInfo, string description);

        protected abstract RasterBand GetRasterBandObject(Schemas.t_Spectral_Band_Info bandInfo, Schemas.t_Raster_Encoding raster_Encoding);

        internal virtual string GetConstellation()
        {
            return GetMission();
        }

        internal virtual string GetAssetTitle(IAsset bandAsset, Schemas.t_Data_File dataFile)
        {
            string title = string.Format("{0} {1} {2} {3}",
                GetProductKey(bandAsset, dataFile).ToLower().Titleize(),
                Dimap.Data_Processing.RADIOMETRIC_PROCESSING.ToLower().Titleize(),
                Dimap.Data_Processing.GEOMETRIC_PROCESSING?.ToLower().Titleize(),
                Dimap.Data_Processing.SPECTRAL_PROCESSING?.ToLower().Titleize());
            return title;
        }

        internal virtual StacProvider GetStacProvider()
        {
            return null;
        }
    }
}
