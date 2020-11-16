using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Services.Router
{
    public class WebRoute : IResource, IStreamable
    {
        private readonly WebRequest request;
        private readonly ulong contentLength;

        private WebResponse cachedResponse;

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
            var response = await request.CloneRequest(request.RequestUri).GetResponseAsync();
            return response.GetResponseStream();
        }

        public Uri Uri => request.RequestUri;

        public ContentType ContentType
        {
            get
            {
                if (!string.IsNullOrEmpty(CachedHeaders[HttpResponseHeader.ContentType]))
                    return new ContentType(CachedHeaders[HttpResponseHeader.ContentType]);

                return new ContentType("application/octet-stream");
            }
        }

        public ResourceType ResourceType => ResourceType.Unknown;

        public WebRequest Request { get => request; }

        public ulong ContentLength
        {
            get
            {
                if (contentLength > 0) return contentLength;
                if (!string.IsNullOrEmpty(CachedHeaders[HttpResponseHeader.ContentLength]))
                    return Convert.ToUInt64(CachedHeaders[HttpResponseHeader.ContentLength]);
                return 0;
            }
        }

        public ContentDisposition ContentDisposition
        {
            get
            {
                if (!string.IsNullOrEmpty(CachedHeaders["Content-Disposition"]))
                    return new ContentDisposition(CachedHeaders["Content-Disposition"]);
                return new ContentDisposition() { FileName = Path.GetFileName(request.RequestUri.ToString()) };
            }

        }

        public bool CanBeRanged
        {
            get
            {
                try
                {
                    if (cachedResponse is FileWebResponse) return true;
                    if (!string.IsNullOrEmpty(CachedHeaders[HttpResponseHeader.AcceptRanges]))
                        return (CachedHeaders[HttpResponseHeader.AcceptRanges] == "bytes");
                }
                catch { }
                return false;
            }
        }

        public WebHeaderCollection CachedHeaders
        {
            get
            {
                if (cachedResponse == null)
                    CacheResponse().GetAwaiter().GetResult();
                return cachedResponse?.Headers;
            }
        }

        private async Task CacheResponse()
        {
            try
            {
                var cacheRequest = request.CloneRequest(request.RequestUri);
                var response = await cacheRequest.GetResponseAsync();
                cachedResponse = new CachedWebResponse(response);
                response.Close();
            }
            catch
            {
                cachedResponse = new CachedWebResponse();
            }
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
            var response = await rangedRequest.GetResponseAsync();
            var stream = response.GetResponseStream();
            if (rangedRequest is FileWebRequest)
            {
                stream.Seek(start, SeekOrigin.Begin);
            }
            return stream;
        }
    }
}