using System.Collections.Generic;
using Stars.Interface.Router;
using Stars.Interface.Supply.Asset;

namespace Stars.Service.Supply
{
    public class DeliveryForm
    {
        private readonly IRoute nodeDeliveredRoute;
        private readonly IDictionary<string, IAsset>  assetsDeliveredRoutes;

        public DeliveryForm(IRoute nodeDeliveredRoute, IDictionary<string, IAsset>  assetsDeliveredRoutes)
        {
            this.nodeDeliveredRoute = nodeDeliveredRoute;
            this.assetsDeliveredRoutes = assetsDeliveredRoutes;
        }

        public IRoute NodeDeliveredRoute => nodeDeliveredRoute;

        public IDictionary<string, IAsset> AssetsDeliveredRoutes => assetsDeliveredRoutes;

        public INode SupplierNode { get; internal set; }
    }
}