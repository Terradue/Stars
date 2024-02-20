using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Amazon.Runtime;
using Amazon.S3;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Terradue.Stars.Services.Resources
{
    public class S3Options
    {
        public static string DefaultName = "S3";

        public S3Options()
        {
            Services = new Dictionary<string, S3Configuration>();
            Policies = new S3OptionsPolicies();
        }

        public List<string> DomainsWithNoCertificateVerification { get; set; } = new List<string>();

        public Dictionary<string, S3Configuration> Services { get; set; }

        public S3OptionsPolicies Policies { get; set; }

        public IConfigurationSection ConfigurationSection { get; set; }

        public IConfiguration RootConfiguration { get; set; }

        public KeyValuePair<string, S3Configuration> GetS3Configuration(string url, ClaimsPrincipal claimsPrincipal = null)
        {
            var kv = Services
                        .Where(c => claimsPrincipal == null || (c.Value.ScopeRoles?.Any(r => claimsPrincipal.IsInRole(r)) ?? true))
                        .Where(c => Regex.Match(url, c.Value.UrlPattern, RegexOptions.Singleline).Success)
                        .FirstOrDefault();
            if (kv.Key != null)
                return kv;
            return default(KeyValuePair<string, S3Configuration>);
        }

    }

    public class S3OptionsPolicies
    {
        public S3OptionsPolicies()
        {
            PrivateWorkspacePolicyId = "personalStorageReadWrite";
        }

        public string PrivateWorkspacePolicyId { get; set; }
    }

    public class S3Configuration
    {
        public S3Configuration()
        {
        }

        public S3Configuration(S3Configuration s3Configuration)
        {
            this.UrlPattern = s3Configuration?.UrlPattern;
            this.Region = s3Configuration?.Region;
            this.ServiceURL = s3Configuration?.ServiceURL;
            this.AccessKey = s3Configuration?.AccessKey;
            this.SecretKey = s3Configuration?.SecretKey;
            this.SessionToken = s3Configuration?.SessionToken;
            this.AuthenticationRegion = s3Configuration?.AuthenticationRegion;
            this.UseHttp = s3Configuration == null ? false : s3Configuration.UseHttp;
            this.ForcePathStyle = s3Configuration == null ? false : s3Configuration.ForcePathStyle;
            this.ScopeRoles = s3Configuration?.ScopeRoles;
            this.TryAdaptRegion = s3Configuration == null ? false : s3Configuration.TryAdaptRegion;
            this.AmazonS3Config = s3Configuration?.AmazonS3Config;
            this.AWSCredentials = s3Configuration?.AWSCredentials;
            this.UseWebIdentity = s3Configuration == null ? false : s3Configuration.UseWebIdentity;
        }

        public string UrlPattern { get; set; }
        public string ServiceURL { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string SessionToken { get; set; }
        public string Region { get; set; }
        public string AuthenticationRegion { get; set; }
        public bool UseHttp { get; set; }
        public bool ForcePathStyle { get; set; }
        public string[] ScopeRoles { get; set; }
        public bool TryAdaptRegion { get; set; } = true;
        public bool UseWebIdentity { get; set; } = false;

        [JsonIgnore]
        public AmazonS3Config AmazonS3Config { get; set; }

        [JsonIgnore]
        public AWSCredentials AWSCredentials { get; set; }
    }
}