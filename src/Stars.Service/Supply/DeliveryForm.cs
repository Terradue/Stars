using System.Collections.Generic;
using Stars.Interface.Router;

namespace Stars.Service.Supply
{
    public class DeliveryForm
    {
        private readonly IRoute nodeDeliveredRoute;
        private readonly IEnumerable<IRoute> assetsDeliveredRoutes;

        public DeliveryForm(IRoute nodeDeliveredRoute, IEnumerable<IRoute> assetsDeliveredRoutes)
        {
            this.nodeDeliveredRoute = nodeDeliveredRoute;
            this.assetsDeliveredRoutes = assetsDeliveredRoutes;
        }

        public IRoute NodeDeliveredRoute => nodeDeliveredRoute;

        public IEnumerable<IRoute> AssetsDeliveredRoutes => assetsDeliveredRoutes;

        public INode SupplierNode { get; internal set; }
    }
}