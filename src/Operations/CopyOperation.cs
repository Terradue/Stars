using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Stars.Router;
using Stars.Supplier.Catalog;

namespace Stars.Operations
{
    internal class CopyOperation : IOperation
    {
        private readonly IConsole console;
        private readonly IReporter reporter;
        private readonly ResourceGrabber grabber;
        private readonly ResourceRoutersManager routerManager;
        private readonly LocalCatalogGeneratorManager catGenManager;

        public CopyOperation(IConsole console, IReporter reporter, ResourceGrabber grabber, ResourceRoutersManager routerManager, LocalCatalogGeneratorManager catGenManager)
        {
            this.console = console;
            this.reporter = reporter;
            this.grabber = grabber;
            this.routerManager = routerManager;
            this.catGenManager = catGenManager;
        }

        internal async Task ExecuteAsync(CommandLineApplication copy)
        {
            foreach (var input in copy.GetOptions().First(o => o.ShortName == "-i").Values)
            {
                IRoute initRoute = grabber.CreateRoute(input);
                await Copy(initRoute, 1, new DirectoryInfo("/tmp/stars"));
            }
        }

        internal async Task Copy(IRoute route, int recursivity, DirectoryInfo outputDirectory)
        {
            if (!route.CanGetResource || recursivity == 0)
            {
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
                    return;
                }
                routableResource = router.Route(resource);
            }
            else
            {
                routableResource = (IRoutable)resource;
            }

            if (routableResource.ResourceType == ResourceType.Catalog || routableResource.ResourceType == ResourceType.Collection)
                await CopyCatalogResource(routableResource, outputDirectory);

            if (recursivity == 0)
            {
                return;
            }

            IEnumerable<IRoute> subroutes = routableResource.GetRoutes();
            foreach (IRoute subroute in subroutes)
            {
                await Copy(subroute, recursivity - 1, new DirectoryInfo(Path.Combine(outputDirectory.FullName, routableResource.Id )));
            }
        }

        private async Task CopyCatalogResource(IResource resource, DirectoryInfo outputDirectory)
        {
            // Create Directory if it does not exist
            if (!outputDirectory.Exists) outputDirectory.Create();

            // 1. Copy the original
            using (FileStream fileStream = File.Create(Path.Combine(outputDirectory.FullName, "router." + resource.Filename)))
            {
                await resource.GetAsStream().CopyToAsync(fileStream);
            }

            // 2. Generate the local catalog
            IResource localResource = GenerateLocalCatalog(resource);

            // 3. Copy it in the folder
            using (FileStream fileStream = File.Create(Path.Combine(outputDirectory.FullName, resource.Filename)))
            {
                await localResource.GetAsStream().CopyToAsync(fileStream);
            }
        }

        private IResource GenerateLocalCatalog(IResource resource)
        {
            // TODO Get the type of the local catalog from the application arguments
            // Using STAC by default

            ILocalCatalogGenerator catGen = catGenManager.GetLocalCatalogGenerator("stac");

            IResource localResource = catGen.LocalizeResource(resource);

            return localResource;

        }
    }
}