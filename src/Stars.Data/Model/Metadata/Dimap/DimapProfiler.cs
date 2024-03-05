using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Stac;
using Stac.Extensions.Eo;
using Stac;
using Terradue.Stars.Interface;
using Stac.Extensions.Raster;
using Humanizer;

namespace Terradue.Stars.Data.Model.Metadata.Dimap
{
    public abstract class DimapProfiler
    {
        private Schemas.DimapDocument[] dimaps;

        public Schemas.DimapDocument Dimap {
            get { return (dimaps == null || dimaps.Length == 0 ? null : dimaps[0]);}
            protected set { dimaps = new Schemas.DimapDocument[] { value }; }
        }

        public Schemas.DimapDocument[] Dimaps {
            get { return dimaps; }
            protected set { dimaps = value; }
        }

        internal Schemas.t_Source_Information GetSceneSource()
        {
            return Dimap.Dataset_Sources
                    .Where(ds => ds.Scene_Source != null)
                    .FirstOrDefault(ds => !string.IsNullOrEmpty(ds.Scene_Source.MISSION));
        }

        internal Schemas.t_Source_Information[] GetSceneSources()
        {
            List<Schemas.t_Source_Information> sceneSources = new List<Schemas.t_Source_Information>();
            foreach (Schemas.DimapDocument dimap in Dimaps)
            {
                foreach (Schemas.t_Source_Information ds in dimap.Dataset_Sources)
                {
                    if (ds.Scene_Source != null && !String.IsNullOrEmpty(ds.Scene_Source.MISSION))
                    {
                        sceneSources.Add(ds);
                    }
                }
            }
            return sceneSources.ToArray();
        }

        internal virtual string GetPlatform()
        {
            return GetMission();
        }

        internal virtual string GetMission()
        {
            var ss = GetSceneSources();
            if (ss == null || ss.Length == 0) return null;
            var mission = String.Format(
                "{0}{1}",
                ss[0].Scene_Source.MISSION,
                String.IsNullOrEmpty(ss[0].Scene_Source.MISSION_INDEX) ? String.Empty : "-" + ss[0].Scene_Source.MISSION_INDEX
            );
            return mission.Replace(" ", "-");
        }

        internal virtual DateTime GetStartTime()
        {
            var ss = GetSceneSources();
            if (ss == null || ss.Length == 0) return DateTime.MinValue;
            DateTime dt = DateTime.MaxValue;
            foreach (Schemas.t_Source_Information s in ss)
            {
                if (s.Scene_Source.IMAGING_START_TIME < dt) dt = s.Scene_Source.IMAGING_START_TIME;
            }
            return dt;
        }

        internal virtual DateTime GetEndTime()
        {
            var ss = GetSceneSources();
            if (ss == null || ss.Length == 0) return DateTime.MinValue;
            DateTime dt = DateTime.MinValue;
            foreach (Schemas.t_Source_Information s in ss)
            {
                if (s.Scene_Source.IMAGING_STOP_TIME > dt) dt = s.Scene_Source.IMAGING_STOP_TIME;
            }
            return dt;
        }

        internal virtual DateTime GetAcquisitionTime()
        {
            var ss = GetSceneSources();
            if (ss == null || ss.Length == 0) return DateTime.MinValue;
            DateTime dt = DateTime.MinValue;
            DateTime st = DateTime.MaxValue;
            foreach (Schemas.t_Source_Information s in ss)
            {
                if (s.Scene_Source.IMAGING_DATE > dt) dt = s.Scene_Source.IMAGING_DATE;
                if (s.Scene_Source.IMAGING_START_TIME < st) st = s.Scene_Source.IMAGING_START_TIME;
            }
            if  (dt == DateTime.MinValue) dt = st;
            return dt;
        }

        internal virtual DateTime GetProcessingTime()
        {
            DateTime createdDate = DateTime.MinValue;
            foreach (Schemas.DimapDocument dimap in Dimaps)
            {
                if (DateTime.TryParse(dimap.Production.DATASET_PRODUCTION_DATE, null, DateTimeStyles.AssumeUniversal, out DateTime dt))
                {
                    if (dt > createdDate) createdDate = dt;
                }
            }
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
            var ss = GetSceneSources();
            if (ss == null || ss.Length == 0) return null;
            return new string[] {
                String.Format("{0}{1}",
                    ss[0].Scene_Source.INSTRUMENT,
                    String.IsNullOrEmpty(ss[0].Scene_Source.INSTRUMENT_INDEX) ? String.Empty : "-" + ss[0].Scene_Source.INSTRUMENT_INDEX
                )
            };
        }

        public virtual string GetSpectralProcessing(Schemas.DimapDocument dimap = null)
        {
            return null;
        }

        internal int? GetAbsoluteOrbit()
        {
            var ss = GetSceneSources();
            if (ss == null || ss.Length == 0 || ss[0].Scene_Source.GRID_REFERENCE == null) return null;
            string[] refs = ss[0].Scene_Source.GRID_REFERENCE.Split('/');
            if (Int32.TryParse(refs[0], out int orbit)) return orbit;
            return null;
        }

        internal int? GetRelativeOrbit()
        {
            var ss = GetSceneSources();
            if (ss == null || ss.Length == 0 || ss[0].Scene_Source?.GRID_REFERENCE == null) return null;
            string[] refs = ss[0].Scene_Source.GRID_REFERENCE.Split('/');
            if (refs.Length >= 2 && Int32.TryParse(refs[0], out int orbit)) return orbit;
            return null;
        }

