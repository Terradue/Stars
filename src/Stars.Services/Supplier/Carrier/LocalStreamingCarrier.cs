using System;
using System.IO;
using System.Threading.Tasks;
using Terradue.Stars.Services.Router;
using Terradue.Stars.Services.Supplier.Destination;
using Terradue.Stars.Interface.Supplier;
using Terradue.Stars.Interface.Supplier.Destination;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Terradue.Stars.Interface;
using System.Security.AccessControl;
using System.Net;

namespace Terradue.Stars.Services.Supplier.Carrier
{
    public class LocalStreamingCarrier : LocalCarrier, ICarrier
    {
        private readonly ILogger logger;
        private readonly IResourceServiceProvider resourceServiceProvider;

        public LocalStreamingCarrier(ILogger<LocalStreamingCarrier> logger,
                                     IResourceServiceProvider resourceServiceProvider) : base(logger)
        {
            this.logger = logger;
            this.resourceServiceProvider = resourceServiceProvider;
            Priority = 75;
        }

        public override int Priority { get; set; }
        public override string Key { get => Id; set { } }

        public override string Id => "Streaming";

        public override bool CanDeliver(IResource route, IDestination destination)
        {
            if (route is IOrderable) return false;
            if (!(destination is LocalFileDestination)) return false;
            if (route is IAsset) return true;
            if (!(route is IStreamResource)) return false;

            return true;
        }

        public override async Task<IResource> Deliver(IDelivery delivery, bool overwrite = false)
        {
            LocalDelivery localDelivery = delivery as LocalDelivery;
            LocalFileSystemResource localRoute = new LocalFileSystemResource(localDelivery.LocalPath, localDelivery.Resource.ResourceType);

            IStreamResource inputStreamResource = await resourceServiceProvider.GetStreamResourceAsync(delivery.Resource);

            if (inputStreamResource == null)
                throw new InvalidDataException(string.Format("There is no streamable content in {0}", delivery.Resource.Uri));

            if (!overwrite && localRoute.File.Exists && inputStreamResource.ContentLength > 0 &&
               Convert.ToUInt64(localRoute.File.Length) == inputStreamResource.ContentLength)
            {
                logger.LogDebug("File {0} exists with the same size. Skipping download", localRoute.File.Name);
                return localRoute;
            }
            await StreamToFile(inputStreamResource, localRoute, overwrite);
            localRoute.File.Refresh();
            if (inputStreamResource.ContentLength > 0 && Convert.ToUInt64(localRoute.File.Length) != inputStreamResource.ContentLength)
                throw new InvalidDataException(string.Format("Data transferred size ({0}) does not correspond with stream content length ({1})", localRoute.File.Length, inputStreamResource.ContentLength));
            return localRoute;
        }

        private async Task StreamToFile(IStreamResource streamable, LocalFileSystemResource localResource, bool overwrite = false)
        {
            FileInfo file = localResource.File;
            Stream stream = null;

            try
            {

                // Try a resume
                if (!overwrite && file.Exists && file.Length > 0 && Convert.ToUInt64(file.Length) < streamable.ContentLength && streamable.CanBeRanged)
                {
                    logger.LogDebug("Trying to resume from {0}", file.Length);
                    stream = await streamable.GetStreamAsync(file.Length);
                    using (FileStream fileStream = file.Open(FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                    {
                        // fileStream.Seek(0, SeekOrigin.End);
                        await stream.CopyToAsync(fileStream, 5 * 1024 * 1024).ConfigureAwait(false);
                        await fileStream.FlushAsync();
                    }
                }
                else
                {
                    stream = await streamable.GetStreamAsync();
                    using (FileStream fileStream = new FileStream(file.FullName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                    {
                        await stream.CopyToAsync(fileStream, 5 * 1024 * 1024).ConfigureAwait(false);
                        await fileStream.FlushAsync();
                    }
                }
            }
            catch (WebException we)
            {
                try
                {
                    using (StreamReader sr = new StreamReader(we.Response.GetResponseStream()))
                        logger.LogDebug(sr.ReadToEnd());
                }
                catch { }
                throw;
            }
        }
    }
}