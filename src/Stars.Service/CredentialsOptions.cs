using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Configuration;

namespace Stars.Service
{
    public class CredentialsOptions : Dictionary<string, object> 
    {

        CredentialCache _credentialCache = new CredentialCache();

        public void Configure(IConfigurationSection configuration)
        {
            if (configuration == null)
                return;
            if (configuration.GetSection("NetworkCredentials") != null)
            {
                Console.Out.WriteLine(configuration.GetSection("NetworkCredentials").Path);
                Console.Out.WriteLine(string.Join(",", configuration.GetSection("NetworkCredentials").GetChildren().Select(nc => nc.Key)));
                var networkCredentials = configuration.GetSection("NetworkCredentials").GetChildren();
                foreach (var networkCredential in networkCredentials)
                {
                    _credentialCache.Add(
                        new Uri(networkCredential["UrlPrefix"]),
                        string.IsNullOrEmpty(networkCredential["AuthType"]) ? "Basic" : networkCredential["AuthType"],
                        new NetworkCredential(networkCredential["Username"], networkCredential["Password"])
                    );
                }
            }
        }
    }
}