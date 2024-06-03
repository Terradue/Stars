using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Terradue.Stars.Services.Credentials
{
    public class ConfigurationCredentialsManager : ICredentials
    {
        private readonly CredentialsOptions _credentialsCache = new CredentialsOptions();
        protected readonly ILogger logger;


        public ConfigurationCredentialsManager(IOptions<CredentialsOptions> options, ILogger<ConfigurationCredentialsManager> logger)
        {
            if (options != null && options.Value != null)
                _credentialsCache.Load(options.Value);
            this.logger = logger;
        }

        public virtual NetworkCredential GetCredential(Uri uri, string authType)
        {
            // get the matching credentials and pick one randomly
            var creds = _credentialsCache.Values
                        .Where(v => MatchUriAndAuth(v, uri, authType));
            if (creds.Count() == 0) return null;
            int i = new Random().Next(0, creds.Count() - 1);
            var cred = creds.ElementAt(i);
            if (cred == null) return null;
            return cred.ToNetWorkCredential();
        }

        private bool MatchUriAndAuth(CredentialsOption v, Uri uri, string authType)
        {
            if (v.Uri == null) return false;
            return v.Uri.IsBaseOf(uri) && v.AuthType.Equals(authType, StringComparison.InvariantCultureIgnoreCase);
        }

        public IEnumerable<CredentialsOption> GetPossibleCredentials(Uri uri, string authType)
        {
            return _credentialsCache.Values.Where(v => MatchUriAndAuth(v, uri, authType));
        }

        public void CacheCredential(Uri uriCut, string authType, NetworkCredential cred)
        {
            _credentialsCache.Add(new Guid().ToString("N"), new CredentialsOption(uriCut.ToString(), authType, cred.UserName, cred.Password));
        }
    }
}
