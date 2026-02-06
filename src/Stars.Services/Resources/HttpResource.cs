// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: HttpResource.cs

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
        private readonly AuthenticationHeaderValue _auth;
        private HttpCachedHeaders _cachedHeaders;

        internal HttpResource(Uri url, HttpClient httpClient, HttpCachedHeaders cachedHeaders = null, AuthenticationHeaderValue auth = null)
        {
            _url = url ?? throw new ArgumentNullException(nameof(url));
            _client = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _cachedHeaders = cachedHeaders;
            _auth = auth; // optional
        }

        public async Task<Stream> GetStreamAsync(CancellationToken ct)
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, _url);
            if (_auth != null) req.Headers.Authorization = _auth;

            var res = await _client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
            res.EnsureSuccessStatusCode();

            // NOTE: do NOT dispose response here, because the caller will consume the stream.
            return await res.Content.ReadAsStreamAsync().ConfigureAwait(false);
        }

        public async Task<Stream> GetStreamAsync(long start, CancellationToken ct, long end = -1)
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, _url);
            if (_auth != null) req.Headers.Authorization = _auth;

            req.Headers.Range = end >= 0
                ? new RangeHeaderValue(start, end)
                : new RangeHeaderValue(start, null);

            var res = await _client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
            res.EnsureSuccessStatusCode();

            return await res.Content.ReadAsStreamAsync().ConfigureAwait(false);
        }

        public ContentType ContentType => new ContentType(CachedHeaders?.ContentType?.ToString() ?? "application/octet-stream");

        public ResourceType ResourceType => ResourceType.Unknown;

        public ulong ContentLength => Convert.ToUInt64(CachedHeaders?.ContentLength ?? 0);

        public ContentDisposition ContentDisposition =>
            CachedHeaders?.ContentDisposition == null ? null : new ContentDisposition(CachedHeaders.ContentDisposition.ToString());

        public Uri Uri => _url;

        public bool CanBeRanged
        {
            get
            {
                if (CachedHeaders != null && CachedHeaders.Contains("Accept-Ranges"))
                    return CachedHeaders.GetValues("Accept-Ranges").Contains("bytes");
                return false;
            }
        }

        public HttpCachedHeaders CachedHeaders
        {
            get
            {
                if (_cachedHeaders == null)
                {
                    // Prefer HEAD for headers; if server rejects HEAD, fall back to GET headers-only.
                    try
                    {
                        using var headReq = new HttpRequestMessage(HttpMethod.Head, _url);
                        if (_auth != null) headReq.Headers.Authorization = _auth;

                        using var headRes = _client.SendAsync(headReq, HttpCompletionOption.ResponseHeadersRead)
                                                  .GetAwaiter().GetResult();
                        headRes.EnsureSuccessStatusCode();
                        _cachedHeaders = new HttpCachedHeaders(headRes);
                    }
                    catch
                    {
                        using var getReq = new HttpRequestMessage(HttpMethod.Get, _url);
                        if (_auth != null) getReq.Headers.Authorization = _auth;

                        using var getRes = _client.SendAsync(getReq, HttpCompletionOption.ResponseHeadersRead)
                                                 .GetAwaiter().GetResult();
                        getRes.EnsureSuccessStatusCode();

                        _cachedHeaders = new HttpCachedHeaders(getRes);
                        _cachedHeaders.AddRange(getRes.Content.Headers);
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
