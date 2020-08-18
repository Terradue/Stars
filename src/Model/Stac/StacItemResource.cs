using System.Collections.Generic;
using Stac.Item;
using System.Linq;
using Newtonsoft.Json;
using Stac;
using Stars.Router;
using Stars.Supplier.Asset;

namespace Stars.Model.Stac
{
    internal class StacItemResource : StacResource, IAssetsContainer
    {
        public StacItemResource(IStacItem stacItem) : base(stacItem)
        {
            contentType.Parameters.Add("profile", "stac-item");
        }

        public IStacItem StacItem => stacObject as IStacItem;

        public override ResourceType ResourceType => ResourceType.Item;

        public override string Filename => stacObject.Id.CleanIdentifier() + ".item.json";

        public IEnumerable<IAsset> GetAssets()
        {
            return StacItem.Assets.Select(asset => new StacAssetAsset(asset.Value, StacItem)).Cast<IAsset>();
        }

        public override string ReadAsString()
        {
            return JsonConvert.SerializeObject(StacItem);
        }
    }
}