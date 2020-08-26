using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Stars.Router;
using Stars.Supply.Asset;

namespace Stars.Operations
{
    internal class ListOperation : IOperation
    {
        private readonly IConsole console;
        private readonly IReporter reporter;
        private readonly ResourceRoutersManager routersManager;
        private readonly CommandLineApplication list;

        public ListOperation(IConsole console, IReporter reporter, ResourceRoutersManager routersManager, CommandLineApplication app)
        {
            this.console = console;
            this.reporter = reporter;
            this.routersManager = routersManager;
            this.list = app.Commands.FirstOrDefault(c => c.Name == "list");
        }

        public async Task ExecuteAsync()
        {
            var inputs = list.GetOptions().Where(o => o.ShortName == "i").SelectMany(o => o.Values);
            CommandOption<int> recursivityOption = (CommandOption<int>)list.GetOptions().FirstOrDefault(o => o.ShortName == "r");


            foreach (var input in inputs)
            {
                IRoute initRoute = WebRoute.Create(new Uri(input));
                await PrintRoute(initRoute, recursivityOption.HasValue() ? recursivityOption.ParsedValue : 1, "", null);
            }
        }

        internal async Task PrintRoute(IRoute route, int recursivity, string prefix, IRouter prevRouter)
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

            // Print the route info if resource cannot be reached
            if (exception != null)
            {
                string resourcePrefix1 = prefix;
                if (prevRouter != null)
                    resourcePrefix1 = string.Format("[{0}] {1}", prevRouter.Label, prefix);
                console.ForegroundColor = GetColorFromType(route.ResourceType);
                await console.Out.WriteAsync(String.Format("{0,-80} {1,40}", (resourcePrefix1 + route.Uri).Truncate(99), route.ContentType));
                console.ForegroundColor = ConsoleColor.Red;
                await console.Out.WriteLineAsync(String.Format(" -> {0}", exception.Message.Truncate(99)));
                return;
            }

            IRoutable routableResource = null;
            IRouter router = null;
            // If resource is not routable (there is no more native routes fom that resource)
            if (!(resource is IRoutable))
            {
                console.ForegroundColor = GetColorFromType(resource.ResourceType);
                // Print the information about the resource
                string resourcePrefix1 = prefix;
                if (prevRouter != null)
                    resourcePrefix1 = string.Format("[{0}] {1}", prevRouter.Label, prefix);
                await console.Out.WriteLineAsync(String.Format("{0,-80} {1,40}", (resourcePrefix1 + resource.Label).Truncate(99), resource.ContentType));
                // Ask the router manager if there is another router available for this resource
                router = routersManager.GetRouter(resource);
                // Definitively no more routes, print the resource info and return
                if (router == null)
                {
                    PrintAssets(resource, prevRouter, prefix);
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
            console.ForegroundColor = GetColorFromType(routableResource.ResourceType);

            // Print info about the routable resource
            // await console.Out.WriteLineAsync(String.Format("{0,-80} {1,40}", (prefix + routableResource.Uri).Truncate(99), routableResource.ContentType));
            string resourcePrefix = prefix;
            if (router != null)
                resourcePrefix = string.Format("[{0}] {1}", router.Label, prefix);
            await console.Out.WriteLineAsync(String.Format("{0,-80} {1,40}", (resourcePrefix + routableResource.Label).Truncate(99), routableResource.ContentType));

            // Let's get sub routes
            IEnumerable<IRoute> subroutes = routableResource.GetRoutes();
            for (int i = 0; i < subroutes.Count(); i++)
            {
                string newPrefix = prefix.Replace('─', ' ').Replace('└', ' ');
                if (i == subroutes.Count() - 1)
                {
                    newPrefix += '└';
                }
                else
                    newPrefix += '│';
                await PrintRoute(subroutes.ElementAt(i), recursivity - 1, newPrefix + new string('─', 2), router);
            }
            PrintAssets(resource, router, prefix);
        }

        private async void PrintAssets(IResource resource, IRouter router, string prefix)
        {
            // List assets
            if (!list.GetOptions().FirstOrDefault(o => o.ShortName == "sa").HasValue()
              && resource is IAssetsContainer)
            {
                IEnumerable<IAsset> assets = ((IAssetsContainer)resource).GetAssets();
                for (int i = 0; i < assets.Count(); i++)
                {
                    string newPrefix = prefix.Replace('─', ' ').Replace('└', ' ');
                    if (i == assets.Count() - 1)
                    {
                        newPrefix += '└';
                    }
                    else
                        newPrefix += '│';

                    console.ForegroundColor = ConsoleColor.DarkCyan;
                    var assetPrefix = newPrefix + new string('─', 2);
                    if (router != null)
                        assetPrefix = string.Format("[{0}] {1}", router.Label, assetPrefix);
                    var asset = assets.ElementAt(i);
                    await console.Out.WriteLineAsync(String.Format("{0,-80} {1,40}", (assetPrefix + asset.Label).Truncate(99), asset.ContentType));
                }
            }
        }

        private ConsoleColor GetColorFromType(ResourceType resourceType)
        {
            switch (resourceType)
            {
                case ResourceType.Catalog:
                    return ConsoleColor.Blue;
                case ResourceType.Collection:
                    return ConsoleColor.Green;
                case ResourceType.Item:
                    return ConsoleColor.Yellow;
                case ResourceType.Asset:
                    return ConsoleColor.DarkCyan;
            }
            return ConsoleColor.White;
        }
    }
}