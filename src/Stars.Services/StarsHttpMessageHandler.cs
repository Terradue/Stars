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
            AllowAutoRedirect = true;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Exception for store.terradue.com that requires to be called with curl user agent
            if (request.RequestUri.Host == "store.terradue.com")
            {
                request.Headers.Remove("User-Agent");
                request.Headers.Add("User-Agent", "Curl (Stars)");
            }

            return base.SendAsync(request, cancellationToken);
        }

    }
}