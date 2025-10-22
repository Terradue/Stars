// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: S2SafeStacFactory.cs

using System.Collections.Generic;
using System.Linq;
using Stac;
using Stac.Extensions.Eo;
using Stac.Extensions.Projection;
using Stac.Extensions.Sar;
using Terradue.OpenSearch.Sentinel.Data.Safe;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Data.Model.Metadata.Sentinels.Sentinel2
{
    public class S2SafeStacFactory : SentinelSafeStacFactory
    {

        private readonly OpenSearch.Sentinel.Data.Safe.Sentinel.S2.Level1.Granules.ILevel1C_Tile mtdTile;

        public S2SafeStacFactory(XFDUType xfdu, IItem route, string identifier, OpenSearch.Sentinel.Data.Safe.Sentinel.S2.Level1.Granules.ILevel1C_Tile mtdTile) : base(xfdu, route, identifier)
        {
            this.mtdTile = mtdTile;
        }

        public static S2SafeStacFactory Create(XFDUType xfdu, IItem route, string identifier, OpenSearch.Sentinel.Data.Safe.Sentinel.S2.Level1.Granules.ILevel1C_Tile mtdTile)
        {
            return new S2SafeStacFactory(xfdu, route, identifier, mtdTile);
        }

        internal override StacItem CreateStacItem()
        {
            StacItem stacItem = base.CreateStacItem();

            AddEoStacExtension(stacItem);

            return stacItem;
        }

        protected virtual void AddEoStacExtension(StacItem stacItem)
        {
            double? cloudCover = mtdTile?.Quality_Indicators_Info?.Image_Content_QI?.CLOUDY_PIXEL_PERCENTAGE.Value;
            EoStacExtension eo = stacItem.EoExtension();
            if (cloudCover != null) eo.CloudCover = cloudCover.Value;
        }

        private SarCommonFrequencyBandName GetFrequencyBand() => SarCommonFrequencyBandName.C;

        public override double GetGroundSampleDistance()
        {
            return 10;
        }

        public string GetInstrumentMode() => "obs";

        public override string GetProductType()
        {
            return Identifier.Substring(4, 6);
        }

        public override string GetDerivedProductType()
        {
            switch (GetProductType())
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
            return string.Format("{0} {1} {2} {3}",
                //StylePlatform(properties.GetProperty<string>("platform")),
                properties.GetProperty<string>("platform").ToUpper(),
                properties.GetProperty<string[]>("instruments").First().ToUpper(),
                properties.GetProperty<string>("processing:level").ToUpper(),
                properties.GetProperty<int>("sat:relative_orbit")
            );
        }

        protected override string GetSensorType(metadataObjectType platformMetadata)
        {
            return "optical";
        }

        protected override void AddProjectionStacExtension(StacItem stacItem)
        {
            if (mtdTile?.Geometric_Info?.Tile_Geocoding == null) return;
            stacItem.ProjectionExtension().Epsg = int.Parse(mtdTile.Geometric_Info.Tile_Geocoding.HORIZONTAL_CS_CODE.Replace("EPSG:", ""));
            // stacItem.ProjectionExtension().Epsg = null;
        }
    }
}
