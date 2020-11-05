using System.Collections.Generic;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;

using Terradue.Stars.Interface.Supplier.Destination;

namespace Terradue.Stars.Console.Operations
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
        public IResource LastRoute { get; internal set; }
    }
}