using System;
using System.Collections.Generic;
using System.Net.S3;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Microsoft.Extensions.Configuration;
using Terradue.Stars.Services.Credentials;
using Terradue.Stars.Services.Plugins;

namespace Terradue.Stars.Services
{
    public class StarsConfiguration
    {
        public static StarsConfiguration Configuration => new StarsConfiguration();

        public Dictionary<string, CredentialsOption> CredentialsOptions { get; set; }

        public AWSOptions AWSOptions { get; set; }

        public System.Net.S3.S3BucketsOptions S3BucketsOptions { get; set; }

        public PluginsOptions PluginsOptions { get; set; }

        public StarsConfiguration UseCredentialsOptions(IConfigurationSection credentialsConfigSection)
        {
            CredentialsOptions = credentialsConfigSection.Get<Dictionary<string, CredentialsOption>>();
            return this;
        }

        public StarsConfiguration UseCredentialsOptions(CredentialsOptions credentialsOptions)
        {
            CredentialsOptions = credentialsOptions;
            return this;
        }

        public StarsConfiguration UseGlobalConfiguration(IConfiguration configuration)
        {
            UseCredentialsOptions(configuration.GetSection("Credentials"));
            UsePluginsOptions(configuration.GetSection("Plugins"));
            UseAmazonWebServices(configuration);
            return this;
        }

        private void UseAmazonWebServices(IConfiguration configuration)
        {
            AWSOptions = configuration.GetAWSOptions("AWS");
            var chain = new CredentialProfileStoreChain();
            AWSCredentials awsCredentials;
            chain.TryGetAWSCredentials(AWSOptions.Profile ?? "default", out awsCredentials);
            AWSOptions.Credentials = awsCredentials;
            S3BucketsOptions = configuration.GetSection("AWS").GetSection("Buckets").Get<S3BucketsOptions>();
        }

        private StarsConfiguration UsePluginsOptions(IConfigurationSection configurationSection)
        {
            PluginsOptions = configurationSection.Get<PluginsOptions>();
            return this;
        }
    }
}