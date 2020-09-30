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
using Terradue.Stars.Interface.Supplier;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Router;
using Terradue.Stars.Services.Supplier;
using Terradue.Stars.Services.Supplier.Carrier;
using Terradue.Stars.Services.Supplier.Destination;
using Terradue.Stars.Services.Translator;

namespace Terradue.Stars.Services.Supplier
{
    public class SupplierService : IStarsService
    {
        public SupplierServiceParameters Parameters { get; set; }
        protected readonly ILogger logger;
        private readonly RoutersManager routersManager;
        protected readonly SupplierManager suppliersManager;
        protected readonly TranslatorManager translatorManager;
        private readonly CarrierManager carrierManager;

        public SupplierService(ILogger logger, RoutersManager routersManager, SupplierManager suppliersManager, TranslatorManager translatorManager, CarrierManager carrierManager)
        {
            this.logger = logger;
            this.routersManager = routersManager;
            this.suppliersManager = suppliersManager;
            this.translatorManager = translatorManager;
            this.carrierManager = carrierManager;
            Parameters = new SupplierServiceParameters();
        }

        public async Task<IRoute> ExecuteAsync(IRoute route, IDestination destination)
        {
            logger.LogDebug("{0} -> {1}", route.Uri, destination.Uri);

            var suppliers = InitSuppliersEnumerator(route);

            IRoute deliveryRoute = null;

            while (deliveryRoute == null && suppliers.MoveNext())
            {
                logger.LogDebug("Searching at {0} supplier for {1}", suppliers.Current.Id, route.Uri.ToString());
                var supplierNode = await AskForSupply(route, suppliers.Current);
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

                deliveryRoute = await Deliver(deliveryQuotation);
                if (deliveryRoute == null)
                {
                    if (Parameters.ContinueOnDeliveryError)
                    {
                        logger.LogDebug("[{0}] Delivery failed. Skipping supplier", suppliers.Current.Id);
                        continue;
                    }

                }

                return deliveryRoute;

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

        private async Task<IRoute> Deliver(IDeliveryQuotation deliveryQuotation)
        {
            IRoute nodeDeliveredRoute = null;
            if (deliveryQuotation.NodeDeliveryQuotes.Item2.Count() > 0)
            {
                var route = await Deliver(deliveryQuotation.NodeDeliveryQuotes.Item1.Uri.ToString(), deliveryQuotation.NodeDeliveryQuotes.Item2);
                if (route == null) return null;
                nodeDeliveredRoute = route;
                IRouter router = routersManager.GetRouter(route);
                if ( router != null )
                    nodeDeliveredRoute = await router.Route(route);
            }
            Dictionary<string, IAsset> assetsDeliveredRoutes = new Dictionary<string, IAsset>();
            foreach (var assetDeliveries in deliveryQuotation.AssetsDeliveryQuotes)
            {
                if (assetDeliveries.Value.Count() == 0) continue;
                var route = await Deliver(assetDeliveries.Key, assetDeliveries.Value);
                if (route == null)
                {
                    if (!Parameters.ContinueOnDeliveryError) return null;
                }
                // OK
                else
                {
                    IAsset asset = MakeAsset(route, (IAsset)assetDeliveries.Value.First().Route);
                    assetsDeliveredRoutes.Add(assetDeliveries.Key, asset);
                }
            }
            if (nodeDeliveredRoute is IItem)
            {
                return MakeItem(nodeDeliveredRoute as IItem, assetsDeliveredRoutes, deliveryQuotation);
            }
            return nodeDeliveredRoute;
        }

        private IRoute MakeItem(IItem item, Dictionary<string, IAsset> assets, IDeliveryQuotation deliveryQuotation)
        {
            return new ContainerNode(item, assets, deliveryQuotation.Destination);
        }

        private IAsset MakeAsset(IRoute route, IAsset assetOrigin)
        {
            if (route is IAsset) return route as IAsset;
            return new GenericAsset(route, assetOrigin.Label, assetOrigin.Roles);
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

        private IDeliveryQuotation QuoteDelivery(ISupplier supplier, IRoute supplierRoute, IDestination destination)
        {
            return supplier.QuoteDelivery(supplierRoute, destination);
        }

        private async Task<IRoute> AskForSupply(IRoute node, ISupplier supplier)
        {
            return await supplier.SearchFor(node);
        }

        private IEnumerator<ISupplier> InitSuppliersEnumerator(IRoute route)
        {
            if (route is IItem)
                return suppliersManager.GetSuppliers(Parameters.SupplierFilters).GetEnumerator();

            return new ISupplier[1] { new NativeSupplier(carrierManager) }.ToList().GetEnumerator();
        }
    }

}