using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Services.Resources
{
    public class HttpResource : IStreamResource
    {
        private readonly Uri _url;
        private readonly HttpClient _client;
        private HttpContentHeaders _cachedHeaders;

        internal HttpResource(Uri url, HttpClient httpClient, HttpContentHeaders cachedHeaders = null)
        {
            this._url = url;
            this._client = httpClient;
            this._cachedHeaders = cachedHeaders;
        }

        public async Task<Stream> GetStreamAsync(CancellationToken ct)
        {
            var res = await this._client.GetAsync(_url, HttpCompletionOption.ResponseHeadersRead, ct);
            return await res.Content.ReadAsStreamAsync();
        }

        public async Task<Stream> GetStreamAsync(long start, CancellationToken ct, long end = -1)
        {
            var request = new HttpRequestMessage { RequestUri = _url };
            request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(0, 1000);
            return await (await this._client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct)).Content.ReadAsStreamAsync();
        }

        public ContentType ContentType => new ContentType(CachedHeaders.ContentType.ToString());

        public ResourceType ResourceType => ResourceType.Unknown;

        public ulong ContentLength => Convert.ToUInt64(CachedHeaders.ContentLength);

        public ContentDisposition ContentDisposition => new ContentDisposition(CachedHeaders.ContentDisposition.ToString());

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

        public HttpContentHeaders CachedHeaders
        {
            get
            {
                if (_cachedHeaders == null)
                {
                    _cachedHeaders = _client.GetAsync(_url).GetAwaiter().GetResult().Content.Headers;
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