using Stac.Extensions.Eo;
using Stac;
using Terradue.Stars.Data.Model.Metadata.Dimap.Schemas;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Data.Model.Metadata.Dimap.DMC
{
    internal class Alsat1BDimapProfiler : DmcDimapProfiler
    {

        public Alsat1BDimapProfiler(t_Dimap_Document dimap) : base(dimap)
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
    }
}