        internal abstract string GetOrbitState();

        public virtual string GetPlatformInternationalDesignator()
        {
            return null;
        }

        internal void AddProcessingSoftware(IDictionary<string, string> software)
        {
            try
            {
                var procParam = Dimap.Data_Processing.Processing_Parameter.FirstOrDefault(pp => pp.PROC_PARAMETER_DESC == "SOFTWARE");
                if (procParam != null)
                {
                    software.Add(procParam.PROC_PARAMETER_VALUE.Split(' ')[0], procParam.PROC_PARAMETER_VALUE.Split(' ')[1]);
                }
                else if (Dimap.Data_Processing.SOFTWARE_VERSION != null)
                {
                    software.Add(Dimap.Data_Processing.SOFTWARE_VERSION.Split(' ')[0], Dimap.Data_Processing.SOFTWARE_VERSION.Split(' ')[1]);
                }
            }
            catch { }
        }

        internal long GetProjection()
        {
            try
            {
                return Int64.Parse(Dimap.Coordinate_Reference_System.Horizontal_CS.Projection.PROJECTION_CODE.Replace("EPSG:", ""));
            }
            catch { }
            return 0;
        }

        internal int[] GetShape()
        {
            int height = 0, width = 0;
            foreach (Schemas.DimapDocument dimap in Dimaps)
            {
                if (Int32.TryParse(dimap.Raster_Dimensions.NCOLS, out int ncols) && Int32.TryParse(dimap.Raster_Dimensions.NROWS, out int nrows))
                {
                    if (height == 0 || nrows < height) height = nrows;
                    if (width == 0 || ncols < width) width = ncols;
                }
            }
            if (height != 0 && width != 0)
            {
                return new int[] { width, height };
            }

            return null;
        }

        internal double GetViewingAngle()
        {
            var ss = GetSceneSources();
            if (ss == null || ss.Length == 0) return 0;
            int count = 0;
            double sum = 0;
            foreach (Schemas.t_Source_Information s in ss)
            {
                if (s.Scene_Source?.VIEWING_ANGLE != null)
                {
                    count++;
                    sum += s.Scene_Source.VIEWING_ANGLE.Value;
                }
            }
            return count == 0 ? 0 : sum / count;
        }

        internal double GetIndidenceAngle()
        {
            var ss = GetSceneSources();
            if (ss == null || ss.Length == 0) return 0;
            int count = 0;
            double sum = 0;
            foreach (Schemas.t_Source_Information s in ss)
            {
                if (s.Scene_Source?.INCIDENCE_ANGLE != null)
                {
                    count++;
                    sum += s.Scene_Source.INCIDENCE_ANGLE.Value;
                }
            }
            return count == 0 ? 0 : sum / count;
        }

        internal double GetResolution()
        {
            var ss = GetSceneSources();
            if (ss == null || ss.Length == 0) return 0;

            double res = 0;
            foreach (Schemas.t_Source_Information s in ss)
            {
                if (s.Scene_Source?.THEORETICAL_RESOLUTION != null)
                {
                    if (res == 0 || s.Scene_Source?.THEORETICAL_RESOLUTION.Value > res) res = s.Scene_Source.THEORETICAL_RESOLUTION.Value;
                }
                else if (s.Scene_Source?.RESOLUTION != null)
                {
                    if (res == 0 || s.Scene_Source?.RESOLUTION.Value > res) res = s.Scene_Source.RESOLUTION.Value;
                }
            }
            return res;
        }

        internal double GetSunElevation()
        {
            var ss = GetSceneSources();
            if (ss == null || ss.Length == 0) return 0;
            int count = 0;
            double sum = 0;
            foreach (Schemas.t_Source_Information s in ss)
            {
                if (s.Scene_Source?.SUN_ELEVATION != null)
                {
                    count++;
                    sum += s.Scene_Source.SUN_ELEVATION.Value;
                }
            }
            return count == 0 ? 0 : sum / count;
        }

        internal double GetSunAngle()
        {
            var ss = GetSceneSources();
            if (ss == null || ss.Length == 0) return 0;
            int count = 0;
            double sum = 0;
            foreach (Schemas.t_Source_Information s in ss)
            {
                if (s.Scene_Source?.SUN_AZIMUTH != null)
                {
                    count++;
                    sum += s.Scene_Source.SUN_AZIMUTH.Value;
                }
            }
            return count == 0 ? 0 : sum / count;
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

        internal virtual string GetAssetTitle(IAsset bandAsset, Schemas.t_Data_File dataFile, Schemas.DimapDocument dimap = null)
        {
            string spectralProcessing = null;
            if (dimap != null) spectralProcessing = GetSpectralProcessing(dimap);
            if (spectralProcessing == null) spectralProcessing = Dimap.Data_Processing.SPECTRAL_PROCESSING?.ToLower().Titleize();
            string title = string.Format("{0} {1} {2} {3}",
                GetProductKey(bandAsset, dataFile).ToLower().Titleize(),
                Dimap.Data_Processing.RADIOMETRIC_PROCESSING.ToLower().Titleize(),
                Dimap.Data_Processing.GEOMETRIC_PROCESSING?.ToLower().Titleize(),
                spectralProcessing
            );
            return title;
        }

        internal virtual string GetAssetSuffix(Schemas.DimapDocument dimap, IAsset metadataAsset = null)
        {
            return String.Empty;
        }

        internal virtual StacProvider GetStacProvider()
        {
            return null;
        }
    }
}