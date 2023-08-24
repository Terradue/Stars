// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: SpotDimapProfiler.cs

using System;
using System.Collections.Generic;
using Stac;
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

        internal override StacProvider GetStacProvider()
        {
            StacProvider provider = new StacProvider("Airbus", new StacProviderRole[] { StacProviderRole.producer, StacProviderRole.processor, StacProviderRole.licensor });
            provider.Description = "The identical Pléiades 1A and Pléiades 1B satellites deliver 50cm imagery products with a 20km swath. The product's location accuracy and excellent image quality make it an ideal source for data for any civil or military project.";
            provider.Uri = new Uri("https://www.intelligence-airbusds.com/imagery/constellation/pleiades/");
            return provider;
        }
    }
}
