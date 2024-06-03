using System;
using System.Collections.Generic;
using System.Linq;

namespace Terradue.Stars.Interface.Supplier
{
    public interface IDeliveryQuotation
    {
        IDictionary<string, IOrderedEnumerable<IDelivery>> AssetsDeliveryQuotes { get; }

        IDictionary<string, Exception> AssetsExceptions { get; }

    }
}
