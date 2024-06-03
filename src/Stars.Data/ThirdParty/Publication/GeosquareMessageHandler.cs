using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Terradue.Stars.Data.ThirdParty.Geosquare
{
    public class GeosquareMessageHandler : HttpClientHandler
    {
        public GeosquareMessageHandler(ICredentials credentials)
        {
            Credentials = credentials;
            AllowAutoRedirect = true;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // This setting is required to always send the Authorization header
            // Geosquare does not challenge the client if the header is not present
            if (Credentials != null)
            {
                var networkCredentials = Credentials.GetCredential(request.RequestUri, "Basic");
                if (networkCredentials != null)
                {
                    var byteArray = Encoding.ASCII.GetBytes($"{networkCredentials.UserName}:{networkCredentials.Password}");
                    request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                }
            }

            return base.SendAsync(request, cancellationToken);
        }

    }
}
