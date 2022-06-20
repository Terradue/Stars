using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace Terradue.Stars.Services.Resources
{
    public class S3Options
    {
        public static string DefaultName = "S3";

        public S3Options()
        {
            Services = new Dictionary<string, S3Configuration>();
            Policies = new S3OptionsPolicies();
            AdaptClientRegion = true;
        }

        public Dictionary<string, S3Configuration> Services { get; set; }

        public S3OptionsPolicies Policies { get; set; }

        public bool AdaptClientRegion { get; set; }

        public IConfigurationSection ConfigurationSection { get; set; }

        public IConfiguration RootConfiguration { get; set; }

        public KeyValuePair<string, S3Configuration> GetS3Configuration(string url)
        {
            var kv = Services.Where(c => Regex.Match(url, c.Value.UrlPattern, RegexOptions.Singleline).Success).FirstOrDefault();
            if (kv.Key != null)
                return kv;
            return default(KeyValuePair<string, S3Configuration>);
        }

    }

    public class S3OptionsPolicies
    {
        public S3OptionsPolicies()
        {
            PersonalStoragePolicyId = "personalStorageReadWrite";
        }

        public string PersonalStoragePolicyId { get; set; }
    }

    public class S3Configuration
    {
        public string UrlPattern { get; set; }
        public string ServiceURL { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string Region { get; set; }
        public string AuthenticationRegion { get; set; }
        public bool UseHttp { get; set; }
        public bool ForcePathStyle { get; set; }
        public bool UserScoped { get; set; }
    }
}