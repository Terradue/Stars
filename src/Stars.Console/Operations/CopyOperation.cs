using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Router.Translator;
using Terradue.Stars.Interface.Supplier;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Services.Router;
using Terradue.Stars.Services.Processing;
using Terradue.Stars.Services.Supplier.Carrier;
using Terradue.Stars.Services.Translator;
using Terradue.Stars.Services.Supplier;
using Terradue.Stars.Interface.Processing;
using Terradue.Stars.Interface;
using Terradue.Stars.Services;
using Terradue.Stars.Services.Store;
using System.Net;

namespace Terradue.Stars.Console.Operations
{
    [Command(Name = "copy", Description = "Copy the tree of resources and assets by routing from the input catalog")]
    internal class CopyOperation : BaseOperation
    {
        [Argument(0)]
        public string[] Inputs { get => inputs; set => inputs = value; }

        [Option("-r|--recursivity", "Resource recursivity depth routing", CommandOptionType.SingleValue)]
        public int Recursivity { get => recursivity; set => recursivity = value; }

        [Option("-sa|--skip-assets", "Do not copy assets", CommandOptionType.NoValue)]
        public bool SkippAssets { get; set; }

        [Option("-o|--output", "Output Url (Default to current dir)", CommandOptionType.SingleValue)]
        public string Output { get => output; set => output = value; }

        [Option("-ao|--allow-ordering", "Allow ordering assets", CommandOptionType.NoValue)]
        public bool AllowOrdering { get; set; }

        [Option("-xa|--extract-archive", "Extract archive files (default to true)", CommandOptionType.SingleValue)]
        public bool ExtractArchives { get; set; } = true;

        [Option("--stop-on-error", "Stop on Error (copy) (default to false)", CommandOptionType.NoValue)]
        public bool StopOnError { get; set; } = false;

        [Option("-si|--supplier-included", "Supplier to include for the data supply (default to all registered)", CommandOptionType.MultipleValue)]
        public string[] SuppliersIncluded { get; set; }

        [Option("-ac|--append-catalog", "Append to existing catalog if one is found", CommandOptionType.NoValue)]
        public bool AppendCatalog { get; set; } = false;

        [Option("-da|--delete-archives", "Delete archives from the catalog when inflated", CommandOptionType.NoValue)]
        public bool DeleteArchive { get; set; } = false;


        private RouterService routingService;
        private CarrierManager carrierManager;
        private ProcessingManager processingManager;
        private TranslatorManager translatorManager;
        private SupplierManager supplierManager;
        private StacStoreService storeService;
        private string[] inputs = new string[0];
        private int recursivity = 1;
        private string output = "file://" + Directory.GetCurrentDirectory();

        public CopyOperation()
        {

        }
        private void InitRoutingTask()
        {
            routingService.Parameters = new RouterServiceParameters()
            {
                Recursivity = Recursivity,
                SkipAssets = SkippAssets
            };
            // routingTask.OnRoutingToNodeException((route, router, exception, state) => PrintRouteInfo(route, router, exception, state));
            routingService.OnBeforeBranching((node, router, state) => CreateCatalog(node, router, state));
            routingService.OnItem((node, router, state) => CopyNode(node, router, state));
            routingService.OnBranching((parentRoute, route, siblings, state) => Task.FromResult((object)PrepareNewRoute(parentRoute, route, siblings, state)));
            routingService.OnAfterBranching(async (parentRoute, router, parentState, subStates) => await UpdateCatalog(parentRoute, router, parentState, subStates));
        }

        private async Task<object> CreateCatalog(ICatalog node, IRouter router, object state)
        {
            CopyOperationState operationState = state as CopyOperationState;
            StacNode stacNode = node as StacNode;
            if (stacNode == null)
            {
                // No? Let's try to translate it to Stac
                stacNode = await translatorManager.Translate<StacNode>(node);
                if (stacNode == null)
                    throw new InvalidDataException(string.Format("Impossible to translate node {0} into STAC.", node.Uri));
            }
            operationState.CurrentStacObject = stacNode;
            return operationState;
        }

        private async Task<object> UpdateCatalog(ICatalog parentRoute, IRouter router, object parentState, IEnumerable<object> subStates)
        {
            CopyOperationState operationState = parentState as CopyOperationState;
            if (operationState.CurrentStacObject is IItem)
                return operationState;
            if (operationState.CurrentStacObject is StacCatalogNode)
            {
                StacCatalogNode stacCatalogNode = operationState.CurrentStacObject as StacCatalogNode;
                operationState.CurrentStacObject = await operationState.StoreService.StoreNodeAtDestination(stacCatalogNode, null, operationState.CurrentDestination, subStates.Select(ss => (ss as CopyOperationState).CurrentStacObject));
            }
            return operationState;
        }

        private CopyOperationState PrepareNewRoute(IResource parentRoute, IResource newRoute, IList<IResource> siblings, object state)
        {
            if (state == null)
            {
                return new CopyOperationState(1, storeService, storeService.RootCatalogDestination);
            }

            CopyOperationState operationState = state as CopyOperationState;
            if (operationState.Depth == 0) return operationState;

            // we will use a subfolder with this id
            string id = Guid.NewGuid().ToString("N");
            if (newRoute is IItem)
                id = (newRoute as IItem).Id;
            if (newRoute is ICatalog)
                id = (newRoute as ICatalog).Id;
            var newDestination = operationState.CurrentDestination.To(newRoute, id);
            newDestination.PrepareDestination();

            return new CopyOperationState(operationState.Depth + 1, operationState.StoreService, newDestination);
        }

