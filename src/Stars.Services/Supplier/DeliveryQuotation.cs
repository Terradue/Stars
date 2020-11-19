using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Router;
using Terradue.Stars.Services.Supplier.Destination;

namespace Terradue.Stars.Services.Supplier
{
    public class DeliveryQuotation : IDeliveryQuotation
    {
        private IDictionary<string, IOrderedEnumerable<IDelivery>> assetsDeliveryQuotes;
        private readonly Dictionary<string, Exception> assetsExceptions;

        public DeliveryQuotation(IDictionary<string, IOrderedEnumerable<IDelivery>> assetsQuotes, Dictionary<string, Exception> assetsExceptions)
        {
            this.assetsDeliveryQuotes = assetsQuotes;
            this.assetsExceptions = assetsExceptions;
        }

        public IDictionary<string, IOrderedEnumerable<IDelivery>> AssetsDeliveryQuotes => assetsDeliveryQuotes;

        public IDictionary<string, Exception> AssetsExceptions => assetsExceptions;
    }
}