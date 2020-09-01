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
using Stars.Router;
using Stars.Supply;
using Stars.Supply.Destination;
using Terradue.ServiceModel.Syndication;

namespace Stars.Supply
{
    [PluginPriority(10)]
    public class GenericSupplier : ISupplier
    {
        private readonly CarrierManager carriersManager;

        public GenericSupplier(CarrierManager carriersManager)
        {
            this.carriersManager = carriersManager;
        }

        public string Id => "Native";

        public Task<INode> SearchFor(INode resource)
        {
            return Task.FromResult<INode>(null);
        }

        public IDeliveryQuotation QuoteDelivery(INode resource, IDestination destination)
        {
            return carriersManager.QuoteDeliveryFromCarriers((this, resource), destination);
        }
    }
}
