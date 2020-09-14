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

        public async Task<IStacNode> ExecuteAsync(INode node, IDestination destination)
        {
            logger.LogDebug("{0} -> {1}", node.Uri, destination.Uri);
            var suppliers = InitSuppliersEnumerator();

            IStacNode nodeAtDestination = null;

            while (nodeAtDestination == null && suppliers.MoveNext())
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

                logger.LogDebug("[{0}] Delivery quotation for {1} routes", suppliers.Current.Id, deliveryQuotation.DeliveryQuotes.Count);

                if (!CheckDelivery(deliveryQuotation))
                {
                    continue;
                }

                var deliveryForm = await OrderDelivery(deliveryQuotation);

            }

            return nodeAtDestination;
        }

        private bool CheckDelivery(IDeliveryQuotation deliveryQuotation)
        {
            foreach (var item in deliveryQuotation.DeliveryQuotes)
            {
                if (item.Value.Count() == 0)
                {
                    logger.LogDebug("Missing carrier for route {0}. Skipping supplier.", item.Key.Uri);
                    return false;
                }
                int j = 1;
                logger.LogDebug("Route {0} : {1} carriers", item.Key.Uri.ToString(), item.Value.Count());
                foreach (var delivery in item.Value)
                {
                    logger.LogDebug("Delivery #{0} by carrier {1} to {2} : {3}$", j, delivery.Carrier.Id, delivery.Cost, delivery.Destination.Uri.ToString());
                    j++;
                }
            }
            return true;
        }

        private async Task<DeliveryForm> OrderDelivery(IDeliveryQuotation deliveryQuotation)
        {
            List<IRoute> deliveredRoutes = new List<IRoute>();
            foreach (var item in deliveryQuotation.DeliveryQuotes)
            {
                logger.LogDebug("Trying to copy Route {0}...", item.Key.Uri.ToString());
                int j = 1;
                foreach (var delivery in item.Value)
                {
                    try
                    {
                        IRoute delivered = await delivery.Carrier.Deliver(delivery.Route, delivery.Supplier, delivery.Destination);
                        if (delivered != null)
                        {
                            deliveredRoutes.Add(delivered);
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        logger.LogError("Error copying {0} to {1} : {2}", delivery.Route.Uri, delivery.Destination.Uri, e.Message);
                    }

                    j++;
                }
            }
            return new DeliveryForm(deliveredRoutes);
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