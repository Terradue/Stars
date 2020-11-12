using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Router.Translator;
using Terradue.Stars.Interface.Supplier;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Services.Router;
using Terradue.Stars.Services.Processing;
using Terradue.Stars.Services.Supplier.Carrier;
using Terradue.Stars.Services.Supplier.Destination;
using Terradue.Stars.Services.Translator;
using Terradue.Stars.Services.Supplier;
using Terradue.Stars.Interface.Processing;
using Terradue.Stars.Interface;
using Terradue.Stars.Services;
using Terradue.Stars.Services.Store;
using Microsoft.Extensions.Options;
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

        [Option("-sa|--skip-assets", "Do not list assets", CommandOptionType.NoValue)]
        public bool SkippAssets { get; set; }

        [Option("-o|--output", "Output Url (Default to current dir)", CommandOptionType.SingleValue)]
        public string OutputUrl { get => outputUrl; set => outputUrl = value; }

        [Option("-ao|--allow-ordering", "Allow ordering assets", CommandOptionType.NoValue)]
        public bool AllowOrdering { get; set; }

        [Option("-xa|--extract-archive", "Extract archive files (default to true)", CommandOptionType.SingleValue)]
        public bool ExtractArchives { get; set; } = true;

        [Option("--stop-on-error", "Stop on Error (copy) (default to false)", CommandOptionType.NoValue)]
        public bool StopOnError { get; set; } = false;

        [Option("-si|--supplier-included", "Supplier to include for the data supply (default to all registered)", CommandOptionType.MultipleValue)]
        public string[] SuppliersIncluded { get; set; }

        [Option("-mc|--merge-catalog", "Merge catalog if one already exists", CommandOptionType.NoValue)]
        public bool MergeCatalog { get; set; } = false;


        private RouterService routingService;
        private CarrierManager carrierManager;
        private ProcessingManager processingManager;
        private TranslatorManager translatorManager;
        private StoreService storeService;
        private string[] inputs = new string[0];
        private int recursivity = 1;
        private string outputUrl = "file://" + Directory.GetCurrentDirectory();

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

            // 1. Import node and its assets via suppliers
            AssetService assetService = ServiceProvider.GetService<AssetService>();
            SupplyParameters supplyParameters = new SupplyParameters()
            {
                ContinueOnDeliveryError = !StopOnError

            };
            if (SuppliersIncluded != null && SuppliersIncluded.Count() > 0)
            {
                supplyParameters.SupplierFilters = new SupplierFilters();
                supplyParameters.SupplierFilters.IncludeIds = SuppliersIncluded;
            }
            StacNode stacNode = await assetService.ImportToStore(node, operationState.StoreService, destination, supplyParameters);

            operationState.CurrentStacObject = stacNode;

            // 2. Apply processing services if any
            ProcessingService processingService = ServiceProvider.GetService<ProcessingService>();
            stacNode = await processingService.ExecuteAsync(stacNode, destination);

            return operationState;
        }

        protected override async Task ExecuteAsync()
        {
            this.routingService = ServiceProvider.GetService<RouterService>();
            this.carrierManager = ServiceProvider.GetService<CarrierManager>();
            this.processingManager = ServiceProvider.GetService<ProcessingManager>();
            this.storeService = ServiceProvider.GetService<StoreService>();
            this.translatorManager = ServiceProvider.GetService<TranslatorManager>();
            await this.storeService.Init(!MergeCatalog);
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
            if ( stacNodes.Count == 1 && stacNodes.First().IsCatalog )
                await storeService.UpdateRootCatalogWithNodes(stacNodes.First().GetRoutes().Cast<StacNode>());
        }

        protected override void RegisterOperationServices(ServiceCollection collection)
        {
            collection.ConfigureAll<StoreOptions>(so =>
            {
                so.RootCatalogue = new CatalogueConfiguration()
                {
                    Identifier = "catalog",
                    Description = "Root catalog",
                    Url = string.Format("{0}/catalog.json", OutputUrl),
                    DestinationUrl = OutputUrl
                };
            });
            collection.AddTransient<ITranslator, DefaultStacTranslator>();
            collection.AddTransient<StoreService, StoreService>();
            if (AllowOrdering)
                collection.AddTransient<ICarrier, OrderingCarrier>();
            if (ExtractArchives)
            {
                collection.AddTransient<IProcessing, ExtractArchiveAction>();
            }
        }
    }
}