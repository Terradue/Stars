using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Services.Resources
{
    public class HttpResource : IStreamResource
    {
        private readonly Uri _url;
        private readonly HttpClient _client;
        private HttpCachedHeaders _cachedHeaders;

        internal HttpResource(Uri url, HttpClient httpClient, HttpCachedHeaders cachedHeaders = null)
        {
            _url = url;
            _client = httpClient;
            _cachedHeaders = cachedHeaders;
        }

        public async Task<Stream> GetStreamAsync(CancellationToken ct)
        {
            var res = await _client.GetAsync(_url, HttpCompletionOption.ResponseHeadersRead, ct);
            return await res.Content.ReadAsStreamAsync();
        }

        public async Task<Stream> GetStreamAsync(long start, CancellationToken ct, long end = -1)
        {
            var request = new HttpRequestMessage { RequestUri = _url };
            request.Headers.Range = new RangeHeaderValue(0, 1000);
            return await (await _client.SendAsync(request, HttpCompletionOption.ResponseContentRead, ct)).Content.ReadAsStreamAsync();
        }

        public ContentType ContentType => new ContentType(CachedHeaders?.ContentType?.ToString() ?? "application/octet-stream");

        public ResourceType ResourceType => ResourceType.Unknown;

        public ulong ContentLength => Convert.ToUInt64(CachedHeaders.ContentLength);

        public ContentDisposition ContentDisposition => CachedHeaders.ContentDisposition == null ? null : new ContentDisposition(CachedHeaders.ContentDisposition.ToString());

        public Uri Uri => _url;

        public bool CanBeRanged
        {
            get
            {
                if (CachedHeaders.Contains("Accept-Ranges"))
                    return (CachedHeaders.GetValues("Accept-Ranges").Contains("bytes"));
                return false;
            }
        }

        public HttpCachedHeaders CachedHeaders
        {
            get
            {
                if (_cachedHeaders == null)
                {
                    using (var res = _client.GetAsync(_url, HttpCompletionOption.ResponseHeadersRead).GetAwaiter().GetResult())
                    {
                        _cachedHeaders = new HttpCachedHeaders(_client.GetAsync(_url).GetAwaiter().GetResult());
                    }
                }
                return _cachedHeaders;
            }
        }

        public override string ToString()
        {
            return Uri.ToString();
        }
    }
}
