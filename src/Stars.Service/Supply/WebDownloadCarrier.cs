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
    public class WebDownloadCarrier : ICarrier
    {

        CredentialCache _credentialCache = new CredentialCache();

        public WebDownloadCarrier()
        {
        }

        public void Configure(IConfigurationSection configuration)
        {
            if (configuration == null)
                return;
            if (configuration.GetSection("NetworkCredentials") != null)
            {
                Console.Out.WriteLine(configuration.GetSection("NetworkCredentials").Path);
                Console.Out.WriteLine(string.Join(",", configuration.GetSection("NetworkCredentials").GetChildren().Select(nc => nc.Key)));
                var networkCredentials = configuration.GetSection("NetworkCredentials").GetChildren();
                foreach (var networkCredential in networkCredentials)
                {
                    _credentialCache.Add(
                        new Uri(networkCredential["UrlPrefix"]),
                        string.IsNullOrEmpty(networkCredential["AuthType"]) ? "Basic" : networkCredential["AuthType"],
                        new NetworkCredential(networkCredential["Username"], networkCredential["Password"])
                    );
                }
            }
        }

        public string Id => "WebDownload";

        public bool CanDeliver(IRoute route, ISupplier supplier, IDestination destination)
        {
            if (!(destination is LocalDirectoryDestination)) return false;
            if (route is IStreamable) return false;

            try
            {
                CreateWebRequest(route.Uri);
            }
            catch
            {
                return false;
            }
            return true;
        }

        private WebRequest CreateWebRequest(Uri uri)
        {
            var request = WebRequest.Create(uri);
            request.Credentials = _credentialCache;
            return request;
        }

        public async Task<IRoute> Deliver(IDelivery delivery)
        {
            LocalDirectoryDestination directory = (LocalDirectoryDestination)delivery.Destination;
            var wr = CreateWebRequest(delivery.Route.Uri);
            LocalFileSystemRoute localRoute = LocalFileSystemRoute.Create(delivery.Route, delivery.Destination);
            await DownloadFile(wr.GetResponseAsync(), localRoute);
            return localRoute;
        }

        private async Task DownloadFile(Task<WebResponse> arg1, LocalFileSystemRoute localRoute)
        {
            var webResponse = await arg1;
            FileInfo file = new FileInfo(localRoute.Uri.AbsolutePath);
            await webResponse.GetResponseStream().CopyToAsync(file.Create());
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

            var wr = CreateWebRequest(route.Uri);
            var response = wr.GetResponse();
            if (response.Headers.AllKeys.Contains("Content-Disposition"))
            {
                try
                {
                    ContentDisposition disposition = new ContentDisposition(response.Headers["Content-Disposition"]);
                    if (disposition != null && !string.IsNullOrEmpty(disposition.FileName)) filename = disposition.FileName;
                }
                catch (FormatException) { }
            }
            if (!string.IsNullOrEmpty(response.ContentType))
            {
                try
                {
                    ContentType ct = new ContentType(response.ContentType);
                    if (ct != null) contentType = ct;
                }
                catch (FormatException) { }
            }
            if (response.ContentLength > 0)
            {
                ulong cl = Convert.ToUInt64(response.ContentLength);
                if (cl > 0) contentLength = cl;
            }

            return new LocalFileSystemRoute(new Uri(filename, UriKind.Relative), contentType, resourceType, contentLength);
        }
    }
}