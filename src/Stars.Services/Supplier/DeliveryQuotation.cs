using System;
using System.Collections.Generic;
using System.Linq;
using Terradue.Stars.Interface.Supplier;

namespace Terradue.Stars.Services.Supplier
{
    public class DeliveryQuotation : IDeliveryQuotation
    {
        private IDictionary<string, IOrderedEnumerable<IDelivery>> assetsDeliveryQuotes;
        private readonly Dictionary<string, Exception> assetsExceptions;

        public DeliveryQuotation(IDictionary<string, IOrderedEnumerable<IDelivery>> assetsQuotes, Dictionary<string, Exception> assetsExceptions)
        {
            assetsDeliveryQuotes = assetsQuotes;
            this.assetsExceptions = assetsExceptions;
        }

        public IDictionary<string, IOrderedEnumerable<IDelivery>> AssetsDeliveryQuotes => assetsDeliveryQuotes;

        public IDictionary<string, Exception> AssetsExceptions => assetsExceptions;
    }
}
