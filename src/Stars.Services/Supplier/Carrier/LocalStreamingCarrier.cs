using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Terradue.Stars.Services.Router;
using System.Net.Http;
using Terradue.Stars.Services.Supplier.Destination;
using Terradue.Stars.Interface.Supplier;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier.Destination;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace Terradue.Stars.Services.Supplier.Carrier
{
    public class LocalStreamingCarrier : LocalCarrier, ICarrier
    {
        private readonly ILogger logger;

        public LocalStreamingCarrier(IOptions<GlobalOptions> options, ILogger logger) : base(options)
        {
            this.logger = logger;
            Priority = 75;
        }

        public override int Priority { get; set; }
        public override string Key { get => Id; set { } }

        public override string Id => "Streaming";

        public override bool CanDeliver(IRoute route, ISupplier supplier, IDestination destination)
        {
            if (!(destination is LocalDirectoryDestination)) return false;
            if (route is IAsset) return true;
            if (!(route is IStreamable)) return false;

            return true;
        }

        public override async Task<IRoute> Deliver(IDelivery delivery)
        {
            LocalDelivery localDelivery = delivery as LocalDelivery;
            LocalFileSystemRoute localRoute = new LocalFileSystemRoute(localDelivery.LocalPath, localDelivery.Route.ContentType, localDelivery.Route.ResourceType, localDelivery.Route.ContentLength);

            IStreamable streamable = delivery.Route as IStreamable;
            if (streamable == null && delivery.Route is IAsset)
                streamable = (delivery.Route as IAsset).GetStreamable();

            if (streamable == null)
                throw new InvalidDataException(string.Format("There is no streamable content in {0}", delivery.Route.Uri));

            if (!carrierServiceOptions.ForceOverwrite && localRoute.File.Exists && streamable.ContentLength > 0 &&
               Convert.ToUInt64(localRoute.File.Length) == streamable.ContentLength)
            {
                logger.LogDebug("File {0} exists with the same size. Skipping download", localRoute.File.Name);
                return localRoute;
            }
            await StreamToFile(streamable, localRoute);
            return localRoute;
        }

        private async Task StreamToFile(IStreamable streamable, LocalFileSystemRoute localRoute)
        {
            FileInfo file = new FileInfo(localRoute.Uri.AbsolutePath);
            Stream stream = null;

            // Try a resume
            if (file.Exists && file.Length > 0 && streamable.CanBeRanged)
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
                using (FileStream fileStream = file.Create())
                {
                    await stream.CopyToAsync(fileStream);
                    await fileStream.FlushAsync();
                }
            }


        }


        protected override (LocalFileDestination, ulong) FindLocalDestination(IRoute route, LocalDirectoryDestination directory)
        {
            IStreamable streamable = route as IStreamable;
            if (streamable == null && route is IAsset)
                streamable = (route as IAsset).GetStreamable();

            if (streamable == null)
                throw new InvalidDataException(string.Format("There is no streamable content in {0}", route.Uri));
            string filename = route.Uri == null ? "unknown" : Path.GetFileName(route.Uri.ToString());
            ulong contentLength = streamable.ContentLength;
            ContentDisposition contentDisposition = streamable.ContentDisposition;
            if (contentDisposition != null && !string.IsNullOrEmpty(contentDisposition.FileName))
                filename = contentDisposition.FileName;

            return (new LocalFileDestination(new FileInfo(Path.Join(directory.Uri.LocalPath, filename))), contentLength);
        }
    }
}