        private async Task<object> CopyNode(IResource node, IRouter router, object state)
        {
            CopyOperationState operationState = state as CopyOperationState;

            // We update the destination in case a new router updated the route
            IDestination destination = operationState.CurrentDestination.To(node);

            IDictionary<string, IAsset> importedAssets = null;

            // In case of a Item
            if (node is IItem)
            {

                // 1. Select Suppliers
                AssetService assetService = ServiceProvider.GetService<AssetService>();

                SupplierFilters supplierFilters = new SupplierFilters();

                if (SuppliersIncluded != null && SuppliersIncluded.Count() > 0)
                {
                    supplierFilters = new SupplierFilters();
                    supplierFilters.IncludeIds = SuppliersIncluded;
                }

                var suppliers = InitSuppliersEnumerator(node, supplierFilters);
                IResource supplierNode = null;

                // 2. Try each of them until one provide the resource
                while (suppliers.MoveNext())
                {
                    logger.Output(string.Format("[{0}] Searching for {1}", suppliers.Current.Id, node.Uri.ToString()));
                    supplierNode = await suppliers.Current.SearchFor(node);
                    if (supplierNode == null && !(supplierNode is IAssetsContainer))
                    {
                        logger.Output(string.Format("[{0}] --> no supply possible", suppliers.Current.Id));
                        continue;
                    }
                    logger.Output(string.Format("[{0}] resource found at {1} [{2}]", suppliers.Current.Id, supplierNode.Uri, supplierNode.ContentType));

                    AssetImportReport deliveryReport = await assetService.ImportAssets(supplierNode as IAssetsContainer, destination, AssetFilters.None);
                    if (StopOnError && deliveryReport.AssetsExceptions.Count > 0)
                        throw new AggregateException(deliveryReport.AssetsExceptions.Values);

                    importedAssets = deliveryReport.GetAssets();
                    break;
                }
            }

            StacNode stacNode = node as StacNode;
            if (stacNode == null)
            {
                // No? Let's try to translate it to Stac
                stacNode = await translatorManager.Translate<StacNode>(node);
                if (stacNode == null)
                    throw new InvalidDataException(string.Format("Impossible to translate node {0} into STAC.", node.Uri));
            }

            stacNode = await storeService.StoreNodeAtDestination(stacNode, importedAssets, destination, null);

            operationState.CurrentStacObject = stacNode;

            // 2. Apply processing services if any
            ProcessingService processingService = ServiceProvider.GetService<ProcessingService>();
            stacNode = await processingService.ExecuteAsync(stacNode, destination, storeService);

            return operationState;
        }

        private IEnumerator<ISupplier> InitSuppliersEnumerator(IResource route, SupplierFilters filters)
        {
            if (route is IItem)
                return supplierManager.GetSuppliers(filters).GetEnumerator();

            return new ISupplier[1] { new NativeSupplier(carrierManager) }.ToList().GetEnumerator();
        }

        protected override async Task ExecuteAsync()
        {
            this.routingService = ServiceProvider.GetService<RouterService>();
            this.carrierManager = ServiceProvider.GetService<CarrierManager>();
            this.processingManager = ServiceProvider.GetService<ProcessingManager>();
            this.storeService = ServiceProvider.GetService<StacStoreService>();
            this.translatorManager = ServiceProvider.GetService<TranslatorManager>();
            this.supplierManager = ServiceProvider.GetService<SupplierManager>();
            await this.storeService.Init(!AppendCatalog);
            InitRoutingTask();
            PrepareNewRoute(null, storeService.RootCatalogNode, null, null);
            List<IResource> routes = Inputs.Select(input => (IResource)WebRoute.Create(new Uri(input), credentials: ServiceProvider.GetService<ICredentials>())).ToList();
            List<StacNode> stacNodes = new List<StacNode>();
            foreach (var route in routes)
            {
                CopyOperationState state = PrepareNewRoute(null, route, null, null);
                state = await routingService.Route(route, recursivity, null, (object)state) as CopyOperationState;
                CopyOperationState copyState = state as CopyOperationState;
                stacNodes.Add(copyState.CurrentStacObject);
            }
            if (stacNodes.Count == 1 && stacNodes.First().IsCatalog)
                await storeService.UpdateRootCatalogWithNodes(stacNodes.First().GetRoutes().Cast<StacNode>());
        }

        protected override void RegisterOperationServices(ServiceCollection collection)
        {
            Uri outputUrl = null;
            try { outputUrl = new Uri(output + "/"); }
            catch
            {
                outputUrl = new Uri(Path.GetFullPath(output) + "/");
            }
            collection.ConfigureAll<StacStoreConfiguration>(so =>
            {
                so.RootCatalogue = new StacCatalogueConfiguration()
                {
                    Identifier = "catalog",
                    Description = "Root catalog",
                    Uri = new Uri(outputUrl, "catalog.json"),
                    DestinationUri = outputUrl
                };
            });
            collection.ConfigureAll<ExtractArchiveOptions>(so =>
            {
                so.KeepArchive = !DeleteArchive;
            });
            collection.AddTransient<ITranslator, DefaultStacTranslator>();
            collection.AddSingleton<StacStoreService, StacStoreService>();
            if (AllowOrdering)
                collection.AddTransient<ICarrier, OrderingCarrier>();
            if (!ExtractArchives)
            {
                collection.RemoveAll<ExtractArchiveAction>();
            }
        }
    }
}