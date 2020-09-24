using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supply;
using Terradue.Stars.Interface.Supply.Asset;
using Terradue.Stars.Interface.Supply.Destination;
using Terradue.Stars.Service.Router;
using Terradue.Stars.Service.Router.Translator;
using Terradue.Stars.Service.Supply;
using Terradue.Stars.Service.Supply.Destination;

namespace Terradue.Stars.Service.Supply
{
    public class SupplyService : IStarsService
    {
        public SupplyTaskParameters Parameters { get; set; }
        protected readonly ILogger logger;
        protected readonly SupplierManager suppliersManager;
        protected readonly TranslatorManager translatorManager;

        public SupplyService(ILogger logger, SupplierManager suppliersManager, TranslatorManager translatorManager)
        {
            this.logger = logger;
            this.suppliersManager = suppliersManager;
            this.translatorManager = translatorManager;
            Parameters = new SupplyTaskParameters();
        }

        public async Task<NodeInventory> ExecuteAsync(INode node, IDestination destination)
        {
            logger.LogDebug("{0} -> {1}", node.Uri, destination.Uri);
            var suppliers = InitSuppliersEnumerator();

            NodeInventory deliveryForm = null;

            while (deliveryForm == null && suppliers.MoveNext())
            {
                logger.LogDebug("Searching at {0} supplier for {1}", suppliers.Current.Id, node.Id);
                var supplierNode = await AskForSupply(node, suppliers.Current);
                if (supplierNode == null)
                {
                    logger.LogDebug("[{0}]  -->  no supply possible", suppliers.Current.Id);
                    continue;
                }
                logger.LogDebug("[{0}] resource found at {1}", suppliers.Current.Id, supplierNode.Uri);

                IDeliveryQuotation deliveryQuotation = QuoteDelivery(suppliers.Current, supplierNode, destination);
                if (deliveryQuotation == null)
                {
                    logger.LogDebug("[{0}]  -->  no delivery possible", suppliers.Current.Id);
                    continue;
                }

                logger.LogDebug("[{0}] Delivery quotation for {1} assets", suppliers.Current.Id, deliveryQuotation.AssetsDeliveryQuotes.Count);

                if (!CheckDelivery(deliveryQuotation))
                {
                    continue;
                }

                deliveryForm = await Deliver(deliveryQuotation);
                if (deliveryForm == null)
                {
                    if (Parameters.ContinueOnDeliveryError)
                    {
                        logger.LogDebug("[{0}] Delivery failed. Skipping supplier", suppliers.Current.Id);
                        continue;
                    }
                    
                }

                deliveryForm.SupplierNode = supplierNode;

                return deliveryForm;

            }

            return null;
        }

        private bool CheckDelivery(IDeliveryQuotation deliveryQuotation)
        {
            if (deliveryQuotation.NodeDeliveryQuotes.Item2.Count() == 0)
                logger.LogWarning("[{0}]N[{1}] No Carrier", deliveryQuotation.Supplier.Id, deliveryQuotation.NodeDeliveryQuotes.Item1.Uri);
            else
            {
                logger.LogDebug("[{0}]N[{1}] {2} carriers", deliveryQuotation.Supplier.Id,
                     deliveryQuotation.NodeDeliveryQuotes.Item1.Uri, deliveryQuotation.NodeDeliveryQuotes.Item2.Count());
                int j = 1;
                foreach (var delivery in deliveryQuotation.NodeDeliveryQuotes.Item2)
                {
                    logger.LogDebug("[{0}]N[{1}]#{2}[{3}] to {4} : {5}$", deliveryQuotation.Supplier.Id,
                        deliveryQuotation.NodeDeliveryQuotes.Item1.Uri, j,
                        delivery.Carrier.Id, delivery.TargetUri.ToString(), delivery.Cost);
                    j++;
                }
                j++;
            }
            foreach (var item in deliveryQuotation.AssetsDeliveryQuotes)
            {
                if (item.Value.Count() == 0)
                {
                    logger.LogDebug("[{0}]A[{1}] No carrier. Skipping supplier.", deliveryQuotation.Supplier.Id,
                        item.Key);
                    return false;
                }

                logger.LogDebug("[{0}]A[{1}] : {2} carriers", deliveryQuotation.Supplier.Id,
                        item.Key, item.Value.Count());
                int j = 1;
                foreach (var delivery in item.Value)
                {
                    logger.LogDebug("[{0}]A[{1}]#{2}[{3}] to {4} : {5}$", deliveryQuotation.Supplier.Id,
                        item.Key, j, delivery.Carrier.Id, delivery.TargetUri.ToString(), delivery.Cost);
                    j++;
                }
            }
            return true;
        }

        private async Task<NodeInventory> Deliver(IDeliveryQuotation deliveryQuotation)
        {
            IRoute nodeDeliveredRoute = null;
            if (deliveryQuotation.NodeDeliveryQuotes.Item2.Count() > 0)
            {
                var route = await Deliver(deliveryQuotation.NodeDeliveryQuotes.Item1.Uri.ToString(), deliveryQuotation.NodeDeliveryQuotes.Item2);
                if (route == null) return null;
                nodeDeliveredRoute = route;
            }
            Dictionary<string, IAsset> assetsDeliveredRoutes = new Dictionary<string, IAsset>();
            foreach (var item in deliveryQuotation.AssetsDeliveryQuotes)
            {
                if (item.Value.Count() == 0) continue;
                var route = await Deliver(item.Key, item.Value);
                if (route == null)
                {
                    if (!Parameters.ContinueOnDeliveryError) return null;
                }
                // OK
                else
                {
                    IAsset asset = MakeAsset(route, (IAsset)item.Value.First().Route);
                    assetsDeliveredRoutes.Add(item.Key, asset);
                }
            }
            return new NodeInventory(nodeDeliveredRoute, assetsDeliveredRoutes, deliveryQuotation.Destination);
        }

        private IAsset MakeAsset(IRoute route, IAsset asset)
        {
            return new GenericAsset(route, asset.Label, asset.Roles);
        }

        private async Task<IRoute> Deliver(string key, IOrderedEnumerable<IDelivery> deliveries)
        {
            logger.LogInformation("Delivery for {0}", key);
            foreach (var delivery in deliveries)
            {
                logger.LogInformation("Delivering {0}[{1}] ({2})...", key, delivery.Route.ResourceType, delivery.Route.Uri);
                try
                {
                    IRoute delivered = await delivery.Carrier.Deliver(delivery);
                    if (delivered != null)
                    {
                        logger.LogInformation("Delivery complete to {0}", delivered.Uri);
                        return delivered;
                    }
                }
                catch (Exception e)
                {
                    logger.LogError("Error supplying {0} to {1} : {2}", delivery.Route.Uri, delivery.TargetUri, e.Message);
                }
            }
            return null;
        }

        private IDeliveryQuotation QuoteDelivery(ISupplier supplier, INode supplierNode, IDestination destination)
        {
            return supplier.QuoteDelivery(supplierNode, destination);
        }

        private async Task<INode> AskForSupply(INode node, ISupplier supplier)
        {
            return await supplier.SearchFor(node);
        }

        private IEnumerator<ISupplier> InitSuppliersEnumerator()
        {
            return suppliersManager.GetSuppliers(Parameters.SupplierFilters).GetEnumerator();
        }
    }

}