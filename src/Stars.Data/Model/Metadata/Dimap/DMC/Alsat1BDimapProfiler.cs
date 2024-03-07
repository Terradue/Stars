using System;
using System.Collections.Generic;
using Stac.Extensions.Eo;
using Stac;
using Terradue.Stars.Data.Model.Metadata.Dimap.Schemas;
using Terradue.Stars.Interface;


namespace Terradue.Stars.Data.Model.Metadata.Dimap.DMC
{
    internal class Alsat1BDimapProfiler : DmcDimapProfiler
    {

        public Alsat1BDimapProfiler(DimapDocument dimap) : base(dimap)
        {
        }

        public Alsat1BDimapProfiler(IEnumerable<DimapDocument> dimaps) : base(dimaps)
        {
        }

        protected override EoBandObject GetEoBandObject(Schemas.t_Spectral_Band_Info bandInfo, string description)
        {
            var eoBandObject = base.GetEoBandObject(bandInfo, description);
            ////
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
            ////

            return eoBandObject;
        }

        internal override string GetMission()
        {
            return "Alsat-1B";
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
            StacProvider provider = new StacProvider("DMC/ASAL", new StacProviderRole[] { StacProviderRole.producer, StacProviderRole.processor, StacProviderRole.licensor });
            provider.Description = "AlSat-1B is based on the SSTL-100 platform, hosting a 24m multispectral imager and a 12m panchromatic imager. The satellite is used for agricultural and disaster monitoring.";
            provider.Uri = new Uri("https://asal.dz/?page_id=76");
            return provider;
        }
    }
}