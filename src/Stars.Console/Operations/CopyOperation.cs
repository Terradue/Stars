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
using Terradue.Stars.Interface.Supply;
using Terradue.Stars.Interface.Supply.Destination;
using Terradue.Stars.Service.Catalog;
using Terradue.Stars.Service.Model.Stac;
using Terradue.Stars.Service.Router;
using Terradue.Stars.Service.Supply;
using Terradue.Stars.Service.Supply.Carrier;
using Terradue.Stars.Service.Supply.Destination;
using Terradue.Stars.Service.Supply.Receipt;
using Terradue.Stars.Service.Translator;

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


        private RoutingService routingTask;
        private DestinationManager destinationManager;
        private CarrierManager carrierManager;
        private ReceiptManager receiptManager;
        private string[] inputs = new string[0];
        private int recursivity = 1;

        public CopyOperation()
        {

        }
        private void InitRoutingTask()
        {
            routingTask.Parameters = new RoutingTaskParameters()
            {
                Recursivity = Recursivity,
                SkipAssets = SkippAssets
            };
            // routingTask.OnRoutingToNodeException((route, router, exception, state) => PrintRouteInfo(route, router, exception, state));
            routingTask.OnBranchingNode((node, router, state) => CopyNode(node, router, state));
            routingTask.OnLeafNode((node, router, state) => CopyLeafNode(node, router, state));
            routingTask.OnBranching(async (parentRoute, route, siblings, state) => await PrepareNewRoute(parentRoute, route, siblings, state));
            routingTask.OnAfterBranching(async (parentRoute, router, parentState, subStates) => await Stacify(parentRoute, router, parentState, subStates));
        }

        private async Task<object> CopyLeafNode(INode node, IRouter router, object state)
        {
            var newState = await CopyNode(node, router, state);

            CopyOperationState operationState = newState as CopyOperationState;

            return await Stacify(operationState.LastRoute, router, newState, new object[0]);
        }


        private async Task<object> Stacify(IRoute parentRoute, IRouter router, object state, IEnumerable<object> subStates)
        {
            CopyOperationState operationState = state as CopyOperationState;

            CatalogingService catalogingTask = ServiceProvider.GetService<CatalogingService>();

            IRoute stacRoute = await catalogingTask.ExecuteAsync(parentRoute, subStates.Cast<CopyOperationState>().Select(s => s.LastRoute), operationState.Assets, operationState.Destination, operationState.Depth);

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

        private async Task<object> CopyNode(INode node, IRouter router, object state)
        {
            CopyOperationState operationState = state as CopyOperationState;

            SupplyService supplyTask = ServiceProvider.GetService<SupplyService>();

            supplyTask.Parameters = new SupplyTaskParameters()
            {
                ContinueOnDeliveryError = !StopOnError
            };

            NodeInventory deliveryForm = await supplyTask.ExecuteAsync(node, operationState.Destination);

            if ( deliveryForm == null && StopOnError )
                throw new InvalidOperationException("[{0}] Delivery failed. Stopping");

            deliveryForm = await PostDelivery(deliveryForm);

            operationState.LastRoute = deliveryForm.Node;
            operationState.Assets = deliveryForm.Assets;

            return operationState;
        }

        private async Task<NodeInventory> PostDelivery(NodeInventory deliveryForm)
        {
            NodeInventory nodeInventory = deliveryForm;
            foreach (var receiver in receiptManager.Plugins)
            {
                if (!receiver.CanReceive(nodeInventory)) continue;
                nodeInventory = await receiver.Receive(nodeInventory);
            }
            return nodeInventory;
        }

        protected override async Task ExecuteAsync()
        {
            this.routingTask = ServiceProvider.GetService<RoutingService>();
            this.destinationManager = ServiceProvider.GetService<DestinationManager>();
            this.carrierManager = ServiceProvider.GetService<CarrierManager>();
            this.receiptManager = ServiceProvider.GetService<ReceiptManager>();
            InitRoutingTask();
            await routingTask.ExecuteAsync(Inputs);
        }

        protected override void RegisterOperationServices(ServiceCollection collection)
        {
            collection.AddTransient<ITranslator, DefaultStacTranslator>();
            if (AllowOrdering)
                collection.AddTransient<ICarrier, OrderingCarrier>();
            if (ExtractArchives)
                collection.AddTransient<IReceiptAction, ExtractArchiveAction>();
        }
    }
}