using System.Collections.Generic;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;

using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Services.Store;

namespace Terradue.Stars.Console.Operations
{
    internal class CopyOperationState
    {
        public CopyOperationState(int depth, StoreService storeService, IDestination destination)
        {
            Depth = depth;
            StoreService = storeService;
            CurrentDestination = destination;
        }

        public int Depth { get; internal set; }
        public IDestination CurrentDestination { get; internal set; }
        public StacNode CurrentStacObject { get; internal set; }
        public StoreService StoreService { get; internal set; }
    }
}