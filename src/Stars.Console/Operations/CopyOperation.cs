using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Router.Translator;
using Terradue.Stars.Interface.Supplier;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Catalog;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Services.Router;
using Terradue.Stars.Services.Processing;
using Terradue.Stars.Services.Supplier.Carrier;
using Terradue.Stars.Services.Supplier.Destination;
using Terradue.Stars.Services.Translator;
using Terradue.Stars.Services.Supplier;
using Terradue.Stars.Interface.Processing;

namespace Terradue.Stars.Operations
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

        [Option("-o|--output-dir", "Output Directory", CommandOptionType.SingleValue)]
        [Required]
        public string OutputDirectory { get; set; }

        [Option("-ao|--allow-ordering", "Allow ordering assets", CommandOptionType.NoValue)]
        public bool AllowOrdering { get; set; }

        [Option("-xa|--extract-archive", "Extract archive files (default to true)", CommandOptionType.NoValue)]
        public bool ExtractArchives { get; set; } = true;

        [Option("--stop-on-error", "Stop on Error (copy) (default to false)", CommandOptionType.NoValue)]
        public bool StopOnError { get; set; } = false;


        private RouterService routingTask;
        private DestinationManager destinationManager;
        private CarrierManager carrierManager;
        private ProcessingManager receiptManager;
        private string[] inputs = new string[0];
        private int recursivity = 1;

        public CopyOperation()
        {

        }
        private void InitRoutingTask()
        {
            routingTask.Parameters = new RouterServiceParameters()
            {
                Recursivity = Recursivity,
                SkipAssets = SkippAssets
            };
            // routingTask.OnRoutingToNodeException((route, router, exception, state) => PrintRouteInfo(route, router, exception, state));
            routingTask.OnBeforeBranching((node, router, state) => CopyNode(node, router, state));
            routingTask.OnItem((node, router, state) => CopyItem(node, router, state));
            routingTask.OnBranching(async (parentRoute, route, siblings, state) => await PrepareNewRoute(parentRoute, route, siblings, state));
            routingTask.OnAfterBranching(async (parentRoute, router, parentState, subStates) => await Stacify(parentRoute, router, parentState, subStates));
        }

        private async Task<object> CopyItem(IItem node, IRouter router, object state)
        {
            var newState = await CopyNode(node, router, state);

            CopyOperationState operationState = newState as CopyOperationState;

            if (operationState.LastRoute != null)
                return await Stacify(operationState.LastRoute, router, newState, new object[0]);

            return newState;
        }


        private async Task<object> Stacify(IRoute parentRoute, IRouter router, object state, IEnumerable<object> subStates)
        {
            CopyOperationState operationState = state as CopyOperationState;

            CoordinatorService catalogingTask = ServiceProvider.GetService<CoordinatorService>();

            IRoute stacRoute = await catalogingTask.ExecuteAsync(parentRoute, subStates.Cast<CopyOperationState>().Select(s => s.LastRoute), operationState.Destination, operationState.Depth);

            operationState.LastRoute = stacRoute;

            return operationState;

        }

        private async Task<object> PrepareNewRoute(IRoute parentRoute, IRoute newRoute, IList<IRoute> siblings, object state)
        {
            if (state == null)
            {
                var destination = await destinationManager.CreateDestination(OutputDirectory);
                if (destination == null)
                    throw new InvalidProgramException("No destination found for " + OutputDirectory);
                return new CopyOperationState(1, destination);
            }

            CopyOperationState operationState = state as CopyOperationState;
            if (operationState.Depth == 0) return state;

            var newDestination = operationState.Destination.RelativeTo(parentRoute, newRoute);

            return new CopyOperationState(operationState.Depth + 1, newDestination);
        }

        private async Task<object> CopyNode(IRoute node, IRouter router, object state)
        {
            CopyOperationState operationState = state as CopyOperationState;

            SupplierService supplyService = ServiceProvider.GetService<SupplierService>();

            supplyService.Parameters = new SupplierServiceParameters()
            {
                ContinueOnDeliveryError = !StopOnError
            };

            IRoute deliveryNode = await supplyService.ExecuteAsync(node, operationState.Destination);

            if (deliveryNode == null)
            {
                if (StopOnError)
                    throw new InvalidOperationException("[{0}] Delivery failed. Stopping");
                operationState.LastRoute = null;
            }
            else
            {
                ProcessingService processingService = ServiceProvider.GetService<ProcessingService>();
                deliveryNode = await processingService.ExecuteAsync(deliveryNode);

                operationState.LastRoute = deliveryNode;
            }

            return operationState;
        }

        protected override async Task ExecuteAsync()
        {
            this.routingTask = ServiceProvider.GetService<RouterService>();
            this.destinationManager = ServiceProvider.GetService<DestinationManager>();
            this.carrierManager = ServiceProvider.GetService<CarrierManager>();
            this.receiptManager = ServiceProvider.GetService<ProcessingManager>();
            InitRoutingTask();
            await routingTask.ExecuteAsync(Inputs);
        }

        protected override void RegisterOperationServices(ServiceCollection collection)
        {
            collection.AddTransient<ITranslator, DefaultStacTranslator>();
            if (AllowOrdering)
                collection.AddTransient<ICarrier, OrderingCarrier>();
            if (ExtractArchives){
                collection.AddTransient<IProcessing, ExtractArchiveAction>();
                ProcessingManager.PluginsPriority.Add(typeof(ExtractArchiveAction), 1);
            }
        }
    }
}