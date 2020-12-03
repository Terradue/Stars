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

        public LocalStreamingCarrier(ILogger<LocalStreamingCarrier> logger) : base(logger)
        {
            this.logger = logger;
            Priority = 75;
        }

        public override int Priority { get; set; }
        public override string Key { get => Id; set { } }

        public override string Id => "Streaming";

        public override bool CanDeliver(IResource route, IDestination destination)
        {
            if (!(destination is LocalFileDestination)) return false;
            if (route is IAsset) return true;
            if (!(route is IStreamable)) return false;

            return true;
        }

        public override async Task<IResource> Deliver(IDelivery delivery)
        {
            LocalDelivery localDelivery = delivery as LocalDelivery;
            LocalFileSystemResource localRoute = new LocalFileSystemResource(localDelivery.LocalPath, localDelivery.Route.ResourceType);

            IStreamable streamable = delivery.Route as IStreamable;
            if (streamable == null && delivery.Route is IAsset)
                streamable = (delivery.Route as IAsset).GetStreamable();

            if (streamable == null)
                throw new InvalidDataException(string.Format("There is no streamable content in {0}", delivery.Route.Uri));

            if (localRoute.File.Exists && streamable.ContentLength > 0 &&
               Convert.ToUInt64(localRoute.File.Length) == streamable.ContentLength)
            {
                logger.LogDebug("File {0} exists with the same size. Skipping download", localRoute.File.Name);
                return localRoute;
            }
            await StreamToFile(streamable, localRoute);
            localRoute.File.Refresh();
            return localRoute;
        }

        private async Task StreamToFile(IStreamable streamable, LocalFileSystemResource localResource)
        {
            FileInfo file = localResource.File;
            Stream stream = null;

            try
            {

                // Try a resume
                if (file.Exists && file.Length > 0 && Convert.ToUInt64(file.Length) < streamable.ContentLength && streamable.CanBeRanged)
                {
                    logger.LogDebug("Trying to resume from {0}", file.Length);
                    stream = await streamable.GetStreamAsync(file.Length);
                    using (FileStream fileStream = file.OpenWrite())
                    {
                        fileStream.Seek(0, SeekOrigin.End);
                        await stream.CopyToAsync(fileStream);
                        await fileStream.FlushAsync();
                    }
                }
                else
                {
                    stream = await streamable.GetStreamAsync();
                    using (FileStream fileStream = new FileStream(file.FullName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                    {
                        await stream.CopyToAsync(fileStream);
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