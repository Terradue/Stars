// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: GzipArchive.cs

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.GZip;
using Microsoft.Extensions.Logging;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Supplier;
using Terradue.Stars.Services.Supplier.Carrier;

namespace Terradue.Stars.Services.Processing
{
    internal class GzipArchive : Archive
    {
        private readonly IAsset asset;
        private readonly ILogger logger;
        private readonly IResourceServiceProvider resourceServiceProvider;
        private readonly IFileSystem fileSystem;

        public GzipArchive(IAsset asset,
                           ILogger logger,
                           IResourceServiceProvider resourceServiceProvider,
                           IFileSystem fileSystem)
        {
            this.asset = asset;
            this.logger = logger;
            this.resourceServiceProvider = resourceServiceProvider;
            this.fileSystem = fileSystem;
        }

        public override Uri Uri => asset.Uri;

        protected async Task<Stream> GetStreamAsync(IAsset asset, CancellationToken ct)
        {
            var streamResource = await resourceServiceProvider.GetStreamResourceAsync(asset, ct);
            var stream = new GZipInputStream(await streamResource.GetStreamAsync(ct));
            return BlockingStream.StartBufferedStreamAsync(stream, null, ct);
        }

        public override async Task<IAssetsContainer> ExtractToDestinationAsync(IDestination destination, CarrierManager carrierManager, CancellationToken ct)
        {
            var inputStream = await GetStreamAsync(asset, ct);
            string name = asset.ContentDisposition.FileName.Replace(".gz", "");

            GzipEntryAsset gzipEntryAsset = new GzipEntryAsset(name, inputStream);

            try
            {
                var newArchive = await Read(gzipEntryAsset, logger, resourceServiceProvider, fileSystem, ct);
                return await newArchive.ExtractToDestinationAsync(destination, carrierManager, ct);
            }
            catch { }

            IDictionary<string, IAsset> assetsExtracted = new Dictionary<string, IAsset>();

            var archiveAssetDestination = destination.To(gzipEntryAsset);
            archiveAssetDestination.PrepareDestination();
            var assetDeliveries = carrierManager.GetSingleDeliveryQuotations(gzipEntryAsset, archiveAssetDestination);
            logger.LogDebug(gzipEntryAsset.Name);
            foreach (var delivery in assetDeliveries)
            {
                var assetExtracted = await delivery.Carrier.DeliverAsync(delivery, ct);
                if (assetExtracted != null)
                {
                    assetsExtracted.Add(gzipEntryAsset.Name, new GenericAsset(assetExtracted, gzipEntryAsset.Title, gzipEntryAsset.Roles));
                    break;
                }
            }

            return new GenericAssetContainer(destination, assetsExtracted);
        }
    }
}
