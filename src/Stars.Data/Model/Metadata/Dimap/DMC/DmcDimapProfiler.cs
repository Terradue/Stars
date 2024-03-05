using System;
using System.Collections.Generic;
using System.Globalization;
using Stac;
using Stac.Extensions.Eo;
using Terradue.Stars.Data.Model.Metadata.Dimap.Schemas;
using Terradue.Stars.Interface;
using Stac.Extensions.Raster;

namespace Terradue.Stars.Data.Model.Metadata.Dimap.DMC
{
    internal class DmcDimapProfiler : GenericDimapProfiler
    {

        public DmcDimapProfiler(DimapDocument dimap) : base(dimap)
        {
        }

        public DmcDimapProfiler(IEnumerable<DimapDocument> dimaps) : base(dimaps)
        {
        }

        protected override RasterBand GetRasterBandObject(Schemas.t_Spectral_Band_Info bandInfo, Schemas.t_Raster_Encoding raster_Encoding)
        {
            RasterBand rasterBand = base.GetRasterBandObject(bandInfo, raster_Encoding);
            if (bandInfo.PHYSICAL_GAINSpecified)
            {
                if (GetInstruments()[0].ToLower().StartsWith("slim-6-22") && (GetProcessingLevel() == "L1T" || GetProcessingLevel() == "L1R"))
                    rasterBand.Scale = 1 / bandInfo.PHYSICAL_GAIN;
            }
            return rasterBand;
        }

        protected override EoBandObject GetEoBandObject(Schemas.t_Spectral_Band_Info bandInfo, string description)
        {
            EoBandObject eoBandObject = base.GetEoBandObject(bandInfo, description);

            if (GetInstruments()[0].ToLower().StartsWith("slim-6-22"))
            {
                //////
                if (bandInfo.BAND_DESCRIPTION.ToUpper() == "BLUE")
                {
                    eoBandObject.SolarIllumination = 2002.25;
                }
                if (bandInfo.BAND_DESCRIPTION.ToUpper() == "NIR")
                {
                    eoBandObject.SolarIllumination = 1032;
                }
                if (bandInfo.BAND_DESCRIPTION.ToUpper() == "RED")
                {
                    eoBandObject.SolarIllumination = 1537;
                }
                if (bandInfo.BAND_DESCRIPTION.ToUpper() == "GREEN")
                {
                    eoBandObject.SolarIllumination = 1808;
                }
                //////
            }

            if (GetInstruments()[0].ToLower().StartsWith("alite"))
            {
                //////
                if (bandInfo.BAND_DESCRIPTION.ToUpper() == "BLUE")
                {
                    eoBandObject.SolarIllumination = 1982.32;
                }
                if (bandInfo.BAND_DESCRIPTION.ToUpper() == "NIR")
                {
                    eoBandObject.SolarIllumination = 991.02;
                }
                if (bandInfo.BAND_DESCRIPTION.ToUpper() == "RED")
                {
                    eoBandObject.SolarIllumination = 1538.90;
                }
                if (bandInfo.BAND_DESCRIPTION.ToUpper() == "GREEN")
                {
                    eoBandObject.SolarIllumination = 1822.45;
                }
                if (bandInfo.BAND_DESCRIPTION.ToUpper() == "PAN")
                {
                    eoBandObject.SolarIllumination = 0.0;
                }
                //////
            }

            return eoBandObject;
        }

        /// <summary>
        /// https://directory.eoportal.org/web/eoportal/satellite-missions/u/uk-dmc-2
        /// </summary>
        /// <returns></returns>
        internal override string GetConstellation()
        {
            return "dmc";
        }

        internal override string GetSensorMode()
        {
            return "optical";
        }

        internal override StacProvider GetStacProvider()
        {
            string platform = GetPlatform().ToLower();
            if (platform.Substring(0, 6) == "uk-dmc")
            {
                StacProvider provider = new StacProvider("DMC", new StacProviderRole[] { StacProviderRole.producer, StacProviderRole.processor, StacProviderRole.licensor });
                provider.Description = "UK-DMC2 is based on the SSTL-100 satellite platform. It provides high resolution (22m) imagery from a sun-synchronous orbit over very large areas (650km swath, 2000km along track), with a daily revisit to global targets.";
                provider.Uri = new Uri("https://www.eoportal.org/satellite-missions/uk-dmc-2");
                return provider;
            }
            else if (platform == "alsat-1b")
            {
                return (new Alsat1BDimapProfiler(null as DimapDocument)).GetStacProvider();
            }
            return null;
        }
    }
}