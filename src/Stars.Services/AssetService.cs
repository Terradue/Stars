using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Services.Router;
using Terradue.Stars.Services.Store;
using Terradue.Stars.Services.Supplier;
using Terradue.Stars.Services.Supplier.Carrier;
using Terradue.Stars.Services.Translator;

namespace Terradue.Stars.Services
{
    public class AssetService : IStarsService
    {
        protected readonly ILogger logger;
        private readonly RoutersManager routersManager;
        protected readonly SupplierManager suppliersManager;
        protected readonly TranslatorManager translatorManager;
        private readonly CarrierManager carrierManager;

        public AssetService(ILogger<AssetService> logger, RoutersManager routersManager, SupplierManager suppliersManager, TranslatorManager translatorManager, CarrierManager carrierManager)
        {
            this.logger = logger;
            this.routersManager = routersManager;
            this.suppliersManager = suppliersManager;
            this.translatorManager = translatorManager;
            this.carrierManager = carrierManager;
        }

        public async Task<StacNode> ImportToStore(IResource node, StoreService storeService, IDestination destination, SupplyParameters parameters)
        {
            if (!(node is IAssetsContainer)) return null;

            logger.LogDebug("Import assets from {0} to {1}", node.Uri, storeService.Id);

            var suppliers = InitSuppliersEnumerator(node, parameters);

            IDictionary<string, IAsset> assets = null;
            IResource supplierNode = null;

            while (assets == null && suppliers.MoveNext())
            {
                logger.LogInformation("[{0}] Searching for {1}", suppliers.Current.Id, node.Uri.ToString());
                supplierNode = await AskForSupply(node, suppliers.Current);
                if (supplierNode == null)
                {
                    logger.LogInformation("[{0}] --> no supply possible", suppliers.Current.Id);
                    continue;
                }
                logger.LogInformation("[{0}] resource found at {1} [{2}]", suppliers.Current.Id, supplierNode.Uri, supplierNode.ContentType);

                IDeliveryQuotation deliveryQuotation = QuoteDelivery(suppliers.Current, supplierNode, destination);
                if (deliveryQuotation == null || deliveryQuotation.AssetsDeliveryQuotes.Count() == 0)
                {
                    logger.LogDebug("[{0}]  -->  no delivery possible", suppliers.Current.Id);
                    continue;
                }

                logger.LogDebug("[{0}] Delivery quotation for {1} assets", suppliers.Current.Id, deliveryQuotation.AssetsDeliveryQuotes.Count);

                if (!CheckDelivery(deliveryQuotation, suppliers.Current))
                {
                    continue;
                }

                assets = await Deliver(deliveryQuotation, parameters);
                if (assets == null)
                {
                    if (parameters.ContinueOnDeliveryError)
                    {
                        logger.LogDebug("[{0}] Asset import failed. Skipping supplier", suppliers.Current.Id);
                        continue;
                    }
                }
            }

            if ( assets == null && !parameters.ContinueOnDeliveryError )
                throw new InvalidOperationException(string.Format("Assets import failed for node {0}", node.Uri));

            return await storeService.StoreNodeAtDestination(supplierNode, assets, destination, null);
        }

        private bool CheckDelivery(IDeliveryQuotation deliveryQuotation, ISupplier supplier)
        {
            foreach (var item in deliveryQuotation.AssetsDeliveryQuotes)
            {
                if (item.Value.Count() == 0)
                {
                    logger.LogDebug("[{0}]A[{1}] No carrier. Skipping supplier.", supplier.Id,
                        item.Key);
                    return false;
                }

                logger.LogDebug("[{0}]A[{1}] : {2} carriers", supplier.Id,
                        item.Key, item.Value.Count());
                int j = 1;
                foreach (var delivery in item.Value)
                {
                    logger.LogDebug("[{0}]A[{1}]#{2}[{3}] to {4} : {5}$", supplier.Id,
                        item.Key, j, delivery.Carrier.Id, delivery.Destination.ToString(), delivery.Cost);
                    j++;
                }
            }
            return true;
        }

        private async Task<IDictionary<string, IAsset>> Deliver(IDeliveryQuotation deliveryQuotation, SupplyParameters parameters)
        {
            Dictionary<string, IAsset> assetsDeliveredRoutes = new Dictionary<string, IAsset>();
            foreach (var assetDeliveries in deliveryQuotation.AssetsDeliveryQuotes)
            {
                if (assetDeliveries.Value.Count() == 0) continue;
                var route = await Deliver(assetDeliveries.Key, assetDeliveries.Value);
                if (route == null)
                {
                    if (!parameters.ContinueOnDeliveryError) return null;
                }
                // OK
                else
                {
                    IAsset asset = MakeAsset(route, (IAsset)assetDeliveries.Value.First().Route);
                    assetsDeliveredRoutes.Add(assetDeliveries.Key, asset);
                }
            }
            return assetsDeliveredRoutes;
        }

        private IAsset MakeAsset(IResource route, IAsset assetOrigin)
        {
            if (route is IAsset) return route as IAsset;
            return new GenericAsset(route, assetOrigin.Label, assetOrigin.Roles);
        }

        private async Task<IResource> Deliver(string key, IOrderedEnumerable<IDelivery> deliveries)
        {
            logger.LogInformation("Starting delivery for {0}", key);
            foreach (var delivery in deliveries)
            {
                logger.LogInformation("Delivering {0} {1} {2} ({3})...", key, delivery.Route.ResourceType, delivery.Route.Uri, delivery.Carrier.Id);
                try
                {
                    delivery.Destination.PrepareDestination();
                    IResource delivered = await delivery.Carrier.Deliver(delivery);
                    if (delivered != null)
                    {
                        logger.LogInformation("Delivery complete to {0}", delivered.Uri);
                        return delivered;
                    }
                }
                catch (Exception e)
                {
                    logger.LogError("Error delivering {0} ({1}) : {2}", key, delivery.Carrier.Id, e.Message);
                }
            }
            return null;
        }

        private IDeliveryQuotation QuoteDelivery(ISupplier supplier, IResource supplierNode, IDestination destination)
        {
            try
            {
                return supplier.QuoteDelivery(supplierNode, destination);
            }
            catch (Exception e)
            {
                logger.LogWarning("Exception during quotation for {0} at {1} : {2}", supplierNode.Uri, supplier.Id, e.Message);
                logger.LogDebug(e.StackTrace);
            }
            return null;
        }

        private async Task<IResource> AskForSupply(IResource node, ISupplier supplier)
        {
            try
            {
                return await supplier.SearchFor(node);
            }
            catch (Exception e)
            {
                logger.LogWarning("Exception during search for {0} at {1} : {2}", node.Uri, supplier.Id, e.Message);
            }
            return null;

        }

        private IEnumerator<ISupplier> InitSuppliersEnumerator(IResource route, SupplyParameters parameters)
        {
            if (route is IItem)
                return suppliersManager.GetSuppliers(parameters.SupplierFilters).GetEnumerator();

            return new ISupplier[1] { new NativeSupplier(carrierManager) }.ToList().GetEnumerator();
        }
    }

}