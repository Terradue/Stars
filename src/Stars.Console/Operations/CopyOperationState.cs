using System.Collections.Generic;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supply.Asset;
using Terradue.Stars.Interface.Supply.Destination;

namespace Terradue.Stars.Operations
{
    internal class CopyOperationState
    {
        public CopyOperationState(int depth, IDestination destination)
        {
            Depth = depth;
            Destination = destination;
        }

        public int Depth { get; internal set; }
        public IDestination Destination { get; internal set; }
        public IRoute LastRoute { get; internal set; }
        public IDictionary<string, IAsset> Assets { get; internal set; }
    }
}