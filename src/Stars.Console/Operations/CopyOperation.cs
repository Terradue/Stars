using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Router.Translator;
using Terradue.Stars.Interface.Supplier;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Services.Router;
using Terradue.Stars.Services.Processing;
using Terradue.Stars.Services.Supplier.Carrier;
using Terradue.Stars.Services.Translator;
using Terradue.Stars.Services.Supplier;
using Terradue.Stars.Interface.Processing;
using Terradue.Stars.Interface;
using Terradue.Stars.Services;
using Terradue.Stars.Services.Store;
using System.Net;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace Terradue.Stars.Console.Operations
{
    [Command(Name = "copy", Description = "Copy the tree of resources and assets by routing from the input catalog")]
    internal class CopyOperation : BaseOperation
    {
        [Argument(0)]
        public string[] Inputs { get => inputs; set => inputs = value; }

        [Option("-r|--recursivity", "Resource recursivity depth routing", CommandOptionType.SingleValue)]
        public int Recursivity { get => recursivity; set => recursivity = value; }

        [Option("-sa|--skip-assets", "Do not copy assets", CommandOptionType.NoValue)]
        public bool SkippAssets { get; set; }

        [Option("-o|--output", "Output Url (Default to current dir)", CommandOptionType.SingleValue)]
        public string Output { get => output; set => output = value; }

        [Option("-ao|--allow-ordering", "Allow ordering assets", CommandOptionType.NoValue)]
        public bool AllowOrdering { get; set; }

        [Option("-xa|--extract-archive", "Extract archive files (default to true)", CommandOptionType.SingleValue)]
        public bool ExtractArchives { get; set; } = true;

        [Option("--stop-on-error", "Stop on Error (copy) (default to false)", CommandOptionType.NoValue)]
        public bool StopOnError { get; set; } = false;

        [Option("-si|--supplier-included", "Supplier to include for the data supply (default to all registered)", CommandOptionType.MultipleValue)]
        public string[] SuppliersIncluded { get; set; }

        [Option("-se|--supplier-excluded", "Supplier to exclude for the data supply (default to none)", CommandOptionType.MultipleValue)]
        public string[] SuppliersExcluded { get; set; }

        [Option("-ac|--append-catalog", "Append to existing catalog if one is found", CommandOptionType.NoValue)]
        public bool AppendCatalog { get; set; } = false;

        [Option("-da|--delete-archives", "Delete archives from the catalog when inflated", CommandOptionType.NoValue)]
        public bool DeleteArchive { get; set; } = false;

        [Option("-rel|--relative", "Make all links relative (and self links removed)", CommandOptionType.NoValue)]
        public bool AllRelative { get; set; } = false;

        [Option("-h|--harvest", "Make the assets harvesting if missing metadata", CommandOptionType.NoValue)]
        public bool Harvest { get; set; } = false;

        [Option("-aa|--absolute-assets", "Make the assets urls absolute", CommandOptionType.NoValue)]
        public bool AbsoluteAssets { get; set; } = false;

        [Option("-res|--result-file", "Write the copy result in a file", CommandOptionType.SingleValue)]
        public string ResultFile { get; set; }

        [Option("-af|--asset-filter", "Asset filters to match to be included in the copy (default to all)", CommandOptionType.MultipleValue)]
        public string[] AssetsFilters { get; set; }

        [Option("--empty", "Empty argument", CommandOptionType.NoValue)]
        public bool Empty { get; set; }

        private RouterService routingService;
        private CarrierManager carrierManager;
        private ProcessingManager processingManager;
        private TranslatorManager translatorManager;
        private SupplierManager supplierManager;
        private StacStoreService storeService;
        private StacLinkTranslator stacLinkTranslator;
        private string[] inputs = new string[0];
        private int recursivity = 1;
        private string output = "file://" + Directory.GetCurrentDirectory();

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
                stacCatalogNode.StacCatalog.Links.Clear();
                stacCatalogNode.StacCatalog.UpdateLinks(subStates.Select(ss => (ss as CopyOperationState).CurrentStacObject));
                operationState.CurrentStacObject = await operationState.StoreService.StoreCatalogNodeAtDestination(stacCatalogNode, operationState.CurrentDestination);
            }
            return operationState;
        }

        private CopyOperationState PrepareNewRoute(IResource parentRoute, IResource newRoute, IEnumerable<IResource> siblings, object state)
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

            StacNode stacNode = node as StacNode;
            if (stacNode == null)
            {
                // No? Let's try to translate it to Stac
                stacNode = await translatorManager.Translate<StacNode>(node);
                if (stacNode == null)
                    throw new InvalidDataException(string.Format("Impossible to translate node {0} into STAC.", node.Uri));
            }

            logger.Output(string.Format("Copy node {0} from {1}", stacNode.Id, node.Uri));

            // We update the destination in case a new router updated the route
            IDestination destination = operationState.CurrentDestination.To(stacNode);

            // In case of a Item
            if (node is IItem)
            {
                StacItemNode stacItemNode = stacNode as StacItemNode;

                // 1. Select Suppliers
                AssetService assetService = ServiceProvider.GetService<AssetService>();

                SupplierFilters supplierFilters = new SupplierFilters();

                if (SuppliersIncluded != null && SuppliersIncluded.Count() > 0)
                {
                    supplierFilters = new SupplierFilters();
                    supplierFilters.IncludeIds = SuppliersIncluded;
                }

                if (SuppliersExcluded != null && SuppliersExcluded.Count() > 0)
                {
                    supplierFilters = new SupplierFilters();
                    supplierFilters.ExcludeIds = SuppliersExcluded;
                }

                IItem sourceItemNode = await stacLinkTranslator.Translate<StacItemNode>(node);
                if (sourceItemNode == null)
                {
                    sourceItemNode = node as IItem;
                }
                else
                {
                    logger.Output(string.Format("alternate STAC source found at {0}", sourceItemNode.Uri));
                    stacItemNode = sourceItemNode as StacItemNode;
                }

                IEnumerator<ISupplier> suppliers = InitSuppliersEnumerator(sourceItemNode, supplierFilters);

                // 2. Try each of them until one provide the resource
                while (suppliers.MoveNext())
                {
                    IResource supplierNode = null;
                    try
                    {
                        logger.Output(string.Format("[{0}] Searching for {1}", suppliers.Current.Id, sourceItemNode.Uri.ToString()));
                        supplierNode = await suppliers.Current.SearchFor(sourceItemNode);
                        if (supplierNode == null && !(supplierNode is IAssetsContainer))
                        {
                            logger.Output(string.Format("[{0}] --> no supply possible", suppliers.Current.Id));
                            continue;
                        }
                        logger.Output(string.Format("[{0}] resource found at {1} [{2}]", suppliers.Current.Id, supplierNode.Uri, supplierNode.ContentType));
                    }
                    catch (Exception e)
                    {
                        logger.Warn(string.Format("[{0}] Exception searching for {1}: {2}", suppliers.Current.Id, sourceItemNode.Uri.ToString(), e.Message));
                        logger.Verbose(e.StackTrace);
                        continue;
                    }

                    if (!SkippAssets)
                    {
                        AssetFilters assetFilters = CreateAssetFiltersFromOptions();
                        AssetImportReport deliveryReport = await assetService.ImportAssets(supplierNode as IAssetsContainer, destination, assetFilters);
                        if (StopOnError && deliveryReport.AssetsExceptions.Count > 0)
                            throw new AggregateException(deliveryReport.AssetsExceptions.Values);

                        if (deliveryReport.ImportedAssets.Count() > 0)
                            stacItemNode.StacItem.MergeAssets(deliveryReport, true);
                        else continue;
                    }
                    break;
                }

                stacNode = await storeService.StoreItemNodeAtDestination(stacItemNode, destination);
            }
            else
            {
                stacNode = await storeService.StoreCatalogNodeAtDestination(stacNode as StacCatalogNode, destination);
            }

            operationState.CurrentStacObject = stacNode;

            // 2. Apply processing services if node was not stac originally
            if (node is IItem)
            {
                ProcessingService processingService = ServiceProvider.GetService<ProcessingService>();
                if (ExtractArchives)
                    stacNode = await processingService.ExtractArchive(stacNode as StacItemNode, destination, storeService);
                if (Harvest)
                    stacNode = await processingService.ExtractMetadata(stacNode as StacItemNode, destination, storeService);
            }

            return operationState;
        }

        private AssetFilters CreateAssetFiltersFromOptions()
        {
            AssetFilters assetFilters = new AssetFilters();
            if ( AssetsFilters == null )
                return assetFilters;
            Regex propertyRegex = new Regex(@"^\{(?'key'[\w:]*)\}(?'value'.*)$");
            foreach (var assetName in AssetsFilters)
            {
                Match propertyMatch = propertyRegex.Match(assetName);
                if (propertyMatch.Success)
                {
                    if (propertyMatch.Groups["key"].Value == "roles")
                    {
                        assetFilters.Add(new RolesAssetFilter(new Regex(propertyMatch.Groups["value"].Value)));
                        continue;
                    }
                    if (propertyMatch.Groups["key"].Value == "uri")
                    {
                        assetFilters.Add(new UriAssetFilter(new Regex(propertyMatch.Groups["value"].Value)));
                        continue;
                    }
                    assetFilters.Add(new PropertyAssetFilter(propertyMatch.Groups["key"].Value,
                        new Regex(propertyMatch.Groups["value"].Value)));
                    continue;
                }
                else
                {
                    assetFilters.Add(new KeyAssetFilter(new Regex("^" + assetName + "$")));
                }
            }
            return assetFilters;
        }

        private IEnumerator<ISupplier> InitSuppliersEnumerator(IResource route, SupplierFilters filters)
        {
            if (route is IItem)
                return supplierManager.GetSuppliers(filters).GetEnumerator();

            return new ISupplier[1] { new NativeSupplier(carrierManager) }.ToList().GetEnumerator();
        }

        protected override async Task ExecuteAsync()
        {
            this.routingService = ServiceProvider.GetService<RouterService>();
            this.carrierManager = ServiceProvider.GetService<CarrierManager>();
            this.processingManager = ServiceProvider.GetService<ProcessingManager>();
            this.storeService = ServiceProvider.GetService<StacStoreService>();
            this.translatorManager = ServiceProvider.GetService<TranslatorManager>();
            this.supplierManager = ServiceProvider.GetService<SupplierManager>();
            this.stacLinkTranslator = ServiceProvider.GetService<StacLinkTranslator>();
            await this.storeService.Init(!AppendCatalog);
            InitRoutingTask();
            PrepareNewRoute(null, storeService.RootCatalogNode, null, null);
            routingService.OnRoutingException((res, router, ex, state) => Task.FromResult(OnRoutingException(res, router, ex, state)));
            List<IResource> routes = null;
            try
            {
                routes = Inputs.Select(input => (IResource)WebRoute.Create(new Uri(input), credentials: ServiceProvider.GetService<ICredentials>())).ToList();
            }
            catch (Exception e)
            {
                logger.Error(string.Format("Exception creating initial routes [{0}] : {1}", string.Join(',', Inputs), e.Message));
                throw e;
            }
            List<StacNode> stacNodes = new List<StacNode>();
            foreach (var route in routes)
            {
                CopyOperationState state = PrepareNewRoute(null, route, null, null);
                state = await routingService.Route(route, recursivity, null, (object)state) as CopyOperationState;
                CopyOperationState copyState = state as CopyOperationState;
                stacNodes.Add(copyState.CurrentStacObject);
            }
            storeService.RootCatalogNode.StacCatalog.UpdateLinks(stacNodes.SelectMany<StacNode, IResource>(sn =>
            {
                if (sn is StacItemNode) return new IResource[] { sn };
                if (sn is StacCatalogNode) return sn.GetRoutes(ServiceProvider.GetService<ICredentials>());
                return new IResource[0];
            }));
            var rootCat = await storeService.StoreCatalogNodeAtDestination(storeService.RootCatalogNode, storeService.RootCatalogDestination);
            if (!string.IsNullOrEmpty(ResultFile))
            {
                Dictionary<string, string> results = new Dictionary<string, string>();
                results.Add("StacCatalogUri", rootCat.Uri.ToString());
                File.WriteAllText(ResultFile, JsonConvert.SerializeObject(results));
            }
        }

        private object OnRoutingException(IResource resource, IRouter router, Exception ex, object state)
        {
            if (StopOnError)
            {
                logger.Error(string.Format("Cannot route to resource at {0}. Aborting: {1}", resource.Uri, ex.Message));
                logger.Verbose(ex.StackTrace);
                throw ex;
            }
            logger.Warn(string.Format("Cannot route to resource at {0}. Skipping: {1}", resource.Uri, ex.Message));
            logger.Verbose(ex.StackTrace);
            return state;
        }

        protected override void RegisterOperationServices(ServiceCollection collection)
        {
            Uri outputUrl = null;
            try { outputUrl = new Uri(output.TrimEnd('/') + "/"); }
            catch
            {
                outputUrl = new Uri(Path.GetFullPath(output.TrimEnd('/')) + "/");
            }
            collection.ConfigureAll<StacStoreConfiguration>(so =>
            {
                so.RootCatalogue = new StacCatalogueConfiguration()
                {
                    Identifier = "catalog",
                    Description = "Root catalog",
                    Uri = new Uri(outputUrl, "catalog.json"),
                    DestinationUri = outputUrl
                };
                so.AllRelative = AllRelative;
                so.AbsoluteAssetsUrl = AbsoluteAssets;
            });
            collection.ConfigureAll<ExtractArchiveOptions>(so =>
            {
                so.KeepArchive = !DeleteArchive;
            });
            collection.AddSingleton<StacStoreService, StacStoreService>();
            collection.AddSingleton<StacLinkTranslator, StacLinkTranslator>();
            if (AllowOrdering)
                collection.AddTransient<ICarrier, OrderingCarrier>();

            if (ExtractArchives)
            {
                collection.AddTransient<IProcessing, ExtractArchiveAction>();
            }
        }
    }
}