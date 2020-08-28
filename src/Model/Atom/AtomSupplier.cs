using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stac.Catalog;
using Stac.Item;
using Stars.Router;
using Stars.Supply;
using Stars.Supply.Destination;
using Terradue.ServiceModel.Syndication;

namespace Stars.Model.Atom
{
    [PluginPriority(10)]
    public class AtomSupplier : ISupplier
    {
        private readonly CarrierManager carriersManager;

        public AtomSupplier(CarrierManager carriersManager)
        {
            this.carriersManager = carriersManager;
        }

        public string Id => "Atom";

        public bool CanSupply(IResource resource)
        {
            return ( resource is AtomItemRoutable || resource is AtomFeedRoutable );
        }

        public Task<IEnumerable<IResource>> LocalizeResource(IResource resource)
        {
            List<IResource> resources = new List<IResource>();
            if ( resource is AtomItemRoutable || resource is AtomFeedRoutable ){
                resources.Add(resource);
            }
            return Task.FromResult((IEnumerable<IResource>)resources);
        }

        public DeliveryQuotation QuoteDelivery(IResource resource, IDestination destination)
        {
            return carriersManager.QuoteDeliveryFromCarriers((this, resource), destination);
        }
    }
}
