using System;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Services.Credentials
{
    public class ConfigurationCredentialsManager : ICredentials
    {
        private readonly IOptions<CredentialsOptions> _options;
        protected readonly ILogger logger;

        private CredentialCache _credentialCache = new CredentialCache();


        public ConfigurationCredentialsManager(IOptions<CredentialsOptions> options, ILogger<ConfigurationCredentialsManager> logger)
        {
            this._options = options;
            this.logger = logger;
            CacheCredentials(options.Value);
        }

        private void CacheCredentials(CredentialsOptions options)
        {
            foreach (var credential in options.Values)
            {
                CacheCredential(new Uri(credential.UriPrefix),
                                credential.AuthType,
                                new NetworkCredential(credential.Username, credential.Password));
            }
        }

        public virtual NetworkCredential GetCredential(Uri uri, string authType)
        {
            NetworkCredential cred = _credentialCache.GetCredential(uri, authType);
            if (cred != null)
            {
                logger.LogInformation("Using saved credentials ({0})", cred.UserName);
            }
            return cred;
        }

        public void CacheCredential(Uri uriCut, string authType, NetworkCredential cred)
        {
            _credentialCache.Add(uriCut, authType, cred);
        }
    }
}