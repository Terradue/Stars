using System;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stars.Router;

namespace Stars.Router
{
    internal class WebRoute : IRoute
    {
        private WebRequest request;

        public WebRoute(WebRequest request)
        {
            this.request = request;
        }

        public Uri Uri => request.RequestUri;

        public ContentType ContentType => null;

        public bool CanGetResource => true;

        public async Task<IResource> GetResource()
        {
            return await request.GetResponseAsync().ContinueWith(wr => new WebResource(wr.Result));
        }
    }
}