using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Terradue.Stars.Services.Credentials
{
    public class CredentialsOptions : Dictionary<string, CredentialsOption>, IDictionary<string, CredentialsOption>
    {
        public CredentialsOptions()
        {
        }

        internal void Load(IDictionary<string, CredentialsOption> credentialsOptions)
        {
            if (credentialsOptions == null) return;
            foreach (var credentialsOption in credentialsOptions)
            {
                this.Remove(credentialsOption.Key);
                this.Add(credentialsOption.Key, credentialsOption.Value);
            }
        }
    }

    public class CredentialsOption
    {
        public CredentialsOption()
        {
            AuthType = "Basic";
        }

        public CredentialsOption(string uriPrefix, string authType, string username, string password)
        {
            UriPrefix = uriPrefix;
            AuthType = authType;
            Username = username;
            Password = password;
        }

        internal NetworkCredential ToNetWorkCredential()
        {
            return new NetworkCredential(Username, Password);
        }

        [JsonIgnore]
        public Uri Uri
        {
            get
            {
                if (UriPrefix == null) return null;
                return new Uri(UriPrefix);
            }
        }

        public string UriPrefix { get; set; }

        public string AuthType { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }
    }
}