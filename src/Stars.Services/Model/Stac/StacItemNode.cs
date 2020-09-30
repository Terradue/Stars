using System.Collections.Generic;
using Stac.Item;
using System.Linq;
using Newtonsoft.Json;
using Stac;
using Terradue.Stars.Services.Router;
using Terradue.Stars.Interface.Router;
using GeoJSON.Net.Geometry;

namespace Terradue.Stars.Services.Model.Stac
{
    public class StacItemNode : StacNode, IItem
    {
        public StacItemNode(IStacItem stacItem) : base(stacItem)
        {
            contentType.Parameters.Add("profile", "stac-item");
        }

        public IStacItem StacItem => stacObject as IStacItem;

        public override ResourceType ResourceType => ResourceType.Item;

        public IGeometryObject Geometry => StacItem.Geometry;

        public IDictionary<string, IAsset> GetAssets()
        {
            return StacItem.Assets.ToDictionary(asset => asset.Key, asset => (IAsset)new StacAssetAsset(asset.Value));
        }

        public override IList<IRoute> GetRoutes()
        {
            return new List<IRoute>();
        }

    }
}