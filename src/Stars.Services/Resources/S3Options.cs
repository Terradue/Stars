using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Terradue.Stars.Services.Resources
{
    public class S3Options
    {
        public static string DefaultName = "S3";

        public S3Options()
        {
            Services = new List<S3Configuration>();
            Policies = new S3OptionsPolicies();
        }

        public List<S3Configuration> Services { get; set; }
        public S3OptionsPolicies Policies { get; set; }

        public S3Configuration GetS3Configuration(string url)
        {
            var kv = Services.Where(c => Regex.Match(url, c.UrlPattern, RegexOptions.Singleline).Success).FirstOrDefault();
            if (kv != null)
                return kv;
            return null;
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
        public string Endpoint { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string Region { get; set; }
        public bool SslEnabled { get; set; }
        public Uri EndpointUrl
        {
            get
            {
                if (string.IsNullOrEmpty(Endpoint))
                    return null;
                return new Uri(string.Format("http{0}://{1}", SslEnabled ? "s" : "", Endpoint));
            }
        }

    }
}