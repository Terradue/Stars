using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Terradue.Stars.Services
{
    public class CredentialsOptions
    {
        CredentialCache _credentialCache = new CredentialCache();

        public CredentialsOptions()
        {
        }

        public CredentialCache CredentialCache { get => _credentialCache; }

        public void Configure(IConfigurationSection configuration, ILogger logger)
        {
            if (configuration == null)
                return;
            var credentials = configuration.GetChildren();
            foreach (var credential in credentials)
            {
                try
                {
                    _credentialCache.Add(
                        new Uri(credential["UriPrefix"]),
                        string.IsNullOrEmpty(credential["AuthType"]) ? "Basic" : credential["AuthType"],
                        new NetworkCredential(credential["Username"], credential["Password"])
                    );
                }
                catch (Exception e)
                {
                    logger.LogWarning("Credential {0} skipped : {1}", credential.Key, e.Message);
                }
            }
        }
    }
}