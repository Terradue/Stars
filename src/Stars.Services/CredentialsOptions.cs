using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Terradue.Stars.Services
{
    public class CredentialsOptions : IDictionary<string, CredentialsOption>
    {
        CredentialCache _credentialCache = new CredentialCache();

        Dictionary<string, NetworkCredential> credentials = new Dictionary<string, NetworkCredential>();

        public CredentialsOptions()
        {
        }

        public CredentialsOption this[string key]
        {
            get => CreateCredential(credentials[key]);
            set
            {
                if (ContainsKey(key))
                {
                    Remove(key);
                }
                Add(key, value);
            }
        }

        internal NetworkCredential GetCredential(Uri uri, string authType)
        {
            return _credentialCache.GetCredential(uri, authType);
        }

        private CredentialsOption CreateCredential(NetworkCredential networkCredential)
        {
            return new CredentialsOption(networkCredential.Domain, "Basic", networkCredential.Password, networkCredential.Password);
        }

        internal void Add(Uri uriCut, string authType, NetworkCredential cred)
        {
            string key = Guid.NewGuid().ToString();
            credentials.Add(key, cred);
            CacheCredential(uriCut, cred, authType?? "Basic");
        }

        private NetworkCredential CreateNetworkCredential(CredentialsOption credential)
        {
            return new NetworkCredential(credential.Username, credential.Password);
        }

        private void CacheCredential(Uri uriPrefix, NetworkCredential networkCredential, string authType = "Basic")
        {
            _credentialCache.Add(uriPrefix, authType, networkCredential);
        }

        public CredentialCache CredentialCache { get => _credentialCache; }

        public ICollection<string> Keys => credentials.Keys;

        public ICollection<CredentialsOption> Values => new Collection<CredentialsOption>(credentials.Values.Select(v => CreateCredential(v)).ToList());

        public int Count => credentials.Count;

        public bool IsReadOnly => false;

        public void Add(string key, CredentialsOption value)
        {
            NetworkCredential credential = CreateNetworkCredential(value);
            credentials.Add(key, credential);
            CacheCredential(new Uri(value.UriPrefix), credential, value.AuthType?? "Basic");
        }

        public void Add(KeyValuePair<string, CredentialsOption> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            credentials.Clear();
            _credentialCache = new CredentialCache();
        }

        public void Configure(IConfigurationSection configuration, ILogger logger)
        {
            if (configuration == null)
                return;
            var credentials = configuration.GetChildren();
            foreach (var credential in credentials)
            {
                try
                {
                    Add(credential.Key, new CredentialsOption(
                        credential["UriPrefix"],
                        string.IsNullOrEmpty(credential["AuthType"]) ? "Basic" : credential["AuthType"],
                        credential["Username"], credential["Password"])
                    );
                }
                catch (Exception e)
                {
                    logger.LogWarning("Credential {0} skipped : {1}", credential.Key, e.Message);
                }
            }
        }

        public bool Contains(KeyValuePair<string, CredentialsOption> item)
        {
            throw new NotImplementedException();
        }

        public bool ContainsKey(string key)
        {
            return credentials.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, CredentialsOption>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, CredentialsOption>> GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public bool Remove(string key)
        {
            NetworkCredential nc = null;
            if (credentials.TryGetValue(key, out nc))
            {
                credentials.Remove(key);
                _credentialCache.Remove(new Uri(nc.Domain), "Basic");
                return true;
            }
            return false;

        }

        public bool Remove(KeyValuePair<string, CredentialsOption> item)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(string key, out CredentialsOption value)
        {
            NetworkCredential nc = null;
            if (credentials.TryGetValue(key, out nc))
            {
                value = CreateCredential(nc);
                return true;
            }
            value = null;
            return false;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return credentials.GetEnumerator();
        }
    }

    public class CredentialsOption
    {
        public CredentialsOption()
        {
        }

        public CredentialsOption(string uriPrefix, string authType, string username, string password)
        {
            UriPrefix = uriPrefix;
            AuthType = authType;
            Username = username;
            Password = password;
        }

        public string UriPrefix { get; set; }

        public string AuthType { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }
    }
}