using System;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stars.Interface.Router;
using Stars.Router;

namespace Stars.Router
{
    internal class WebRoute : IRoute
    {
        private readonly WebRequest request;
        private readonly ulong contentLength;

        internal WebRoute(WebRequest request, ulong contentLength = 0)
        {
            this.request = request;
            this.contentLength = contentLength;
        }

        public static WebRoute Create(Uri uri, ulong contentLength = 0)
        {
            WebRequest request = WebRequest.Create(uri);
            return new WebRoute(request, contentLength);
        }

        public async Task<IResource> GotoResource()
        {
            return await request.GetResponseAsync().ContinueWith(wr => new WebResource(wr.Result));
        }

        public Uri Uri => request.RequestUri;

        public ContentType ContentType => null;

        public bool CanRead => true;

        public ResourceType ResourceType => ResourceType.Unknown;

        public WebRequest Request { get => request; }

        public ulong ContentLength => contentLength;
    }
}