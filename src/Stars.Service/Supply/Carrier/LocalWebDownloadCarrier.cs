using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Terradue.Stars.Service.Router;
using System.Net.Http;
using Terradue.Stars.Service.Supply.Destination;
using Terradue.Stars.Interface.Supply;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supply.Destination;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace Terradue.Stars.Service.Supply.Carrier
{
    public class LocalWebDownloadCarrier : LocalCarrier, ICarrier
    {

        private readonly ILogger logger;
        private readonly ICredentials credentials;

        public LocalWebDownloadCarrier(IOptions<GlobalOptions> options, ILogger logger, ICredentials credentials) : base(options)
        {
            this.logger = logger;
            this.credentials = credentials;
        }

        public override string Id => "WebDownload";

        public override bool CanDeliver(IRoute route, ISupplier supplier, IDestination destination)
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
            request.Credentials = credentials;
            return request;
        }

        public override async Task<IRoute> Deliver(IDelivery delivery)
        {
            LocalDelivery localDelivery = delivery as LocalDelivery;
            LocalFileSystemRoute localRoute = new LocalFileSystemRoute(localDelivery.LocalPath, localDelivery.Route.ContentType, localDelivery.Route.ResourceType, localDelivery.Route.ContentLength);

            if (!carrierServiceOptions.ForceOverwrite && localRoute.File.Exists && delivery.Route.ContentLength > 0 &&
                Convert.ToUInt64(localRoute.File.Length) == delivery.Route.ContentLength)
            {
                logger.LogDebug("File {0} exists with the same size. Skipping download", localRoute.File.Name);
                return localRoute;
            }

            var wr = CreateWebRequest(delivery.Route.Uri);

            await DownloadFile(wr.GetResponseAsync(), localRoute.File);
            return localRoute;
        }

        private async Task DownloadFile(Task<WebResponse> arg1, FileInfo file)
        {
            var webResponse = await arg1;
            using (FileStream fileStream = file.Create())
            {
                await webResponse.GetResponseStream().CopyToAsync(fileStream);
                fileStream.Close();
            }
        }


        protected override (string, ulong) FindLocalDestination(IRoute route, LocalDirectoryDestination directory)
        {
            string filename = route.Uri == null ? "unknown" : Path.GetFileName(route.Uri.ToString());

            ulong contentLength = route.ContentLength;

            var wr = CreateWebRequest(route.Uri);
            try
            {
                using (var response = wr.GetResponse())
                {
                    if (response.Headers.AllKeys.Contains("Content-Disposition"))
                    {
                        try
                        {
                            ContentDisposition disposition = new ContentDisposition(response.Headers["Content-Disposition"]);
                            if (disposition != null && !string.IsNullOrEmpty(disposition.FileName)) filename = disposition.FileName;
                        }
                        catch (FormatException) { }
                    }
                    if (response.ContentLength > 0)
                    {
                        ulong cl = Convert.ToUInt64(response.ContentLength);
                        if (cl > 0) contentLength = cl;
                    }
                }
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    HttpWebResponse webResponse = e.Response as HttpWebResponse;
                    if (webResponse != null && webResponse.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        throw e;
                    }
                }
            }


            return (Path.Join(directory.Uri.ToString(), filename), contentLength);
        }
    }
}