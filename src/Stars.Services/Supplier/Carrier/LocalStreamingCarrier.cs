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
using Terradue.Stars.Services.Processing.Destination;
using Terradue.Stars.Interface.Supply;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supply.Destination;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace Terradue.Stars.Services.Processing.Carrier
{
    public class LocalStreamingCarrier : LocalCarrier, ICarrier
    {
        private readonly ILogger logger;

        public LocalStreamingCarrier(IOptions<GlobalOptions> options, ILogger logger) : base(options)
        {
            this.logger = logger;
        }


        public override string Id => "Streaming";

        public override bool CanDeliver(IRoute route, ISupplier supplier, IDestination destination)
        {
            if (!(destination is LocalDirectoryDestination)) return false;
            if (!(route is IStreamable)) return false;

            return true;
        }

        public override async Task<IRoute> Deliver(IDelivery delivery)
        {
            LocalDelivery localDelivery = delivery as LocalDelivery;
            LocalFileSystemRoute localRoute = new LocalFileSystemRoute(localDelivery.LocalPath, localDelivery.Route.ContentType, localDelivery.Route.ResourceType, localDelivery.Route.ContentLength);

             if ( !carrierServiceOptions.ForceOverwrite && localRoute.File.Exists && delivery.Route.ContentLength > 0 &&
                Convert.ToUInt64(localRoute.File.Length) == delivery.Route.ContentLength ){
                logger.LogDebug("File {0} exists with the same size. Skipping download", localRoute.File.Name);
                return localRoute;
            }

            using (var stream = await (delivery.Route as IStreamable).GetStreamAsync())
            {
                await StreamToFile(stream, localRoute);
            }
            return localRoute;
        }

        private async Task StreamToFile(Stream stream, LocalFileSystemRoute localRoute)
        {
            FileInfo file = new FileInfo(localRoute.Uri.AbsolutePath);
            using ( FileStream fileStream = file.Create()){
                await stream.CopyToAsync(fileStream);
                await fileStream.FlushAsync();
            }
        }


        protected override (string, ulong) FindLocalDestination(IRoute route, LocalDirectoryDestination directory)
        {
            string filename = route.Uri == null ? "unknown" : Path.GetFileName(route.Uri.ToString());
            ulong contentLength = route.ContentLength;
            ContentDisposition contentDisposition = (route as IStreamable).ContentDisposition;
            if (contentDisposition != null && !string.IsNullOrEmpty(contentDisposition.FileName))
                filename = contentDisposition.FileName;

            return (Path.Join(directory.Uri.ToString(), filename), contentLength);
        }
    }
}