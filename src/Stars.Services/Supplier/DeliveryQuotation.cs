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
        private readonly IResource supplierNode;
        private IDictionary<string, IOrderedEnumerable<IDelivery>> assetsDeliveryQuotes;

        public DeliveryQuotation(IResource supplierNode, IDictionary<string, IOrderedEnumerable<IDelivery>> assetsQuotes)
        {
            this.supplierNode = supplierNode;
            this.assetsDeliveryQuotes = assetsQuotes;
        }

        public IDictionary<string, IOrderedEnumerable<IDelivery>> AssetsDeliveryQuotes => assetsDeliveryQuotes;

        public IResource SupplierNode => supplierNode;
    }
}