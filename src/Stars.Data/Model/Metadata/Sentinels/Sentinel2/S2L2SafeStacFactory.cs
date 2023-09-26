using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Stac;
using Stac.Extensions.Eo;
using Stac.Extensions.Projection;
using Stac.Extensions.Sar;
using Terradue.OpenSearch.Sentinel.Data.Safe;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Data.Model.Metadata.Sentinels.Sentinel2
{
    public class S2L2SafeStacFactory : S2SafeStacFactory
    {

        private readonly Level2A_Tile mtdTile;

        public S2L2SafeStacFactory(XFDUType xfdu, IItem route, string identifier, Level2A_Tile mtdTile) : base(xfdu, route, identifier, null)
        {
            this.mtdTile = mtdTile;
        }

        public static S2L2SafeStacFactory Create(XFDUType xfdu, IItem route, string identifier, Level2A_Tile mtdTile)
        {
            return new S2L2SafeStacFactory(xfdu, route, identifier, mtdTile);
        }

        protected override void AddEoStacExtension(StacItem stacItem)
        {
            double? cloudCover = mtdTile?.Quality_Indicators_Info?.Image_Content_QI?.CLOUDY_PIXEL_PERCENTAGE.Value;
            EoStacExtension eo = stacItem.EoExtension();
            if (cloudCover != null) eo.CloudCover = cloudCover.Value;
        }

        protected override void AddProjectionStacExtension(StacItem stacItem)
        {
            if ( mtdTile?.Geometric_Info == null ) return;
            stacItem.ProjectionExtension().Epsg = int.Parse(mtdTile.Geometric_Info.Tile_Geocoding.HORIZONTAL_CS_CODE.Replace("EPSG:", ""));
        }
    }
}