using System;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Service.Router;

namespace Terradue.Stars.Service.Router
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

        public async Task<INode> GoToNode()
        {
            return await GetResponse().ContinueWith(wr => new WebNode(wr.Result));
        }

        public async Task<Stream> GetStreamAsync()
        {
            return (await GetResponse()).GetResponseStream();
        }

        public async Task<WebResponse> GetResponse()
        {
            // try
            // {
                return await request.GetResponseAsync();
            // }
            // catch (WebException e)
            // {
            //     var httpResponse = e.Response as HttpWebResponse;
            //     if ( httpResponse != null && httpResponse.StatusCode == HttpStatusCode.Unauthorized && CredentialsManager != null){
                    
            //     }
            // }
            // return null;
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