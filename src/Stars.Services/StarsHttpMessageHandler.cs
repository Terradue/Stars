// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: StarsHttpMessageHandler.cs

using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Terradue.Stars.Services.Credentials;
using System.Net;

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

            // --- NEW: Basic auth from embedded credentials in URL (user:pass@host) ---
            if (request.Headers.Authorization == null &&
                request.RequestUri != null &&
                !string.IsNullOrWhiteSpace(request.RequestUri.UserInfo) &&
                request.RequestUri.UserInfo != "preauth")
            {
                var parts = request.RequestUri.UserInfo.Split(new[] { ':' }, 2);
                var username = Uri.UnescapeDataString(parts[0]);
                var password = parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : "";

                if (!string.IsNullOrEmpty(username))
                {
                    var token = System.Convert.ToBase64String(
                        System.Text.Encoding.UTF8.GetBytes($"{username}:{password}")
                    );
                    request.Headers.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", token);

                    // Optional but recommended: avoid leaking creds further down (e.g. logs/redirects)
                    // NOTE: changing RequestUri mid-flight is usually ok for HttpClientHandler.
                    var ub = new UriBuilder(request.RequestUri) { UserName = null, Password = null };
                    request.RequestUri = ub.Uri;
                }
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
