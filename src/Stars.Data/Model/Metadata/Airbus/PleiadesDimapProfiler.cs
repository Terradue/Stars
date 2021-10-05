using System.Collections.Generic;
using Stac.Extensions.Eo;
using Terradue.Stars.Data.Model.Metadata.Airbus.Schemas;

namespace Terradue.Stars.Data.Model.Metadata.Airbus
{
    internal class PleiadesDimapProfiler : AirbusProfiler
    {
        public PleiadesDimapProfiler(Dimap_Document dimap) : base(dimap)
        {
        }

        internal override string GetConstellation()
        {
            return "pleiades";
        }

        internal override string GetMission()
        {
            return base.GetMission().Replace("PHR", "Pleiades");
        }

        protected override IDictionary<EoBandCommonName?, int> BandOrders
        {
            get
            {
                Dictionary<EoBandCommonName?, int> bandOrders = new Dictionary<EoBandCommonName?, int>();
                bandOrders.Add(EoBandCommonName.pan, 0);
                bandOrders.Add(EoBandCommonName.red, 1);
                bandOrders.Add(EoBandCommonName.green, 2);
                bandOrders.Add(EoBandCommonName.blue, 3);
                bandOrders.Add(EoBandCommonName.nir, 4);
                return bandOrders;
            }
        }

        public override string GetPlatformInternationalDesignator()
        {
            string mission = GetMission().ToLower();
            switch (mission)
            {
                case "pleiades-1a":
                    return "2011-076F";
                case "pleiades-1b":
                    return "2012-068A";
            }
            return null;
        }

    }
}