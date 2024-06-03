using System.Net;

namespace Terradue.Stars.Services.Router
{
    internal class CachedWebResponse : WebResponse
    {

        WebHeaderCollection headers = new WebHeaderCollection();

        public CachedWebResponse(WebResponse response)
        {
            headers = CopyHeaders(response.Headers);
        }

        public CachedWebResponse()
        {
        }

        private WebHeaderCollection CopyHeaders(WebHeaderCollection headers)
        {
            WebHeaderCollection webHeaders = new WebHeaderCollection();
            foreach (var key in headers.AllKeys)
                webHeaders.Add(key, headers[key]);
            return webHeaders;
        }

        public override WebHeaderCollection Headers => headers;
    }
}
