using System.Collections.Generic;
using Stac.Item;
using System.Linq;
using Newtonsoft.Json;
using Stac;
using Stars.Service.Router;
using Stars.Service.Supply.Asset;
using Stars.Interface.Router;
using Stars.Interface.Supply.Asset;

namespace Stars.Service.Model.Stac
{
    internal class StacItemResource : StacResource, IAssetsContainer, IRoutable
    {
        public StacItemResource(IStacItem stacItem) : base(stacItem)
        {
            contentType.Parameters.Add("profile", "stac-item");
        }

        public IStacItem StacItem => stacObject as IStacItem;

        public override ResourceType ResourceType => ResourceType.Item;

        public IEnumerable<IAsset> GetAssets()
        {
            return StacItem.Assets.Select(asset => new StacAssetAsset(asset.Value, StacItem)).Cast<IAsset>();
        }

        public override IList<IRoute> GetRoutes()
        {
            return new List<IRoute>();
        }

        public override string ReadAsString()
        {
            return JsonConvert.SerializeObject(StacItem);
        }
    }
}