using System;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stars.Router;

namespace Stars.Router
{
    internal class WebRoute : IRoute
    {
        private readonly WebRequest request;

        internal WebRoute(WebRequest request)
        {
            this.request = request;
        }

        public static WebRoute Create(Uri uri)
        {
            WebRequest request = WebRequest.Create(uri);
            return new WebRoute(request);
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

    }
}