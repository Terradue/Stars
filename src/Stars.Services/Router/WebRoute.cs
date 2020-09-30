using System;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Services.Router;

namespace Terradue.Stars.Services.Router
{
    public class WebRoute : IRoute, IStreamable
    {
        private readonly WebRequest request;
        private readonly ulong contentLength;

        internal WebRoute(WebRequest request, ulong contentLength = 0)
        {
            this.request = request;
            this.contentLength = contentLength;
        }

        public static WebRoute Create(Uri uri, ulong contentLength = 0, ICredentials credentials = null)
        {
            WebRequest request = CreateWebRequest(uri, credentials);
            return new WebRoute(request, contentLength);
        }

        private static WebRequest CreateWebRequest(Uri uri, ICredentials credentials = null)
        {
            var request = WebRequest.Create(uri);
            request.Headers.Set("User-Agent", "Stars/0.0.1");
            request.Credentials = credentials;
            return request;
        }

        public async Task<Stream> GetStreamAsync()
        {
            return (await GetResponse()).GetResponseStream();
        }

        public async Task<WebResponse> GetResponse()
        {
            return await request.CloneRequest(request.RequestUri).GetResponseAsync();
        }

        public Uri Uri => request.RequestUri;

        public ContentType ContentType => new ContentType("application/octet-stream");

        public ResourceType ResourceType => ResourceType.Unknown;

        public WebRequest Request { get => request; }

        public ulong ContentLength => contentLength;

        public ContentDisposition ContentDisposition => new ContentDisposition() { FileName = Path.GetFileName(request.RequestUri.ToString()) };

    }
}