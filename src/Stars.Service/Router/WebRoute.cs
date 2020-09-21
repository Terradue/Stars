using System;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stars.Interface.Router;
using Stars.Service.Router;

namespace Stars.Service.Router
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

        public static WebRoute Create(Uri uri, ulong contentLength = 0)
        {
            WebRequest request = CreateWebRequest(uri);
            return new WebRoute(request, contentLength);
        }

        private static WebRequest CreateWebRequest(Uri uri)
        {
            var request = WebRequest.Create(uri);
            request.Headers.Set("User-Agent", "Stars/0.0.1");
            return request;
        }

        public async Task<INode> GoToNode()
        {
            return await request.GetResponseAsync().ContinueWith(wr => new WebNode(wr.Result));
        }

        public async Task<Stream> GetStreamAsync()
        {
            return (await request.GetResponseAsync()).GetResponseStream();
        }

        public Uri Uri => request.RequestUri;

        public ContentType ContentType => null;

        public bool CanRead => true;

        public ResourceType ResourceType => ResourceType.Unknown;

        public WebRequest Request { get => request; }

        public ulong ContentLength => contentLength;

        public ContentDisposition ContentDisposition => new ContentDisposition() { FileName = Path.GetFileName(request.RequestUri.ToString()) };
    }
}