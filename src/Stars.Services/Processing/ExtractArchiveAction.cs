using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Supplier.Carrier;
using Terradue.Stars.Services.Supplier.Destination;
using Microsoft.Extensions.Logging;
using Terradue.Stars.Services.Supplier;
using Terradue.Stars.Interface.Processing;
using Terradue.Stars.Interface;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.IO.Abstractions;
using System.Threading;

namespace Terradue.Stars.Services.Processing
{
    public class ExtractArchiveAction : IProcessing
    {

        private readonly DestinationManager destinationManager;
        private readonly CarrierManager carrierManager;
        private readonly IFileSystem fileSystem;
        private readonly IResourceServiceProvider resourceServiceProvider;
        private readonly ILogger logger;
        private readonly IOptions<ExtractArchiveOptions> options;

        public int Priority { get; set; }
        public string Key { get; set; }

        public ProcessingType ProcessingType => ProcessingType.ArchiveExtractor;

        public string Label => "Archive Extractor (ZIP, TAR GZ, GZIP)";

        public ExtractArchiveAction(IOptions<ExtractArchiveOptions> options,
                                    DestinationManager destinationManager,
                                    CarrierManager carrierManager,
                                    IFileSystem fileSystem,
                                    IResourceServiceProvider resourceServiceProvider,
                                    ILogger<ExtractArchiveAction> logger)
        {
            this.options = options;
            this.destinationManager = destinationManager;
            this.carrierManager = carrierManager;
            this.fileSystem = fileSystem;
            this.resourceServiceProvider = resourceServiceProvider;
            this.logger = logger;
            Key = "ExtractArchive";
            Priority = 1;
        }

        public bool CanProcess(IResource route, IDestination destination)
        {
            IAssetsContainer assetsContainer = route as IAssetsContainer;
            return assetsContainer != null && assetsContainer.Assets != null && assetsContainer.Assets.Any(asset =>
            {
                try
                {
                    return IsArchive(asset.Value);
                }
                catch
                {
                    return false;
                }
            });
        }

        private bool IsArchive(IAsset asset)
        {
            return IsArchiveContentType(asset) || IsArchiveFileNameExtension(asset);
        }

        private bool IsArchiveFileNameExtension(IAsset asset)
        {
            return Archive.ArchiveFileExtensions.Keys.Any(ext => asset.ContentDisposition != null && !string.IsNullOrEmpty(asset.ContentDisposition.FileName) && asset.ContentDisposition.FileName.EndsWith(ext, true, CultureInfo.InvariantCulture));
        }

        private bool IsArchiveContentType(IAsset asset)
        {
            return asset.ContentType != null && Archive.ArchiveContentTypes.Contains(asset.ContentType.MediaType);
        }

        public async Task<IResource> ProcessAsync(IResource route, IDestination destination, CancellationToken ct, string suffix = null)
        {
            IItem item = route as IItem;
            if (item == null) return route;
            IAssetsContainer assetsContainer = route as IAssetsContainer;
            Dictionary<string, IAsset> newAssets = new Dictionary<string, IAsset>();
            foreach (var asset in assetsContainer.Assets)
            {
                try
                {
                    if (!IsArchive(asset.Value))
                    {
                        newAssets.Add(asset.Key, asset.Value);
                        continue;
                    }
                }
                catch (Exception e)
                {
                    logger.LogWarning("Impossible to identify asset {0} as an archive: {1}", asset.Key, e.Message);
                    continue;
                }
                logger.LogInformation("Extracting asset {0}...", asset.Value.Uri);
                IAssetsContainer extractedAssets = await ExtractArchiveAsync(asset, destination, ct);
                int i = 0;
                foreach (var extractedAsset in extractedAssets.Assets)
                {
                    newAssets.Add(extractedAsset.Key, extractedAsset.Value);
                    i++;
                }
                if (options.Value.KeepArchive)
                {
                    newAssets.Add(asset.Key, asset.Value);
                }
            }

            if (newAssets == null || newAssets.Count == 0) return route;

            return new ContainerNode(route as IItem, newAssets, suffix);

        }

        private async Task<IAssetsContainer> ExtractArchiveAsync(KeyValuePair<string, IAsset> asset, IDestination destination, CancellationToken ct)
        {
            Archive archive = await Archive.Read(asset.Value, logger, resourceServiceProvider, fileSystem, ct);

            return await archive.ExtractToDestinationAsync(destination, carrierManager, ct);
        }

        public string GetRelativePath(IResource route, IDestination destination)
        {
            return null;
        }
    }
}