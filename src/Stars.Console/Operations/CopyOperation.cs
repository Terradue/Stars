using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
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
    internal class CopyOperation : IOperation
    {
        private readonly IConsole console;
        private readonly IReporter reporter;
        private readonly ResourceRoutersManager routersManager;
        private readonly SupplierManager suppliersManager;
        private readonly DestinationManager destinationManager;
        private readonly TranslatorManager translatorManager;
        private readonly CommandLineApplication copy;

        public CopyOperation(IServiceProvider serviceProvider)
        {
            this.console = serviceProvider.GetService<IConsole>();
            this.reporter = serviceProvider.GetService<IReporter>();
            this.routersManager = serviceProvider.GetService<RoutersManager>();
            this.suppliersManager = serviceProvider.GetService<SupplierManager>();
            this.destinationManager = serviceProvider.GetService<DestinationManager>();
            this.translatorManager = serviceProvider.GetService<TranslatorManager>();
            var app = serviceProvider.GetService<CommandLineApplication>();
            this.copy = app.Commands.FirstOrDefault(c => c.Name == "copy");
        }

        public async Task ExecuteAsync()
        {
            var inputs = copy.GetOptions().Where(o => o.ShortName == "i").SelectMany(o => o.Values);
            var output = copy.GetOptions().FirstOrDefault(o => o.ShortName == "o");
            CommandOption<int> recursivityOption = (CommandOption<int>)copy.GetOptions().FirstOrDefault(o => o.ShortName == "r");

            IDestination destination = await destinationManager.CreateDestination(output.Value());
            if (destination == null)
            {
                throw new OperationCanceledException("No valid destination found for " + output);
            }

            foreach (var input in inputs)
            {
                IRoute initRoute = WebRoute.Create(new Uri(input));
                await Copy(initRoute, recursivityOption.HasValue() ? recursivityOption.ParsedValue : 1, destination, null);
            }
        }

        internal async Task Copy(IRoute route, int recursivity, IDestination destination, IRouter prevRouter)
        {
            // Stop here if there is no route
            if (route == null) return;

            INode resource = null;
            Exception exception = null;

            // if recursivity threshold is not reached
            if (recursivity > 0)
            {
                // let's keep going -> follow the route to the resource
                try
                {
                    resource = await route.GoToNode();
                    if (resource == null)
                        throw new NullReferenceException("Null");
                }
                catch (AggregateException ae)
                {
                    exception = ae.InnerException;
                }
                catch (Exception e)
                {
                    exception = e;
                }
            }
            else
            {
                exception = new Exception("Max Depth");
            }

            // Print warning route info + exception if resource cannot be reached
            if (exception != null)
            {
                reporter.Warn(String.Format("[{0}] {1} ({2}): {3}", prevRouter == null ? "none" : prevRouter.Label, route.Uri, route.ContentType, exception.Message));
                return;
            }

            IRoutable routableResource = null;
            IRouter router = null;
            // If resource is not routable (there is no more native routes fom that resource)
            if (!(resource is IRoutable))
            {
                // Print the information about the resource

                // Ask the router manager if there is another router available for this resource
                router = routersManager.GetRouter(resource);
                // Definitively no more routes, print the resource info and return
                if (router == null)
                {
                    await CopyResource(resource, destination);
                    return;
                }
                // New route!
                routableResource = await router.Route(resource);
            }
            else
            {
                // If the resource is natively routable
                routableResource = (IRoutable)resource;
                router = prevRouter;
            }

            // Print info about the routable resource
            // await console.Out.WriteLineAsync(String.Format("{0,-80} {1,40}", (prefix + routableResource.Uri).Truncate(99), routableResource.ContentType));
            await CopyResource(routableResource, destination);

            // Let's get sub routes
            IEnumerable<IRoute> subroutes = routableResource.GetRoutes();
            for (int i = 0; i < subroutes.Count(); i++)
            {
                var subroute = subroutes.ElementAt(i);
                await Copy(subroute, recursivity - 1, destination.RelativePath(route, subroute), router);
            }

        }

        private async Task<IRoute> CopyResource(INode node, IDestination destination)
        {
            reporter.Verbose(String.Format("{0} -> {1}", node.Uri, destination.Uri));
            // List all possible resource supply
            Dictionary<ISupplier, INode> possible_supplies = new Dictionary<ISupplier, INode>();
            var native_supplier = suppliersManager.GetDefaultSupplier();

            // 1. Get the resource native supplier 
            if (native_supplier != null)
                possible_supplies.Add(native_supplier, node);


            // 2. Search for other suppliers
            if (!node.IsCatalog)
            {
                // First, translate to stac
                var translations = await translatorManager.Translate(node);
                // Let's find suppliers
                foreach (var stacNode in translations.Values)
                {
                    foreach (var supply in await SearchForOtherSuppliers(stacNode))
                    {
                        // When a new supplier is found
                        if (!possible_supplies.ContainsKey(supply.Key))
                        {
                            possible_supplies.Add(supply.Key, supply.Value);
                        }
                    }
                    if (possible_supplies.Count > 0)
                        break;
                }
            }

            // TEMP debug quotation mechanism
            int i = 1;
            foreach (var supply in possible_supplies)
            {
                IDeliveryQuotation supply_quotation = supply.Key.QuoteDelivery(supply.Value, destination);
                if (supply_quotation == null)
                {
                    supply_quotation = native_supplier.QuoteDelivery(supply.Value, destination);
                    if (supply_quotation == null)
                    {
                        reporter.Output(string.Format("Supplier #{0} {1} no delivery possible", i, supply.Key.Id));
                        continue;
                    }
                }

                reporter.Output(string.Format("Supplier #{0} {1} Quotation for {2} routes", i, supply.Key.Id, supply_quotation.DeliveryQuotes.Count));

                foreach (var route in supply_quotation.DeliveryQuotes)
                {
                    reporter.Output(string.Format("  Route {0} : {1} carriers", route.Key.Uri.ToString(), route.Value.Count()));
                    int j = 1;
                    foreach (var delivery in route.Value)
                    {
                        reporter.Output(string.Format("    Delivery #{0} by carrier {1} to {3} : {2}$", j, delivery.Carrier.Id, delivery.Cost, delivery.Destination.Uri.ToString()));
                        j++;
                    }
                }

                i++;

            }
            return null;

        }

        private async Task<IDictionary<ISupplier, INode>> SearchForOtherSuppliers(IStacNode resource)
        {
            return await suppliersManager.GetSuppliers(resource);
        }
    }
}