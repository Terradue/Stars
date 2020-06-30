using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Stars.Router;

namespace Stars.Operations
{
    internal class ListOperation : IOperation
    {
        private readonly IConsole console;
        private readonly IReporter reporter;
        private readonly ResourceGrabber grabber;
        private readonly ResourceRoutersManager routerManager;

        public ListOperation(IConsole console, IReporter reporter, ResourceGrabber grabber, ResourceRoutersManager routerManager)
        {
            this.console = console;
            this.reporter = reporter;
            this.grabber = grabber;
            this.routerManager = routerManager;
        }

        internal async Task ExecuteAsync(List<string> inputs, int recursivity = 0)
        {
            foreach (var input in inputs)
            {
                IRoute initRoute = grabber.CreateRoute(input);
                await PrintRoute(initRoute, recursivity, "");
            }
        }

        internal async Task PrintRoute(IRoute route, int recursivity, string prefix)
        {
            console.ForegroundColor = GetColorFromType(route.ResourceType);
            if (!route.CanGetResource || recursivity == 0)
            {
                await console.Out.WriteLineAsync(String.Format("{0,-100} {1,50}", prefix + route.Uri, route.ContentType));
                
                return;
            }

            IResource resource = await route.GetResource();
            IRoutable routableResource = null;

            if (!(resource is IRoutable))
            {
                IResourceRouter router = null;
                router = routerManager.GetRouterForResource(resource);

                if (router == null)
                {
                    await console.Out.WriteLineAsync(String.Format("{0,-100} {1,50}", prefix + resource.Uri, resource.ContentType));
                    
                    return;
                }
                routableResource = router.Route(resource);
            }
            else
            {
                routableResource = (IRoutable)resource;
            }
            console.ForegroundColor = GetColorFromType(routableResource.ResourceType);

            await console.Out.WriteLineAsync(String.Format("{0,-100} {1,50}", prefix + routableResource.Uri, routableResource.ContentType));
            await console.Out.WriteLineAsync(String.Format("{0,-100}",  prefix.Replace('─', ' ').Replace('└', ' ') + routableResource.Label));

            if (recursivity == 0)
            {
                // foreach (var subroute in subroutes)
                //     await console.Out.WriteLineAsync(String.Format("{0}{1,-57} {2,20}", prefix, subroute.Uri, subroute.ContentType));
                return;
            }

            IEnumerable<IRoute> subroutes = routableResource.GetRoutes();
            for (int i = 0; i < subroutes.Count(); i++)
            {
                string newPrefix = prefix.Replace('─', ' ').Replace('└', ' ');
                if ( i == subroutes.Count() -1 ){
                    newPrefix += '└';
                }
                else
                    newPrefix += '│';
                await PrintRoute(subroutes.ElementAt(i), recursivity - 1, newPrefix + new string('─', 2));
            }
        }

        private ConsoleColor GetColorFromType(ResourceType resourceType)
        {
            switch (resourceType){
                case ResourceType.Catalog:
                    return ConsoleColor.Blue;
                case ResourceType.Collection:
                    return ConsoleColor.Green;
                case ResourceType.Item:
                    return ConsoleColor.Yellow;
                case ResourceType.Asset:
                    return ConsoleColor.Red;
            }
            return ConsoleColor.White;
        }
    }
}