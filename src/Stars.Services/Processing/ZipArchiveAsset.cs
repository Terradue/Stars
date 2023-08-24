// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: ZipArchiveAsset.cs

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ionic.Zip;
using Microsoft.Extensions.Logging;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Router;
using Terradue.Stars.Services.Supplier;
using Terradue.Stars.Services.Supplier.Carrier;
using Terradue.Stars.Services.Supplier.Destination;

namespace Terradue.Stars.Services.Processing
{
    public class ZipArchiveAsset : Archive
    {
        private ZipFile zipFile;
        private readonly IAsset asset;
        private readonly ILogger logger;
        private readonly IResourceServiceProvider resourceServiceProvider;
        private IStreamResource localStreamable;
        private readonly IFileSystem fileSystem;

        public ZipArchiveAsset(IAsset asset,
                               ILogger logger,
                               IResourceServiceProvider resourceServiceProvider,
                               IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
            this.asset = asset;
            this.logger = logger;
            this.resourceServiceProvider = resourceServiceProvider;
        }

        protected async Task<Stream> GetZipStreamAsync(IAsset asset, CarrierManager carrierManager, CancellationToken ct)
        {
            if (asset.Uri.Scheme == "file")
            {
                return await (await resourceServiceProvider.GetStreamResourceAsync(asset, ct)).GetStreamAsync(ct);
            }
            var tmpDestination = LocalFileDestination.Create(fileSystem.Directory.CreateDirectory(Path.GetTempPath()), asset);
            var tmpArchiveAssetDestination = tmpDestination.To(asset, Guid.NewGuid().ToString());
            tmpArchiveAssetDestination.PrepareDestination();
            var localZipDelivery = carrierManager.GetSingleDeliveryQuotations(asset, tmpArchiveAssetDestination).First();
            localStreamable = await localZipDelivery.Carrier.DeliverAsync(localZipDelivery, ct) as LocalFileResource;
            return await localStreamable.GetStreamAsync(ct);
        }

        public IReadOnlyDictionary<string, IAsset> Assets
        {
            get
            {
                Dictionary<string, IAsset> assets = new Dictionary<string, IAsset>();
                if (zipFile == null) return assets;
                foreach (ZipEntry entry in zipFile)
                {
                    if (entry.IsDirectory) continue;
                    assets.Add(entry.FileName, new ZipEntryAsset(entry, zipFile, asset, ParentAssetBaseDir));
                }
                return assets;
            }
        }

        public override Uri Uri => asset.Uri;

        // If set to true, detects relative location of parent/generic asset
        // and adjusts included assets' filenames with that directory
        public bool UseParentAssetBaseDir
        {
            get
            {
                return (ParentAssetBaseDir != null);
            }

            set
            {
                if (value)
                {
                    string parentAssetFileName = asset?.ContentDisposition?.FileName;
                    if (parentAssetFileName == null)
                    {
                        ParentAssetBaseDir = null;
                    }
                    else
                    {
                        string dirName = Path.GetDirectoryName(parentAssetFileName);
                        ParentAssetBaseDir = string.IsNullOrEmpty(dirName) ? null : dirName;
                    }
                }
                else
                {
                    ParentAssetBaseDir = null;
                }
            }
        }

        // Directory where ZIP file is located
        // (value is set automatically if UseParentAssetBaseDir is set to true)
        public string ParentAssetBaseDir { get; set; }

        public string AutodetectSubfolder()
        {
            List<string> names = new List<string>();
            foreach (ZipEntry entry in zipFile)
            {
                names.Add(entry.FileName);
            }
            var commonfolder = Findstem(names.ToArray());
            if (commonfolder.IndexOf('/') > 1)
                return "";
            return Path.GetFileNameWithoutExtension(asset.Uri.ToString());
        }

        public override async Task<IAssetsContainer> ExtractToDestinationAsync(IDestination destination, CarrierManager carrierManager, CancellationToken ct)
        {
            Dictionary<string, IAsset> assetsExtracted = new Dictionary<string, IAsset>();
            zipFile = ZipFile.Read(await GetZipStreamAsync(asset, carrierManager, ct));
            string subFolder = AutodetectSubfolder();

            foreach (var archiveAsset in Assets)
            {
                var archiveAssetDestination = destination.To(archiveAsset.Value, subFolder);
                archiveAssetDestination.PrepareDestination();
                var assetDeliveries = carrierManager.GetSingleDeliveryQuotations(archiveAsset.Value, archiveAssetDestination);
                logger.LogDebug(archiveAsset.Key);
                foreach (var delivery in assetDeliveries)
                {
                    var assetExtracted = await delivery.Carrier.DeliverAsync(delivery, ct);
                    if (assetExtracted != null)
                    {
                        var extractedAsset = new GenericAsset(assetExtracted, archiveAsset.Value.Title, archiveAsset.Value.Roles);
                        if (delivery.Resource is IAsset)
                        {
                            extractedAsset.MergeProperties((delivery.Resource as IAsset).Properties);
                        }
                        assetsExtracted.Add(asset.ContentDisposition.FileName + "!" + archiveAsset.Key, extractedAsset);
                        break;
                    }
                }
            }
            DisposeLocalStreamable();
            return new GenericAssetContainer(this, assetsExtracted);
        }

        private void DisposeLocalStreamable()
        {
            if (localStreamable != null)
                File.Delete(localStreamable.Uri.LocalPath);
            localStreamable = null;
        }
    }
}
