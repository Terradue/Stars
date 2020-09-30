using System;
using System.Collections.Generic;
using System.Linq;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier.Destination;

namespace Terradue.Stars.Interface.Supplier
{
    public interface IDeliveryQuotation
    {

        (IRoute, IOrderedEnumerable<IDelivery>) NodeDeliveryQuotes { get; }

        IDictionary<string, IOrderedEnumerable<IDelivery>> AssetsDeliveryQuotes { get; }
        
        ISupplier Supplier { get; }
        
        IDestination Destination { get; }
    }
}