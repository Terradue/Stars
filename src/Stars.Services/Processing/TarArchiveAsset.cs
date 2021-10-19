using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Tar;
using Ionic.Zip;
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
        private readonly ILogger logger;

        public TarArchiveAsset(IAsset asset, ILogger logger)
        {
            this.asset = asset;
            this.logger = logger;
        }

        public override System.Uri Uri => asset.Uri;

        protected virtual BlockingStream GetTarStream(IAsset asset)
        {
            const int chunk = 4096;
            BlockingStream blockingStream = new BlockingStream(1000);
            asset.GetStreamable().GetStreamAsync()
                .ContinueWith(task =>
                {
                    var stream = task.GetAwaiter().GetResult();
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
                });
            return blockingStream;
        }

        internal async override Task<IAssetsContainer> ExtractToDestination(IDestination destination, CarrierManager carrierManager)
        {
            IDictionary<string, IAsset> assets = await ExtractTar(GetTarStream(asset), DeliverTarEntry, destination, carrierManager);
            return new GenericAssetContainer(destination, assets);
        }

        private async Task<IAsset> DeliverTarEntry(TarEntryAsset tarEntryAsset, IDestination destination, CarrierManager carrierManager)
        {
            var archiveAssetDestination = destination.To(tarEntryAsset);
            archiveAssetDestination.PrepareDestination();
            var assetDeliveries = carrierManager.GetSingleDeliveryQuotations(tarEntryAsset, archiveAssetDestination);
            logger.LogDebug(tarEntryAsset.Name);
            var assetExtracted = await assetDeliveries.First().Carrier.Deliver(assetDeliveries.First());
            var entryAsset = new GenericAsset(assetExtracted, tarEntryAsset.Name, new string[] { "data" });
            if (assetDeliveries.First().Route is IAsset)
            {
                entryAsset.MergeProperties((assetDeliveries.First().Route as IAsset).Properties);
            }
            return entryAsset;
        }



        /// <summary>
        /// Extractes a <c>tar</c> archive to the specified directory.
        /// </summary>
        /// <param name="tarStream">The <i>.tar</i> to extract.</param>
        /// <param name="outputDir">Output directory to write the files.</param>
        public Task<IDictionary<string, IAsset>> ExtractTar(Stream tarStream, Func<TarEntryAsset, IDestination, CarrierManager, Task<IAsset>> tarEntryAction, IDestination destination, CarrierManager carrierManager)
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
                if (String.IsNullOrWhiteSpace(name))
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

                    extractedAssets.Add(name, tarEntryAction(tarEntryAsset, destination, carrierManager).GetAwaiter().GetResult());

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