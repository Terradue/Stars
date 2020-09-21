using System;
using System.Collections.Generic;
using System.Linq;
using Stars.Interface.Router;
using Stars.Interface.Supply.Asset;
using Stars.Interface.Supply.Destination;

namespace Stars.Interface.Supply
{
    public interface IDeliveryQuotation
    {

        (IRoute, IOrderedEnumerable<IDelivery>) NodeDeliveryQuotes { get; }

        IDictionary<string, IOrderedEnumerable<IDelivery>> AssetsDeliveryQuotes { get; }
        
        ISupplier Supplier { get; }
        
        IDestination Destination { get; }
    }
}