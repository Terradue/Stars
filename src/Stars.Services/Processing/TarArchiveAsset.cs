// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: TarArchiveAsset.cs

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Supplier;
using Terradue.Stars.Services.Supplier.Carrier;

namespace Terradue.Stars.Services.Processing
{
    internal class TarArchiveAsset : Archive
    {
        protected readonly IAsset asset;
        protected readonly IResourceServiceProvider resourceServiceProvider;
        private readonly ILogger logger;

        public TarArchiveAsset(IAsset asset,
                               IResourceServiceProvider resourceServiceProvider,
                               ILogger logger)
        {
            this.asset = asset;
            this.resourceServiceProvider = resourceServiceProvider;
            this.logger = logger;
        }

        public override Uri Uri => asset.Uri;

        protected virtual async Task<Stream> GetTarStreamAsync(IAsset asset, CancellationToken ct)
        {
            var streamResource = await resourceServiceProvider.GetStreamResourceAsync(asset, ct);
            var stream = await streamResource.GetStreamAsync(ct);
            return BlockingStream.StartBufferedStreamAsync(stream, null, ct);
        }

        public async override Task<IAssetsContainer> ExtractToDestinationAsync(IDestination destination, CarrierManager carrierManager, CancellationToken ct)
        {
            IDictionary<string, IAsset> assets = await ExtractTarAsync(await GetTarStreamAsync(asset, ct), DeliverTarEntryAsync, destination, carrierManager, ct);
            return new GenericAssetContainer(destination, assets);
        }

        private async Task<IAsset> DeliverTarEntryAsync(TarEntryAsset tarEntryAsset, IDestination destination, CarrierManager carrierManager, CancellationToken ct)
        {
            var archiveAssetDestination = destination.To(tarEntryAsset);
            archiveAssetDestination.PrepareDestination();
            var assetDeliveries = carrierManager.GetSingleDeliveryQuotations(tarEntryAsset, archiveAssetDestination);
            logger.LogDebug(tarEntryAsset.Name);
            var assetExtracted = await assetDeliveries.First().Carrier.DeliverAsync(assetDeliveries.First(), ct);
            var entryAsset = new GenericAsset(assetExtracted, tarEntryAsset.Name, new string[] { "data" });
            if (assetDeliveries.First().Resource is IAsset)
            {
                entryAsset.MergeProperties((assetDeliveries.First().Resource as IAsset).Properties);
            }
            return entryAsset;
        }



        /// <summary>
        /// Extractes a <c>tar</c> archive to the specified directory.
        /// </summary>
        /// <param name="tarStream">The <i>.tar</i> to extract.</param>
        /// <param name="outputDir">Output directory to write the files.</param>
        public Task<IDictionary<string, IAsset>> ExtractTarAsync(Stream tarStream,
                                                                 Func<TarEntryAsset, IDestination, CarrierManager, CancellationToken, Task<IAsset>> tarEntryAction,
                                                                 IDestination destination,
                                                                 CarrierManager carrierManager,
                                                                 CancellationToken ct)
        {
            Dictionary<string, IAsset> extractedAssets = new Dictionary<string, IAsset>();
            string longLink = null;
            while (true)
            {
                var buffer = new byte[100];
                tarStream.Read(buffer, 0, 100);
                var name = Encoding.ASCII.GetString(buffer);
                if (name.IndexOf('\0') >= 0)
                    name = name.Substring(0, name.IndexOf('\0'));
                if (string.IsNullOrWhiteSpace(name))
                    break;
                buffer = new byte[24];
                tarStream.Read(buffer, 0, 24);
                buffer = new byte[12];
                tarStream.Read(buffer, 0, 12);
                string sizestr = Encoding.UTF8.GetString(buffer, 0, 12).Trim('\0').Trim();
                int size = 0;
                if (!string.IsNullOrEmpty(sizestr))
                    size = Convert.ToInt32(sizestr, 8);
                buffer = new byte[376];
                tarStream.Read(buffer, 0, 376);

                if (longLink != null)
                {
                    name = longLink;
                    longLink = null;
                }

                if (name.Contains("@LongLink"))
                {
                    buffer = new byte[size];
                    tarStream.Read(buffer, 0, size);
                    longLink = Encoding.ASCII.GetString(buffer);
                    longLink = longLink.Substring(0, longLink.IndexOf('\0'));
                    size = 0;
                }

                if (size > 0)
                {

                    BlockingStream blockingStream = new BlockingStream(Convert.ToUInt64(size));

                    TarEntryAsset tarEntryAsset = new TarEntryAsset(name, Convert.ToUInt64(size), blockingStream);

                    const int chunk = 81920;
                    long totalRead = 0;
                    var extractTask = Task.Factory.StartNew((state) =>
                    {
                        int chunkSize = chunk;
                        int read = 0;
                        if (size < chunk)
                            chunkSize = Convert.ToInt32(size);
                        var buffer = new byte[chunk];
                        do
                        {
                            try
                            {
                                read = tarStream.Read(buffer, 0, chunkSize);
                                blockingStream.Write(buffer, 0, read);
                            }
                            catch (Exception e)
                            {
                                logger.LogWarning(e.Message);
                            }
                            totalRead += read;
                            if (size < (totalRead + chunk))
                                chunkSize = Convert.ToInt32(size - totalRead);
                            buffer = new byte[chunk];
                        } while (totalRead < size);
                        blockingStream.Close();
                    }, null, TaskCreationOptions.AttachedToParent);

                    extractedAssets.Add(name, tarEntryAction(tarEntryAsset, destination, carrierManager, ct).GetAwaiter().GetResult());

                    if (!extractTask.IsCompleted)
                    {
                        // Read until the end of the entry
                        blockingStream.CopyTo(Stream.Null);
                    }
                }

                var pos = tarStream.Position;

                int offset = Convert.ToInt32(512 - (pos % 512));
                if (offset == 512)
                    offset = 0;

                if (offset > 0)
                {
                    var offsetBuf = new byte[offset];
                    tarStream.Read(offsetBuf, 0, offset);
                }
            }
            return Task.FromResult<IDictionary<string, IAsset>>(extractedAssets);
        }
    }
}
