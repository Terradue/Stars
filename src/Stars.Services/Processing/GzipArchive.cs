using System;
using System.Collections.Generic;
using System.IO.Abstractions;
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

        protected BlockingStream GetStream(IAsset asset)
        {
            const int chunk = 4096;
            BlockingStream blockingStream = new BlockingStream(1000);
            resourceServiceProvider.GetStreamResourceAsync(asset)
                .ContinueWith(task => task.Result.GetStreamAsync()
                    .ContinueWith(task =>
                    {
                        var stream = new ICSharpCode.SharpZipLib.GZip.GZipInputStream(task.GetAwaiter().GetResult());
                        // blockingStream.SetLength(stream.Length);
                        Task.Factory.StartNew(() =>
                        {
                            int read;
                            var buffer = new byte[chunk];
                            do
                            {
                                read = stream.Read(buffer, 0, chunk);
                                blockingStream.Write(buffer, 0, read);
                            } while (read == chunk);
                            blockingStream.Close();
                        });
                    })
                );
            return blockingStream;
        }

        internal async override Task<IAssetsContainer> ExtractToDestination(IDestination destination, CarrierManager carrierManager)
        {
            var blockingStream = GetStream(asset);
            string name = asset.ContentDisposition.FileName.Replace(".gz", "");

            GzipEntryAsset gzipEntryAsset = new GzipEntryAsset(name, blockingStream);

            try
            {
                var newArchive = await Archive.Read(gzipEntryAsset, logger, resourceServiceProvider, fileSystem);
                return await newArchive.ExtractToDestination(destination, carrierManager);
            }
            catch { }

            IDictionary<string, IAsset> assetsExtracted = new Dictionary<string, IAsset>();

            var archiveAssetDestination = destination.To(gzipEntryAsset);
            archiveAssetDestination.PrepareDestination();
            var assetDeliveries = carrierManager.GetSingleDeliveryQuotations(gzipEntryAsset, archiveAssetDestination);
            logger.LogDebug(gzipEntryAsset.Name);
            foreach (var delivery in assetDeliveries)
            {
                var assetExtracted = await delivery.Carrier.Deliver(delivery);
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