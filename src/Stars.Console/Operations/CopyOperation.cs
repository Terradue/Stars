// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: CopyOperation.cs

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Stac;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Processing;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Services.Plugins;
using Terradue.Stars.Services.Processing;
using Terradue.Stars.Services.Router;
using Terradue.Stars.Services.Store;
using Terradue.Stars.Services.Supplier;
using Terradue.Stars.Services.Supplier.Carrier;
using Terradue.Stars.Services.Translator;

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
        public bool SkipAssets { get; set; }

        [Option("-o|--output", "Output Url (Default to current dir)", CommandOptionType.SingleValue)]
        public string Output { get => output; set => output = value; }

        [Option("-ao|--allow-ordering", "Allow ordering assets", CommandOptionType.NoValue)]
        public bool AllowOrdering { get; set; } = false;

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

        [Option("-ka|--keep-all", "Keep all assets in the items when a processing is applied (e.g. default deletes original archive after extraction. may be overriden when using asset-filter-out)", CommandOptionType.NoValue)]
        public bool KeepAll { get; set; } = false;

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

        [Option("-koa|--keep-original-assets", "Keep original assets not included in the copy (default to false)", CommandOptionType.NoValue)]
        public bool KeepOriginalAssets { get; set; } = false;

        [Option("--nocopy-cog", "Add Asset filters in order to not copy Cloud Optimized Assets (imply asset filters on COG media types and keep original assets)", CommandOptionType.NoValue)]
        public bool NoCopyCog { get; set; } = false;

        [Option("-afo|--asset-filter-out", "Asset filters to match to be included in the Items after the extraction and harvesting (default to all)", CommandOptionType.MultipleValue)]
        public string[] AssetsFiltersOut { get; set; }

        [Option("-nohtml|--do-not-accept-html-asset", "Do not accept html assets delivery. Skip supply if HTML. (Do not apply when asset is clearly identified as html)", CommandOptionType.NoValue)]
        public bool NoHtmlAsset { get; set; } = false;

        [Option("--empty", "Empty argument", CommandOptionType.NoValue)]
        public bool Empty { get; set; }

        private RouterService routingService;
        private CarrierManager carrierManager;
        private ProcessingManager processingManager;
        private TranslatorManager translatorManager;
        private SupplierManager supplierManager;
        private StacStoreService storeService;
        private StacLinkTranslator stacLinkTranslator;
        private IResourceServiceProvider resourceServiceProvider;
        private string[] inputs = Array.Empty<string>();
        private int recursivity = 1;
        private string output = "file://" + Directory.GetCurrentDirectory();

        public CopyOperation(IConsole console) : base(console)
        {

        }
        private void InitRoutingTask()
        {
            routingService.Parameters = new RouterServiceParameters()
            {
                Recursivity = Recursivity,
                SkipAssets = SkipAssets
            };
            // routingTask.OnRoutingToNodeException((route, router, exception, state) => PrintRouteInfo(route, router, exception, state));
            routingService.OnBeforeBranching((node, router, state, subroutes, ct) => CreateCatalog(node, router, state, ct));
            routingService.OnItem((node, router, state, ct) => CopyNodeAsync(node, router, state, ct));
            routingService.OnBranching(async (parentRoute, route, siblings, state, ct) => await PrepareNewRouteAsync(parentRoute, route, siblings, state, ct));
            routingService.OnAfterBranching(async (parentRoute, router, parentState, subStates, ct) => await UpdateCatalog(parentRoute, router, parentState, subStates, ct));
        }

        private async Task<object> CreateCatalog(ICatalog node, IRouter router, object state, CancellationToken ct)
        {
            CopyOperationState operationState = state as CopyOperationState;
            StacNode stacNode = node as StacNode;
            if (stacNode == null)
            {
                // No? Let's try to translate it to Stac
                stacNode = await translatorManager.TranslateAsync<StacNode>(node, ct);
                if (stacNode == null)
                    throw new InvalidDataException(string.Format("Impossible to translate node {0} into STAC.", node.Uri));
            }
            operationState.CurrentStacObject = stacNode;
            return operationState;
        }

        private async Task<object> UpdateCatalog(ICatalog parentRoute, IRouter router, object parentState, IEnumerable<object> subStates, CancellationToken ct)
        {
            CopyOperationState operationState = parentState as CopyOperationState;
            if (operationState.CurrentStacObject is IItem)
                return operationState;
            if (operationState.CurrentStacObject is StacCatalogNode)
            {
                StacCatalogNode stacCatalogNode = operationState.CurrentStacObject as StacCatalogNode;
                stacCatalogNode.StacCatalog.Links.Clear();
                stacCatalogNode.StacCatalog.UpdateLinks(subStates.Select(ss => (ss as CopyOperationState).CurrentStacObject));
                operationState.CurrentStacObject = await operationState.StoreService.StoreCatalogNodeAtDestinationAsync(stacCatalogNode, operationState.CurrentDestination, ct);
            }
            return operationState;
        }

        private async Task<CopyOperationState> PrepareNewRouteAsync(IResource parentRoute, IResource newRoute, IEnumerable<IResource> siblings, object state, CancellationToken ct)
        {
            newRoute = await resourceServiceProvider.GetStreamResourceAsync(newRoute, ct);
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

        private async Task<object> CopyNodeAsync(IResource node, IRouter router, object state, CancellationToken ct)
        {
            CopyOperationState operationState = state as CopyOperationState;

            StacNode stacNode = node as StacNode;
            if (stacNode == null)
            {
                // No? Let's try to translate it to Stac
                stacNode = await translatorManager.TranslateAsync<StacNode>(node, ct);
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

                IItem sourceItemNode = await stacLinkTranslator.TranslateAsync<StacItemNode>(node, ct);
                if (sourceItemNode == null)
                {
                    sourceItemNode = node as IItem;
                }
                else
                {
                    logger.Output(string.Format("alternate STAC source found at {0}", sourceItemNode.Uri));
                    stacItemNode = sourceItemNode as StacItemNode;
                }

                PluginList<ISupplier> suppliers = InitSuppliersEnumerator(sourceItemNode, supplierFilters);

                // 2. Try each of them until one provide the resource
                foreach (var supplier in suppliers)
                {
                    IResource supplierNode = null;
                    try
                    {
                        logger.Output(string.Format("[{0}] Searching for {1}", supplier.Value.Id, sourceItemNode.Uri.ToString()));
                        supplierNode = await supplier.Value.SearchForAsync(sourceItemNode, ct);
                        if (supplierNode == null && supplierNode is not IAssetsContainer)
                        {
                            logger.Output(string.Format("[{0}] --> no supply possible", supplier.Value.Id));
                            continue;
                        }
                        logger.Output(string.Format("[{0}] resource found at {1} [{2}]", supplier.Value.Id, supplierNode.Uri, supplierNode.ContentType));
                    }
                    catch (Exception e)
                    {
                        logger.Warn(string.Format("[{0}] Exception searching for {1}: {2}", supplier.Value.Id, sourceItemNode.Uri.ToString(), e.Message));
                        logger.Verbose(e.StackTrace);
                        continue;
                    }

                    if (!SkipAssets)
                    {
                        AssetFilters assetFilters = AssetFilters.CreateAssetFilters(AssetsFilters);
                        if (NoCopyCog)
                        {
                            Dictionary<string, string> cogParameters = new Dictionary<string, string>();
                            cogParameters.Add("cloud-optimized", "true");
                            cogParameters.Add("profile", "cloud-optimized");
                            assetFilters.Add(new NotAssetFilter(new ContentTypeAssetFilter(null, cogParameters)));
                            KeepOriginalAssets = true;
                        }
                        AssetChecks assetChecks = GetAssetChecks();
                        AssetImportReport deliveryReport = await assetService.ImportAssetsAsync(supplierNode as IAssetsContainer, destination, assetFilters, assetChecks, ct);

                        // Process cases that must raise an exception
                        if (StopOnError)
                        {
                            // exception in delivery report
                            if (deliveryReport.AssetsExceptions.Count > 0)
                                throw new AggregateException(deliveryReport.AssetsExceptions.Values);
                            // no delivery but exception in quotation
                            if (deliveryReport.AssetsExceptions.Count == 0 && deliveryReport.Quotation.AssetsExceptions != null)
                                throw new AggregateException(deliveryReport.Quotation.AssetsExceptions.Values);
                        }

                        if (deliveryReport.ImportedAssets.Count() > 0)
                            stacItemNode.StacItem.MergeAssets(deliveryReport, !KeepOriginalAssets);
                        else continue;
                    }
                    else
                    {
                        stacItemNode.StacItem.MergeAssets(supplierNode as IAssetsContainer, false);
                    }
                    break;
                }

                stacNode = await storeService.StoreItemNodeAtDestinationAsync(stacItemNode, destination, ct);
            }
            else
            {
                stacNode = await storeService.StoreCatalogNodeAtDestinationAsync(stacNode as StacCatalogNode, destination, ct);
            }

            operationState.CurrentStacObject = stacNode;

            // 2. Apply processing services if node was not stac originally
            if (node is IItem)
            {
                ProcessingService processingService = ServiceProvider.GetService<ProcessingService>();
                processingService.Parameters.KeepOriginalAssets = KeepAll;
                if (ExtractArchives)
                    stacNode = await processingService.ExtractArchiveAsync(stacNode as StacItemNode, destination, storeService, ct);
                if (Harvest)
                    stacNode = await processingService.ExtractMetadataAsync(stacNode as StacItemNode, destination, storeService, ct);

                if (AssetsFiltersOut != null && AssetsFiltersOut.Count() > 0)
                {
                    AssetFilters assetFilters = AssetFilters.CreateAssetFilters(AssetsFiltersOut);
                    FilteredAssetContainer filteredAssetContainer = new FilteredAssetContainer(stacNode as IItem, assetFilters);
                    var assets = filteredAssetContainer.Assets.ToDictionary(a => a.Key, a => (a.Value as StacAssetAsset).StacAsset);
                    (stacNode as StacItemNode).StacItem.Assets.Clear();
                    (stacNode as StacItemNode).StacItem.Assets.AddRange(assets);
                    logger.Verbose(string.Format("{0} assets kept: {1}", assets.Count, string.Join(", ", assets.Keys)));
                    stacNode = await storeService.StoreItemNodeAtDestinationAsync(stacNode as StacItemNode, destination, ct);
                }
            }

            return operationState;
        }

        private AssetChecks GetAssetChecks()
        {
            if (NoHtmlAsset)
                return AssetChecks.NoHtml;

            return AssetChecks.None;
        }

        private PluginList<ISupplier> InitSuppliersEnumerator(IResource route, SupplierFilters filters)
        {
            if (route is IItem)
                return supplierManager.GetSuppliers(filters);

            return new PluginList<ISupplier>(new ISupplier[1] { new NativeSupplier(carrierManager) });
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            routingService = ServiceProvider.GetService<RouterService>();
            carrierManager = ServiceProvider.GetService<CarrierManager>();
            processingManager = ServiceProvider.GetService<ProcessingManager>();
            storeService = ServiceProvider.GetService<StacStoreService>();
            translatorManager = ServiceProvider.GetService<TranslatorManager>();
            supplierManager = ServiceProvider.GetService<SupplierManager>();
            stacLinkTranslator = ServiceProvider.GetService<StacLinkTranslator>();
            resourceServiceProvider = ServiceProvider.GetService<IResourceServiceProvider>();
            var stacRouter = ServiceProvider.GetService<StacRouter>();
            await storeService.InitAsync(ct, !AppendCatalog);
            InitRoutingTask();
            await PrepareNewRouteAsync(null, storeService.RootCatalogNode, null, null, ct);
            routingService.OnRoutingException((res, router, ex, state, ct) => Task.FromResult(OnRoutingException(res, router, ex, state, ct)));
            List<IResource> routes = null;
            try
            {
                var tasks = Inputs.Select(input => resourceServiceProvider.CreateStreamResourceAsync(new GenericResource(new Uri(input)), ct));
                routes = (await Task.WhenAll(tasks)).Cast<IResource>().ToList();
            }
            catch (Exception e)
            {
                logger.Error(string.Format("Exception creating initial routes [{0}] : {1}", string.Join(",", Inputs), e.Message));
                throw e;
            }
            List<StacNode> stacNodes = new List<StacNode>();
            foreach (var route in routes)
            {
                CopyOperationState state = await PrepareNewRouteAsync(null, route, null, null, ct);
                state = await routingService.RouteAsync(route, recursivity, null, (object)state, ct) as CopyOperationState;
                CopyOperationState copyState = state as CopyOperationState;
                stacNodes.Add(copyState.CurrentStacObject);
            }
            storeService.RootCatalogNode.StacCatalog.UpdateLinks(stacNodes.SelectMany(sn =>
            {
                if (sn is StacItemNode) return new IResource[] { sn };
                if (sn is StacCatalogNode) return sn.GetRoutes(stacRouter);
                return Array.Empty<IResource>();
            }));
            var rootCat = await storeService.StoreCatalogNodeAtDestinationAsync(storeService.RootCatalogNode, storeService.RootCatalogDestination, ct);
            if (!string.IsNullOrEmpty(ResultFile))
            {
                Dictionary<string, string> results = new Dictionary<string, string>();
                results.Add("StacCatalogUri", rootCat.Uri.ToString());
                File.WriteAllText(ResultFile, JsonConvert.SerializeObject(results));
            }
        }

        private object OnRoutingException(IResource resource, IRouter router, Exception ex, object state, CancellationToken ct)
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
                so.KeepArchive = KeepAll;
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
