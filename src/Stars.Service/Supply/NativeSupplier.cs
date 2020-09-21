using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stac.Catalog;
using Stac.Item;
using Stars.Interface.Router;
using Stars.Interface.Supply;
using Stars.Interface.Supply.Destination;
using Stars.Service.Router;
using Stars.Service.Supply;
using Stars.Service.Supply.Carrier;
using Stars.Service.Supply.Destination;
using Terradue.ServiceModel.Syndication;

namespace Stars.Service.Supply
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
