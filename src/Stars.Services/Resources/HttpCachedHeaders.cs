using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http.Headers;

namespace Terradue.Stars.Services.Resources
{
    public class HttpCachedHeaders : HttpHeaders
    {
        public HttpCachedHeaders(HttpHeaders source)
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
                base.Remove("Content-Type");
                base.Add("Content-Type", value.ToString());
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
                base.Remove("Content-Length");
                base.Add("Content-Length", value.ToString());
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
                base.Remove("Content-Disposition");
                base.Add("Content-Disposition", value.ToString());
            }
        }
    }
}