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
    public class WebRoute : IRoute, IStreamable, IDisposable
    {
        private readonly WebRequest request;
        private readonly ulong contentLength;

        private WebResponse cacheResponse = null;

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
            if ( cacheResponse != null ) cacheResponse.Close();
            cacheResponse = await request.CloneRequest(request.RequestUri).GetResponseAsync();
            return cacheResponse.GetResponseStream();
        }

        public Uri Uri => request.RequestUri;

        public ContentType ContentType
        {
            get
            {
                var response = GetOrCreateCacheResponse().Result;
                if (!string.IsNullOrEmpty(response.ContentType))
                    return new ContentType(response.ContentType);
                return new ContentType("application/octet-stream");
            }
        }

        public ResourceType ResourceType => ResourceType.Unknown;

        public WebRequest Request { get => request; }

        public ulong ContentLength => contentLength == 0 ? Convert.ToUInt64(GetOrCreateCacheResponse().Result.ContentLength) : contentLength;

        public ContentDisposition ContentDisposition
        {
            get
            {
                var response = GetOrCreateCacheResponse().Result;
                if (!string.IsNullOrEmpty(response.Headers["Content-Disposition"]))
                    return new ContentDisposition(response.Headers["Content-Disposition"]);
                return new ContentDisposition() { FileName = Path.GetFileName(request.RequestUri.ToString()) };
            }

        }

        public bool CanBeRanged
        {
            get
            {
                WebResponse response = GetOrCreateCacheResponse().Result;
                return response is FileWebResponse ||
                    (response is HttpWebResponse && response.Headers[HttpResponseHeader.AcceptRanges] == "bytes");
            }
        }

        private async Task<WebResponse> GetOrCreateCacheResponse()
        {
            if (cacheResponse == null)
            {
                var cacheRequest = request.CloneRequest(request.RequestUri);
                if ( cacheResponse != null ) cacheResponse.Close();
                cacheResponse = await cacheRequest.GetResponseAsync();
                // cacheResponse.Close();
            }
            return cacheResponse;
        }

        public async Task<Stream> GetStreamAsync(long start, long end = -1)
        {
            var rangedRequest = request.CloneRequest(request.RequestUri);
            if (rangedRequest is HttpWebRequest)
            {
                if (end == -1)
                    (rangedRequest as HttpWebRequest).AddRange(start);
                else
                    (rangedRequest as HttpWebRequest).AddRange(start, end);
            }
            if (rangedRequest is FtpWebRequest)
            {
                (rangedRequest as FtpWebRequest).ContentOffset = start;
            }
            if ( cacheResponse != null ) cacheResponse.Close();
            cacheResponse = await rangedRequest.GetResponseAsync();
            var stream = cacheResponse.GetResponseStream();
            if (rangedRequest is FileWebRequest)
            {
                stream.Seek(start, SeekOrigin.Begin);
            }
            return stream;
        }

        public void Dispose()
        {
            if ( cacheResponse != null )
                cacheResponse.Close();
        }
    }
}