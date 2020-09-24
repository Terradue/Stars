using System.Collections.Generic;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supply.Asset;
using Terradue.Stars.Interface.Supply.Destination;

namespace Terradue.Stars.Service.Supply
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