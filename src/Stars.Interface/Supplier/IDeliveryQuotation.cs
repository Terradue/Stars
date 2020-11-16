using System;
using System.Collections.Generic;
using System.Linq;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier.Destination;

namespace Terradue.Stars.Interface.Supplier
{
    public interface IDeliveryQuotation
    {
        IResource SupplierNode { get; }

        IDictionary<string, IOrderedEnumerable<IDelivery>> AssetsDeliveryQuotes { get; }

    }
}