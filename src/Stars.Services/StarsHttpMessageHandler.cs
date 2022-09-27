using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Terradue.Stars.Services
{
    public class StarsHttpMessageHandler : HttpClientHandler
    {
        public StarsHttpMessageHandler(ICredentials credentials)
        {
            Credentials = credentials;
            // UseDefaultCredentials = true;
            AllowAutoRedirect = true;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri.Host == "store.terradue.com")
            {
                request.Headers.Remove("User-Agent");
                request.Headers.Add("User-Agent", "Curl (TerrApi)");
            }

            return base.SendAsync(request, cancellationToken);
        }

    }
}