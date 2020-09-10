using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Stars.Service.Router;
using System.Net.Http;
using Stars.Service.Supply.Asset;
using Stars.Service.Supply.Destination;
using Stars.Interface.Supply;
using Stars.Interface.Router;
using Stars.Interface.Supply.Destination;

namespace Stars.Service.Supply
{
    public class StreamingCarrier : ICarrier
    {

        public StreamingCarrier()
        {
        }

        public void Configure(IConfigurationSection configuration)
        {
        }

        public string Id => "Streaming";

        public bool CanDeliver(IRoute route, ISupplier supplier, IDestination destination)
        {
            if (!(destination is LocalDirectoryDestination)) return false;
            if (!(route is IStreamable)) return false;

            return true;
        }

        public async Task<IRoute> Deliver(IRoute route, ISupplier supplier, IDestination destination)
        {
            LocalDirectoryDestination directory = (LocalDirectoryDestination)destination;
            LocalFileSystemRoute localRoute = LocalFileSystemRoute.Create(route, destination);
            await StreamToFile(await (route as IStreamable).GetStreamAsync(), localRoute);
            return localRoute;
        }

        private async Task StreamToFile(Stream stream, LocalFileSystemRoute localRoute)
        {
            FileInfo file = new FileInfo(localRoute.Uri.AbsolutePath);
            await stream.CopyToAsync(file.Create());
        }

        public IDelivery QuoteDelivery(IRoute route, ISupplier supplier, IDestination destination)
        {
            if (!CanDeliver(route, supplier, destination)) return null;

            var newRoute = FindLocalDestination(route);

            // Let's make a cost as MB to download
            int cost = 1000;
            if (route.ContentLength > 0)
                cost = Convert.ToInt32(route.ContentLength / 1024 / 1024);
            else
            {
                if (newRoute.ContentLength > 0)
                    cost = Convert.ToInt32(newRoute.ContentLength / 1024 / 1024);
            }
            return new SimpleDelivery(this, route, supplier, destination.To(newRoute), cost);
        }

        private IRoute FindLocalDestination(IRoute route)
        {
            string filename = route.Uri == null ? "unknown" : Path.GetFileName(route.Uri.ToString());
            ContentType contentType = route.ContentType;
            ResourceType resourceType = route.ResourceType;
            ulong contentLength = route.ContentLength;
            ContentDisposition contentDisposition = (route as IStreamable).ContentDisposition;
            if (contentDisposition != null && !string.IsNullOrEmpty(contentDisposition.FileName))
                filename = contentDisposition.FileName;

            return new LocalFileSystemRoute(new Uri(filename, UriKind.Relative), contentType, resourceType, contentLength);
        }
    }
}