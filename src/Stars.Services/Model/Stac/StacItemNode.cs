using System.Collections.Generic;
using Stac.Item;
using System.Linq;
using Newtonsoft.Json;
using Stac;
using Terradue.Stars.Services.Router;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supply.Asset;

namespace Terradue.Stars.Services.Model.Stac
{
    public class StacItemNode : StacNode, IAssetsContainer, IRoutable
    {
        public StacItemNode(IStacItem stacItem) : base(stacItem)
        {
            contentType.Parameters.Add("profile", "stac-item");
        }

        public IStacItem StacItem => stacObject as IStacItem;

        public override ResourceType ResourceType => ResourceType.Item;

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