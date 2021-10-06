using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GeoJSON.Net.Geometry;
using Stac;
using Stac.Extensions.Eo;
using Stac.Extensions.Sar;
using Stac;
using Terradue.OpenSearch.Sentinel.Data.Safe;
using Terradue.Stars.Geometry.Gml321;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;

namespace Terradue.Stars.Data.Model.Metadata.Sentinels.Sentinel2
{
    public class S2SafeStacFactory : SentinelSafeStacFactory
    {

        public S2SafeStacFactory(XFDUType xfdu, IItem route, string identifier) : base(xfdu, route, identifier)
        {
        }

        public static S2SafeStacFactory Create(XFDUType xfdu, IItem route, string identifier)
        {
            return new S2SafeStacFactory(xfdu, route, identifier);
        }

        internal override StacItem CreateStacItem()
        {
            StacItem stacItem = base.CreateStacItem();

            AddEoStacExtension(stacItem);

            return stacItem;
        }

        private void AddEoStacExtension(StacItem stacItem)
        {
        }

        private SarCommonFrequencyBandName GetFrequencyBand() => SarCommonFrequencyBandName.C;

        public override double GetGroundSampleDistance()
        {
            return 10;
        }

        public string GetInstrumentMode() => "obs";

        public override string GetProductType()
        {
            switch (Identifier.Substring(4, 6))
            {
                case "MSIL1C":
                    return "Level-1C";
                case "MSIL2A":
                    return "Level-2A";
                default:
                    return "UNKNOWN";
            }
        }

        protected override string GetProcessingLevel()
        {
            switch (Identifier.Substring(4, 6))
            {
                case "MSIL1C":
                    return "L1C";
                case "MSIL2A":
                    return "L2A";
                default:
                    return null;
            }
        }

        protected override string GetTitle(IDictionary<string, object> properties)
        {
            return string .Format("{0} {1} {2}", StylePlatform(properties.GetProperty<string>("platform")),
                                                  properties.GetProperty<string[]>("instruments").First().ToUpper(),
                                                  properties.GetProperty<string>("processing:level").ToUpper());
        }
    }
}