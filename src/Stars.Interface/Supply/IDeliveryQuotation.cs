using System;
using System.Collections.Generic;
using System.Linq;
using Stars.Interface.Router;

namespace Stars.Interface.Supply
{
    public interface IDeliveryQuotation
    {

        (IRoute, IOrderedEnumerable<IDelivery>) NodeDeliveryQuotes { get; }

        IDictionary<IRoute, IOrderedEnumerable<IDelivery>> AssetsDeliveryQuotes { get; }
        
        ISupplier Supplier { get; }
    }
}