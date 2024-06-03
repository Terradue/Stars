using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Terradue.Stars.Services.Credentials;

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

            // Preauth if configured and no auth header is present
            if (Credentials is ConfigurationCredentialsManager configurationCredentialsManager &&
                 request.Headers.Authorization == null)
            {
                var credOption = configurationCredentialsManager.GetPossibleCredentials(request.RequestUri, "Basic")
                                            .FirstOrDefault(cred => cred.PreAuth == true);
                if (credOption != null)
                {
                    // Add a basc auth header
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic",
                        System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{credOption.Username}:{credOption.Password}")));
                }
            }

            return base.SendAsync(request, cancellationToken);
        }

    }
}
