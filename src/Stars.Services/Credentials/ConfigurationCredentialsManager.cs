using System;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Services.Credentials
{
    public class ConfigurationCredentialsManager : ICredentials
    {
        private readonly CredentialsOptions _credentialsCache = new CredentialsOptions();
        protected readonly ILogger logger;


        public ConfigurationCredentialsManager(IOptions<CredentialsOptions> options, ILogger<ConfigurationCredentialsManager> logger)
        {
            if (options != null && options.Value != null)
                this._credentialsCache.Load(options.Value);
            this.logger = logger;
        }

        public virtual NetworkCredential GetCredential(Uri uri, string authType)
        {
            var cred = _credentialsCache.Values.FirstOrDefault(v => MatchUriAndAuth(v, uri, authType));
            if (cred == null) return null;
            return cred.ToNetWorkCredential();
        }

        private bool MatchUriAndAuth(CredentialsOption v, Uri uri, string authType)
        {
            return v.Uri.IsBaseOf(uri) && v.AuthType.Equals(authType, StringComparison.InvariantCultureIgnoreCase);
        }

        public void CacheCredential(Uri uriCut, string authType, NetworkCredential cred)
        {
            this._credentialsCache.Add(new Guid().ToString("N"), new CredentialsOption(uriCut.ToString(), authType, cred.UserName, cred.Password));
        }
    }
}