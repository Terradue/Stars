using System.Collections.Generic;
using Stac.Extensions.Eo;
using Terradue.Stars.Data.Model.Metadata.Airbus.Schemas;

namespace Terradue.Stars.Data.Model.Metadata.Airbus
{
    internal class SpotDimapProfiler : AirbusProfiler
    {
        public SpotDimapProfiler(Dimap_Document dimap) : base(dimap)
        {
        }

        internal override string GetConstellation()
        {
            return "spot";
        }

        internal override string GetMission()
        {
            return base.GetMission();
        }

        public override string GetPlatformInternationalDesignator()
        {
            switch (GetMission().ToLower())
            {
                case "spot-6":
                    return "2012-047A";
                case "spot-7":
                    return "2014-034A";
            }
            return null;
        }

        protected override IDictionary<EoBandCommonName?, int> BandOrders
        {
            get
            {
                Dictionary<EoBandCommonName?, int> bandOrders = new Dictionary<EoBandCommonName?, int>();
                bandOrders.Add(EoBandCommonName.red, 1);
                bandOrders.Add(EoBandCommonName.green, 2);
                bandOrders.Add(EoBandCommonName.blue, 3);
                bandOrders.Add(EoBandCommonName.nir, 4);
                return bandOrders;

            }
        }
    }
}