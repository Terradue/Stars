﻿// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: S3Options.cs

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
            UrlPattern = s3Configuration?.UrlPattern;
            Region = s3Configuration?.Region;
            ServiceURL = s3Configuration?.ServiceURL;
            AccessKey = s3Configuration?.AccessKey;
            SecretKey = s3Configuration?.SecretKey;
            SessionToken = s3Configuration?.SessionToken;
            AuthenticationRegion = s3Configuration?.AuthenticationRegion;
            UseHttp = s3Configuration == null ? false : s3Configuration.UseHttp;
            ForcePathStyle = s3Configuration == null ? false : s3Configuration.ForcePathStyle;
            ScopeRoles = s3Configuration?.ScopeRoles;
            TryAdaptRegion = s3Configuration == null ? false : s3Configuration.TryAdaptRegion;
            AmazonS3Config = s3Configuration?.AmazonS3Config;
            AWSCredentials = s3Configuration?.AWSCredentials;
            UseWebIdentity = s3Configuration == null ? false : s3Configuration.UseWebIdentity;
        }

        /// <summary>
        /// Create a new S3Configuration from original AmazonS3Config and AWSCredentials
        /// This is useful when you want to create a new S3Configuration from an existing AmazonS3Config and AWSCredentials
        /// and leverage the modification to access a custom S3 service by using IS3ClientFactory
        /// </summary>
        /// <param name="amazonS3Config"></param>
        /// <param name="awsCredentials"></param>
        /// <returns></returns>
        public static S3Configuration Create(AmazonS3Config amazonS3Config, AWSCredentials awsCredentials)
        {
            return new S3Configuration
            {
                AmazonS3Config = amazonS3Config,
                AWSCredentials = awsCredentials,
                UrlPattern = $".*", // match all
                Region = amazonS3Config?.RegionEndpoint?.SystemName,
                ServiceURL = amazonS3Config?.ServiceURL,
                AccessKey = awsCredentials?.GetCredentials()?.AccessKey,
                SecretKey = awsCredentials?.GetCredentials()?.SecretKey,
                SessionToken = awsCredentials?.GetCredentials()?.Token,
                AuthenticationRegion = amazonS3Config?.AuthenticationRegion,
                UseHttp = amazonS3Config?.UseHttp ?? false,
                ForcePathStyle = amazonS3Config?.ForcePathStyle ?? false,
                TryAdaptRegion = true,
                UseWebIdentity = false
            };
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
