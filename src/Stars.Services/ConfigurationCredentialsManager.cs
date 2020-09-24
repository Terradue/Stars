using System;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Services
{
    public class ConfigurationCredentialsManager : ICredentials
    {
        private readonly IOptions<CredentialsOptions> _options;
        protected readonly ILogger logger;

        public ConfigurationCredentialsManager(IOptions<CredentialsOptions> options, ILogger logger)
        {
            this._options = options;
            this.logger = logger;
        }

        public virtual NetworkCredential GetCredential(Uri uri, string authType)
        {
            var cred = _options.Value.CredentialCache.GetCredential(uri, authType);
            if ( cred != null ){
                logger.LogInformation("Using saved credentials ({0})", cred.UserName);
            }
            return cred;
        }

        public void CacheCredential(Uri uriCut, string authType, NetworkCredential cred)
        {
            _options.Value.CredentialCache.Add(uriCut, authType, cred);
        }
    }
}