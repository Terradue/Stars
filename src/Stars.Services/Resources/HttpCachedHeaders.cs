// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: HttpCachedHeaders.cs

using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Terradue.Stars.Services.Resources
{
    public class HttpCachedHeaders : HttpHeaders
    {
        public HttpCachedHeaders(HttpResponseMessage message)
        {
            foreach (var header in message.Content.Headers)
            {
                try
                {
                    Add(header.Key, header.Value.ToArray());
                }
                catch { }
            }
            foreach (var header in message.Headers)
            {
                try
                {
                    Add(header.Key, header.Value.ToArray());
                }
                catch { }
            }
        }

        public void AddRange(HttpHeaders source)
        {
            foreach (var header in source)
            {
                try
                {
                    Add(header.Key, header.Value.ToArray());
                }
                catch { }
            }
        }




        public MediaTypeHeaderValue ContentType
        {
            get
            {
                try
                {
                    return (MediaTypeHeaderValue)MediaTypeHeaderValue.Parse(GetValues("Content-Type").FirstOrDefault());
                }
                catch { return null; }
            }
            set
            {
                Remove("Content-Type");
                Add("Content-Type", value.ToString());
            }
        }

        public long? ContentLength
        {
            get
            {
                try
                {
                    return long.Parse(GetValues("Content-Length").FirstOrDefault());
                }
                catch { return null; }
            }
            set
            {
                Remove("Content-Length");
                Add("Content-Length", value.ToString());
            }
        }

        public ContentDispositionHeaderValue ContentDisposition
        {
            get
            {
                try
                {
                    return ContentDispositionHeaderValue.Parse(GetValues("Content-Disposition").FirstOrDefault());
                }
                catch { return null; }
            }
            set
            {
                Remove("Content-Disposition");
                Add("Content-Disposition", value.ToString());
            }
        }
    }
}
