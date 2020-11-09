using System.Collections.Generic;
using Stac.Item;
using System.Linq;
using Newtonsoft.Json;
using Stac;
using Terradue.Stars.Services.Router;
using Terradue.Stars.Interface.Router;
using GeoJSON.Net.Geometry;
using System.Net;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Services.Model.Stac
{
    public class StacItemNode : StacNode, IItem
    {
        private readonly ICredentials credentials;

        public StacItemNode(IStacItem stacItem, System.Net.ICredentials credentials = null) : base(stacItem)
        {
            contentType.Parameters.Add("profile", "stac-item");
            this.credentials = credentials;
        }

        public IStacItem StacItem => stacObject as IStacItem;

        public override ResourceType ResourceType => ResourceType.Item;

        public IGeometryObject Geometry => StacItem.Geometry;

        public IDictionary<string, object> Properties => StacItem.Properties;

        public IDictionary<string, IAsset> GetAssets()
        {
            return StacItem.Assets.ToDictionary(asset => asset.Key, asset => (IAsset)new StacAssetAsset(asset.Value, this, credentials));
        }

        public override IList<IResource> GetRoutes()
        {
            return new List<IResource>();
        }

    }
}