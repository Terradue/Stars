using System;
using System.Collections.Generic;
using Stac;
using Stac.Extensions.Eo;
using Terradue.Stars.Data.Model.Metadata.Airbus.Schemas;

namespace Terradue.Stars.Data.Model.Metadata.Airbus
{
    internal class PleiadesNEODimapProfiler : AirbusProfiler
    {
        public PleiadesNEODimapProfiler(Dimap_Document dimap) : base(dimap)
        {
        }

        internal override string GetConstellation()
        {
            return "pleiades";
        }

        internal override string GetMission()
        {
            return base.GetMission().Replace("PNEO", "pleiades-neo");
        }

        protected override IDictionary<EoBandCommonName?, int> BandOrders
        {
            get

            //TODO PAN assets ? 
            {   //MS-FS assets
                Dictionary<EoBandCommonName?, int> bandOrders = new Dictionary<EoBandCommonName?, int>();
                bandOrders.Add(EoBandCommonName.blue, 0);
                bandOrders.Add(EoBandCommonName.green, 1);
                bandOrders.Add(EoBandCommonName.red, 2);
                bandOrders.Add(EoBandCommonName.coastal, 3);
                bandOrders.Add(EoBandCommonName.rededge, 4);
                bandOrders.Add(EoBandCommonName.nir, 5);
                return bandOrders;
            }
        }

        public override string GetPlatformInternationalDesignator()
        {
            //string mission = GetMission().ToLower();
            //switch (mission)
            //{
                //TODO check rational to distinguish the mission vs designator
                //case "pleiades-3":
                //    return "2021-034A";
                //case "pleiades-4":
                    return "2021-073E";
            //}
        }

        internal override StacProvider GetStacProvider()
        {
            StacProvider provider = new StacProvider("Airbus", new StacProviderRole[] { StacProviderRole.producer, StacProviderRole.processor, StacProviderRole.licensor });
            provider.Description = "Pléiades Neo is the most advanced optical constellation of Airbus, with two identical 30 cm resolution satellites and optimum reactivity. Pleiades Neo allows users to unleash the potential of geospatial applications and analytics.";
            //TODO check Uri vs gdoc url
            provider.Uri = new Uri("https://www.intelligence-airbusds.com/imagery/constellation/pleiades-neo/");
            //TODO set "propietary" for field license here ? 
            return provider;
        }

        public override string[] GetInstruments()
        {
            return new string[1] { "pneo-imager"};
        }

        //TODO override CompleteAsset
    }
}