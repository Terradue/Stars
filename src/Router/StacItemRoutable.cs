using System.Collections.Generic;
using Stac.Item;
using System.Linq;
using Newtonsoft.Json;
using Stac;

namespace Stars.Router
{
    internal class StacItemRoutable : StacRoutable, IRoutable
    {
        public StacItemRoutable(IStacItem stacItem) : base(stacItem)
        {
            contentType.Parameters.Add("profile", "stac-item");
        }

        public IStacItem StacItem => stacObject as IStacItem;

        public override IEnumerable<IRoute> GetRoutes()
        {
            return StacItem.Assets.Select(asset => new StacAssetRoute(asset.Value, StacItem)).Cast<IRoute>();
        }

        public override string ReadAsString()
        {
            return JsonConvert.SerializeObject(StacItem);
        }
    }
}