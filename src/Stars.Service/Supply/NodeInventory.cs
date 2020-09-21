using System.Collections.Generic;
using Stars.Interface.Router;
using Stars.Interface.Supply.Asset;
using Stars.Interface.Supply.Destination;

namespace Stars.Service.Supply
{
    public class NodeInventory
    {
        private readonly IRoute node;
        private readonly IDictionary<string, IAsset>  assets;

        public NodeInventory(IRoute node, IDictionary<string, IAsset>  assets, IDestination destination)
        {
            this.node = node;
            this.assets = assets;
            Destination = destination;
        }

        public IRoute Node => node;

        public IDictionary<string, IAsset> Assets => assets;

        public INode SupplierNode { get; internal set; }
        public IDestination Destination { get; internal set; }
    }
}