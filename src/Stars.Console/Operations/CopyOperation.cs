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
using Stars.Interface.Model;
using Stars.Interface.Router;
using Stars.Interface.Supply;
using Stars.Interface.Supply.Destination;
using Stars.Service.Router;
using Stars.Service.Supply;
using Stars.Service.Supply.Asset;
using Stars.Service.Supply.Destination;

namespace Stars.Operations
{
    [Command(Name = "copy", Description = "Copy the tree of resources and assets by routing from the input reference")]
    internal class CopyOperation : IOperation
    {
        [Argument(0)]
        public string[] Inputs { get => inputs; set => inputs = value; }

        [Option("-r|--recursivity", "Resource recursivity depth routing", CommandOptionType.SingleValue)]
        public int Recursivity { get => recursivity; set => recursivity = value; }

        [Option("-sa|--skip-assets", "Do not list assets", CommandOptionType.NoValue)]
        public bool SkippAssets { get; set; }

        [Option("-o|--output_dir", "Output Directory", CommandOptionType.SingleValue)]
        public string OutputDirectory { get; set; }

        private readonly IConsole console;
        private readonly ILogger logger;
        private readonly RoutingTask routingTask;
        private readonly DestinationManager destinationManager;
        private readonly IServiceProvider serviceProvider;

        private string[] inputs = new string[0];
        private int recursivity = 1;

        public CopyOperation(IServiceProvider serviceProvider)
        {
            this.console = serviceProvider.GetService<IConsole>();
            this.logger = serviceProvider.GetService<ILogger>();
            this.routingTask = serviceProvider.GetService<RoutingTask>();
            this.destinationManager = serviceProvider.GetService<DestinationManager>();
            this.serviceProvider = serviceProvider;
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
            routingTask.OnLeafNode((node, router, state) => CopyNode(node, router, state));
            routingTask.OnBranching(async (parentRoute, route, siblings, state) => await PrepareNewRoute(parentRoute, route, siblings, state));
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

            var newDestination = operationState.Destination.RelativePath(parentRoute, newRoute);

            return new CopyOperationState(operationState.Depth + 1, newDestination);
        }

        private async Task<object> CopyNode(INode node, IRouter router, object state)
        {
            CopyOperationState operationState = state as CopyOperationState;

            SupplyingTask supplyTask = serviceProvider.GetService<SupplyingTask>();

            supplyTask.Parameters = new SupplyTaskParameters();

            await supplyTask.ExecuteAsync(node, operationState.Destination);

            return state;
        }


        public async Task OnExecuteAsync()
        {
            InitRoutingTask();
            await routingTask.ExecuteAsync(Inputs);
        }

        public ValidationResult OnValidate()
        {
            Program._logger.IsVerbose = Program.Verbose;
            return ValidationResult.Success;
        }

    }
}