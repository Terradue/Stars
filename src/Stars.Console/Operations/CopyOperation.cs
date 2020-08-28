using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Stars.Interface.Router;
using Stars.Interface.Supply;
using Stars.Interface.Supply.Destination;
using Stars.Router;
using Stars.Supply;
using Stars.Supply.Asset;
using Stars.Supply.Destination;

namespace Stars.Operations
{
    internal class CopyOperation : IOperation
    {
        private readonly IConsole console;
        private readonly IReporter reporter;
        private readonly ResourceRoutersManager routersManager;
        private readonly SupplierManager suppliersManager;
        private readonly DestinationManager destinationManager;
        private readonly CommandLineApplication copy;

        public CopyOperation(IServiceProvider serviceProvider)
        {
            this.console = serviceProvider.GetService<IConsole>();
            this.reporter = serviceProvider.GetService<IReporter>();
            this.routersManager = serviceProvider.GetService<ResourceRoutersManager>();
            this.suppliersManager = serviceProvider.GetService<SupplierManager>();
            this.destinationManager = serviceProvider.GetService<DestinationManager>();
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

            IResource resource = null;
            Exception exception = null;

            // if recursivity threshold is not reached
            if (recursivity > 0)
            {
                // let's keep going -> follow the route to the resource
                try
                {
                    resource = await route.GotoResource();
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
                reporter.Warn(String.Format("[{0}] {1} ({2}): {3}", prevRouter.Label, route.Uri, route.ContentType, exception.Message));
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
                routableResource = await router.Go(resource);
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

        private async Task<IRoute> CopyResource(IResource resource, IDestination destination)
        {
            reporter.Verbose(String.Format("{0} -> {1}", resource.Uri, destination.Uri));
            // List all possible resource supply
            IEnumerable<(ISupplier, IResource)> possible_supplies = await ListResourceSupplier(resource, destination);

            // Ask for quotation to all carriers
            //            

            // TEMP debug quotation mechanism
            int i = 1;
            foreach (var supply in possible_supplies)
            {
                IDeliveryQuotation supply_quotation = supply.Item1.QuoteDelivery(supply.Item2, destination);

                reporter.Verbose(string.Format("Supplier #{0} {1} Quotation for {2} routes", i, supply.Item1.Id, supply_quotation.DeliveryQuotes.Count));

                foreach (var route in supply_quotation.DeliveryQuotes)
                {
                    reporter.Verbose(string.Format("  Route {0} : {1} carriers", route.Key.Uri.ToString(), route.Value.Count()));
                    int j = 1;
                    foreach (var delivery in route.Value)
                    {
                        reporter.Verbose(string.Format("    Delivery #{0} by carrier {1} to {3} : {2}$", j, delivery.Carrier.Id, delivery.Cost, delivery.Destination.Uri.ToString()));
                        j++;
                    }
                }

                i++;

            }
            return null;

        }

        private async Task<IEnumerable<(ISupplier, IResource)>> ListResourceSupplier(IResource resource, IDestination destination)
        {
            IEnumerable<ISupplier> possible_suppliers = suppliersManager.GetSuppliers(resource);

            List<(ISupplier, IResource)> possible_deliveries = new List<(ISupplier, IResource)>();

            foreach (var supplier in possible_suppliers)
            {
                IEnumerable<IResource> possible_resources = await supplier.LocalizeResource(resource);
                foreach (var possible_resource in possible_resources)
                {
                    possible_deliveries.Add((supplier, possible_resource));
                }
            }

            return possible_deliveries;
        }
    }
}