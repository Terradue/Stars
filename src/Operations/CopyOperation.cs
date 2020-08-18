using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Stars.Router;
using Stars.Supplier.Asset;
using Stars.Supplier.Catalog;

namespace Stars.Operations
{
    internal class CopyOperation : IOperation
    {
        private readonly IConsole console;
        private readonly IReporter reporter;
        private readonly ResourceRoutersManager routersManager;
        private readonly LocalCatalogGeneratorManager catGenManager;
        private readonly CommandLineApplication copy;

        public CopyOperation(IConsole console, IReporter reporter, ResourceRoutersManager routersManager, LocalCatalogGeneratorManager catGenManager, CommandLineApplication app)
        {
            this.console = console;
            this.reporter = reporter;
            this.routersManager = routersManager;
            this.catGenManager = catGenManager;
            this.copy = app.Commands.FirstOrDefault(c => c.Name == "list");
        }

        public async Task ExecuteAsync()
        {
            var inputs = copy.GetOptions().Where(o => o.ShortName == "i").SelectMany(o => o.Values);

            foreach (var input in inputs)
            {
                IRoute initRoute = WebRoute.Create(new Uri(input));
                await Copy(initRoute, 1, new DirectoryInfo("/tmp/stars"));
            }
        }

        internal async Task Copy(IRoute route, int recursivity, DirectoryInfo outputDirectory)
        {
            // Stop here
            if (recursivity == 0) return;

            // Let's see if there is a router for the route
            //IRouter router = routersManager.GetRouter(route);
            IResource resource = await route.GotoResource();

            // No resource -> Stop!
            if (resource == null) return;

            IRoutable routableResource = null;
            // If resource is not routable
            if (!(resource is IRoutable))
            {
                IRouter router = routersManager.GetRouter(resource);
                // if (router == null && resource.)
                // {
                //     await console.Out.WriteLineAsync(String.Format("{0,-100} {1,50}", (prefix + resource.Uri).Truncate(99), resource.ContentType));
                //     return;
                // }
                // await PrintRoute(await router.Go(resource), recursivity, prefix);
                return;
            }

            // Ok a resource. Let's copy it
            await CopyResource(resource, outputDirectory);

            // No more routes -> Stop
            if (!(resource is IRoutable)) return;

            routableResource = (IRoutable)resource;

            IEnumerable<IRoute> subroutes = routableResource.GetRoutes();
            foreach (IRoute subroute in subroutes)
            {
                await Copy(subroute, recursivity - 1, new DirectoryInfo(Path.Combine(outputDirectory.FullName, routableResource.Id)));
            }
        }

        private async Task CopyResource(IResource resource, DirectoryInfo outputDirectory)
        {
            // Create Directory if it does not exist
            if (!outputDirectory.Exists) outputDirectory.Create();

            // Copy the resource prefixed
            using (FileStream fileStream = File.Create(Path.Combine(outputDirectory.FullName, resource.Filename)))
            {
                await resource.GetAsStream().CopyToAsync(fileStream);
            }

        }

        private async Task CopyLocalCatalogResource(IResource resource, IEnumerable<IResource> children, IEnumerable<IAsset> assets, DirectoryInfo outputDirectory)
        {

            // 1. Generate the local catalog or collection
            IResource localResource = GenerateLocalCatalog(resource, children, assets);

            // 3. Copy it in the folder
            using (FileStream fileStream = File.Create(Path.Combine(outputDirectory.FullName, resource.Filename)))
            {
                await localResource.GetAsStream().CopyToAsync(fileStream);
            }
        }

        private IResource GenerateLocalCatalog(IResource resource, IEnumerable<IResource> children, IEnumerable<IAsset> assets)
        {
            // TODO Get the type of the local catalog from the application arguments
            // Using STAC by default

            ILocalCatalogGenerator catGen = catGenManager.GetLocalCatalogGenerator("stac");

            IResource localResource = catGen.LocalizeResource(resource, children, assets);

            return localResource;

        }
    }
}