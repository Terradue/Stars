using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stac.Catalog;
using Stac.Item;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supply;
using Terradue.Stars.Interface.Supply.Destination;
using Terradue.Stars.Service.Router;
using Terradue.Stars.Service.Supply;
using Terradue.Stars.Service.Supply.Carrier;
using Terradue.Stars.Service.Supply.Destination;
using Terradue.ServiceModel.Syndication;

namespace Terradue.Stars.Service.Supply
{
    [PluginPriority(10)]
    public class NativeSupplier : ISupplier
    {
        private readonly CarrierManager carriersManager;

        public NativeSupplier(CarrierManager carriersManager)
        {
            this.carriersManager = carriersManager;
        }

        public string Id => "Native";

        public Task<INode> SearchFor(INode resource)
        {
            return Task.FromResult<INode>(resource);
        }

        public IDeliveryQuotation QuoteDelivery(INode resource, IDestination destination)
        {
            return carriersManager.QuoteDeliveryFromCarriers((this, resource), destination);
        }

        public Task<IOrder> Order(IOrderable orderableRoute)
        {
            throw new NotSupportedException();
        }
    }
}
