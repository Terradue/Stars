using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stars.Interface;
using Stars.Interface.Model;
using Stars.Interface.Router;
using Stars.Interface.Supply;
using Stars.Interface.Supply.Destination;
using Stars.Service.Router;
using Stars.Service.Router.Translator;
using Stars.Service.Supply;
using Stars.Service.Supply.Asset;
using Stars.Service.Supply.Destination;

namespace Stars.Service.Supply
{
    public class SupplyingTask : IStarsTask
    {
        public SupplyTaskParameters Parameters { get; set; }
        private readonly ILogger logger;
        private readonly SupplierManager suppliersManager;
        private readonly TranslatorManager translatorManager;

        public SupplyingTask(ILogger logger, SupplierManager suppliersManager, TranslatorManager translatorManager)
        {
            this.logger = logger;
            this.suppliersManager = suppliersManager;
            this.translatorManager = translatorManager;
            Parameters = new SupplyTaskParameters();
        }

        public async Task<DeliveryForm> ExecuteAsync(INode node, IDestination destination)
        {
            logger.LogDebug("{0} -> {1}", node.Uri, destination.Uri);
            var suppliers = InitSuppliersEnumerator();

            DeliveryForm deliveryForm = null;

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



            }

            return deliveryForm;
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
                        delivery.Carrier.Id, delivery.Destination.Uri.ToString(), delivery.Cost);
                    j++;
                }
                j++;
            }
            foreach (var item in deliveryQuotation.AssetsDeliveryQuotes)
            {
                if (item.Value.Count() == 0)
                {
                    logger.LogDebug("[{0}]A[{1}] No carrier. Skipping supplier.", deliveryQuotation.Supplier.Id,
                        item.Key.Uri);
                    return false;
                }

                logger.LogDebug("[{0}]A[{1}] : {2} carriers", deliveryQuotation.Supplier.Id,
                        item.Key.Uri, item.Value.Count());
                int j = 1;
                foreach (var delivery in item.Value)
                {
                    logger.LogDebug("[{0}]A[{1}]#{2}[{3}] to {4} : {5}$", deliveryQuotation.Supplier.Id,
                        item.Key.Uri, j, delivery.Carrier.Id, delivery.Destination.Uri.ToString(), delivery.Cost);
                    j++;
                }
            }
            return true;
        }

        private async Task<DeliveryForm> Deliver(IDeliveryQuotation deliveryQuotation)
        {
            List<IRoute> deliveredRoutes = new List<IRoute>();
            if (deliveryQuotation.NodeDeliveryQuotes.Item2.Count() > 0)
            {
                var route = await Deliver(deliveryQuotation.NodeDeliveryQuotes.Item1, deliveryQuotation.NodeDeliveryQuotes.Item2);
                if (route == null) return null;
                deliveredRoutes.Add(route);
            }

            foreach (var item in deliveryQuotation.AssetsDeliveryQuotes)
            {
                var route = await Deliver(item.Key, item.Value);
                if (route == null) return null;
                deliveredRoutes.Add(route);
                break;
            }
            return new DeliveryForm(deliveredRoutes);
        }

        private async Task<IRoute> Deliver(IRoute route, IOrderedEnumerable<IDelivery> deliveries)
        {
            logger.LogInformation("Delivering {0} {1}...", route.ResourceType, route.Uri);
            foreach (var delivery in deliveries)
            {
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
                    logger.LogError("Error supplying {0} to {1} : {2}", delivery.Route.Uri, delivery.Destination.Uri, e.Message);
                    logger.LogDebug(e.StackTrace);
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