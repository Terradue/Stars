// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: S3ClientFactory.cs

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stac;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Services.Resources
{
    public class S3ClientFactory : IS3ClientFactory
    {
        private readonly ILogger<S3ClientFactory> logger;
        private readonly IOptionsMonitor<S3Options> s3Options;
        private readonly ILogger<IAmazonS3> _s3ClientLogger;
        private readonly IConfiguration configuration;

        public S3ClientFactory(ILogger<S3ClientFactory> logger,
                                IOptionsMonitor<S3Options> s3Options,
                                ILogger<IAmazonS3> s3ClientLogger,
                                IConfiguration configuration)
        {
            this.logger = logger;
            this.s3Options = s3Options;
            _s3ClientLogger = s3ClientLogger;
            this.configuration = configuration;
        }

        public IAmazonS3 CreateS3Client(string serviceName)
        {
            var s3Section = configuration.GetSection("S3");
            var servicesSection = s3Section.GetSection("Services");
            AWSOptions awsOptions = servicesSection.GetAWSOptions(serviceName) ?? throw new Exception($"Service {serviceName} not found in S3 configuration");
            awsOptions.Credentials = GetConfiguredCredentials(serviceName);
            return awsOptions.CreateServiceClient<IAmazonS3>();
        }

        public IAmazonS3 CreateS3Client(IAsset asset)
        {
            S3Url s3Url = S3Url.ParseUri(asset.Uri);
            var s3Config = GetAmazonS3Config(s3Url);
            var region = asset.Properties.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                                                  .GetProperty<string>(Stac.Extensions.Storage.StorageStacExtension.RegionField);
            if (!string.IsNullOrEmpty(region))
            {
                s3Config.AmazonS3Config.RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(region);
            }
            s3Config.AWSCredentials = GetConfiguredCredentials(s3Url);

            return CreateS3Client(asset, s3Config);
        }

        private IAmazonS3 CreateS3Client(IAsset asset, S3Configuration s3Config)
        {
            var s3Url = S3Url.ParseUri(asset.Uri);
            IAmazonS3 client = new S3Client(s3Config, this, _s3ClientLogger);
            return client;
        }



        public IAmazonS3 CreateS3Client(S3Configuration s3Config)
        {
            IAmazonS3 client = new S3Client(s3Config, this, _s3ClientLogger);
            return client;
        }

        public IAmazonS3 CreateS3Client(S3Url s3Url)
        {
            var s3Config = GetAmazonS3Config(s3Url);
            s3Config.AWSCredentials = GetConfiguredCredentials(s3Url);

            return CreateS3Client(s3Config);
        }

        /// <summary>
        /// Get the S3 Client using the identity provider
        /// </summary>
        /// <param name="s3Url"></param>
        /// <param name="identityProvider"></param>
        /// <param name="policy"></param>
        /// <returns></returns>
        public async Task<IAmazonS3> CreateS3ClientAsync(S3Url s3Url,
                                                         IIdentityProvider identityProvider,
                                                         string policy = null)
        {
            var s3Config = GetAmazonS3Config(s3Url);
            if (s3Config.UseWebIdentity)
            {
                s3Config.AWSCredentials = await GetWebIdentityCredentialsAsync(s3Config.ServiceURL,
                                                                              identityProvider.GetIdToken(),
                                                                              null);
            }
            if (s3Config.AWSCredentials == null)
                s3Config.AWSCredentials = GetConfiguredCredentials(s3Url, identityProvider);

            return CreateS3Client(s3Config);
        }

        /// <summary>
        /// Creates the AWSCredentials using either the profile indicated from the AWSOptions object
        /// of the SDK fallback credentials search.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public AWSCredentials GetConfiguredCredentials(S3Url s3Url, IIdentityProvider identityProvider = null)
        {
            var s3Configuration = s3Options.CurrentValue.GetS3Configuration(s3Url.ToString(), identityProvider?.GetPrincipal());

            if (!string.IsNullOrEmpty(s3Configuration.Value?.AccessKey) && !string.IsNullOrEmpty(s3Configuration.Value?.SecretKey))
            {
                if (!string.IsNullOrEmpty(s3Configuration.Value?.SessionToken))
                {
                    return new SessionAWSCredentials(s3Configuration.Value.AccessKey, s3Configuration.Value.SecretKey, s3Configuration.Value.SessionToken);
                }
                return new BasicAWSCredentials(s3Configuration.Value.AccessKey, s3Configuration.Value.SecretKey);
            }

            AWSOptions options = GetNamedAWSOptionsOrDefault(s3Configuration.Key);

            if (options != null)
            {
                if (options.Credentials != null)
                {
                    logger?.LogInformation("Using AWS credentials specified with the AWSOptions.Credentials property");
                    return options.Credentials;
                }
                if (!string.IsNullOrEmpty(options.Profile))
                {
                    var chain = new CredentialProfileStoreChain(options.ProfilesLocation);
                    if (chain.TryGetAWSCredentials(options.Profile, out AWSCredentials result))
                    {
                        logger?.LogInformation($"Found AWS credentials for the profile {options.Profile}");
                        return result;
                    }
                    else
                    {
                        logger?.LogInformation($"Failed to find AWS credentials for the profile {options.Profile}");
                    }
                }
            }

            return GetCredentials();
        }

        public AWSCredentials GetConfiguredCredentials(string serviceName)
        {
            if (s3Options.CurrentValue.Services.ContainsKey(serviceName))
            {
                var s3Configuration = s3Options.CurrentValue.Services[serviceName];
                if (!string.IsNullOrEmpty(s3Configuration.AccessKey) != null && !string.IsNullOrEmpty(s3Configuration.SecretKey))
                {
                    return new BasicAWSCredentials(s3Configuration.AccessKey, s3Configuration.SecretKey);
                }
            }

            AWSOptions options = GetNamedAWSOptionsOrDefault(serviceName);

            if (options != null)
            {
                if (options.Credentials != null)
                {
                    logger?.LogInformation("Using AWS credentials specified with the AWSOptions.Credentials property");
                    return options.Credentials;
                }
                if (!string.IsNullOrEmpty(options.Profile))
                {
                    var chain = new CredentialProfileStoreChain(options.ProfilesLocation);
                    if (chain.TryGetAWSCredentials(options.Profile, out AWSCredentials result))
                    {
                        logger?.LogInformation($"Found AWS credentials for the profile {options.Profile}");
                        return result;
                    }
                    else
                    {
                        logger?.LogInformation($"Failed to find AWS credentials for the profile {options.Profile}");
                    }
                }
            }

            return GetCredentials();

        }

        private AWSCredentials GetCredentials()
        {
            AWSCredentials credentials = null;

            try
            {
                credentials = new EnvironmentVariablesAWSCredentials();
                if (credentials.GetCredentials() != null)
                {
                    logger?.LogInformation("Using AWS credentials found in the environment");
                    return credentials;
                }
            }
            catch (InvalidOperationException e)
            {
                logger?.LogInformation(e.Message);
            }

            credentials = FallbackCredentialsFactory.GetCredentials(true);
            if (credentials == null)
            {
                logger?.LogError("Last effort to find AWS Credentials with AWS SDK's default credential search failed");
                throw new AmazonClientException("Failed to find AWS Credentials for constructing AWS service client");
            }
            else
            {
                if (credentials.GetType().Name == "DefaultInstanceProfileAWSCredentials")
                {
                    logger?.LogInformation("No configured AWS credentials. Defaulting to instance profile credentials (EC2)");
                }
                else
                {
                    logger?.LogInformation($"Found credentials using the AWS SDK's default credential search ({credentials.GetType().Name})");
                }
            }

            return credentials;
        }

        public S3Configuration GetAmazonS3Config(S3Url s3Url)
        {
            var s3Configuration = s3Options.CurrentValue.GetS3Configuration(s3Url.ToString());
            AWSOptions awsOptions = GetNamedAWSOptionsOrDefault(s3Configuration.Key) ?? throw new Exception($"Service for {s3Url} not found in S3 configuration");

            // If AWSOptions has no ServiceURL, try to use the derived one from the S3Url
            if (string.IsNullOrEmpty(awsOptions.DefaultClientConfig.ServiceURL) && s3Url.EndpointUrl != null)
            {
                awsOptions.DefaultClientConfig.ServiceURL = s3Url.EndpointUrl.ToString();
            }

            // Case of credentials in the configuration
            if (!string.IsNullOrEmpty(s3Configuration.Value?.AccessKey) && !string.IsNullOrEmpty(s3Configuration.Value?.SecretKey))
            {
                awsOptions.Credentials = new BasicAWSCredentials(s3Configuration.Value.AccessKey, s3Configuration.Value.SecretKey);
            }

            // Case of region not in the configuration but in the S3Url
            if (awsOptions.Region == null && !string.IsNullOrEmpty(s3Url.Region))
            {
                awsOptions.Region = RegionEndpoint.GetBySystemName(s3Url.Region);
            }

            AmazonCustomS3Config amazonS3Config = CreateS3Configuration(awsOptions);

            if (s3Configuration.Value?.ForcePathStyle == true || s3Url.PathStyle)
            {
                amazonS3Config.ForcePathStyle = true;
            }

            // if (string.IsNullOrEmpty(amazonS3Config.ServiceURL))
            // {
            //     logger?.LogInformation($"No Service URL configured, defaulting to {Amazon.RegionEndpoint.USEast1}");
            //     amazonS3Config.RegionEndpoint = Amazon.RegionEndpoint.USEast1;
            //     // amazonS3Config.ServiceURL = "https://s3.us-east-1.amazonaws.com";
            // }

            amazonS3Config.AllowAutoRedirect = true;
            amazonS3Config.RetryMode = RequestRetryMode.Standard;
            amazonS3Config.LogResponse = true;

            var config = new S3Configuration(s3Configuration.Value);
            config.AmazonS3Config = amazonS3Config;

            return config;

        }

        /// <summary>
        /// Creates the ClientConfig object for the service client.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static AmazonCustomS3Config CreateS3Configuration(AWSOptions options)
        {
            AmazonCustomS3Config config = new AmazonCustomS3Config();

            if (options == null)
            {
                options = new AWSOptions();
            }

            if (options.DefaultConfigurationMode.HasValue)
            {
                config.DefaultConfigurationMode = options.DefaultConfigurationMode.Value;
            }

            var defaultConfig = options.DefaultClientConfig;
            var emptyArray = new object[0];
            var singleArray = new object[1];

            var clientConfigTypeInfo = typeof(ClientConfig).GetTypeInfo();
            foreach (var property in clientConfigTypeInfo.DeclaredProperties)
            {
                if (property.GetMethod != null && property.SetMethod != null)
                {
                    // Skip RegionEndpoint because it is set below and calling the get method on the
                    // property triggers the default region fallback mechanism.
                    if (string.Equals(property.Name, "RegionEndpoint", StringComparison.Ordinal))
                        continue;

                    // DefaultConfigurationMode is skipped from the DefaultClientConfig because it is expected to be set
                    // at the top level of AWSOptions which is done before this loop.
                    if (string.Equals(property.Name, "DefaultConfigurationMode", StringComparison.Ordinal))
                        continue;

                    // Skip setting RetryMode if it is set to legacy but the DefaultConfigurationMode is not legacy.
                    // This will allow the retry mode to be configured from the DefaultConfiguration.
                    // This is a workaround to handle the inability to tell if RetryMode was explicitly set.
                    // if (string.Equals(property.Name, "RetryMode", StringComparison.Ordinal) &&
                    //     defaultConfig.RetryMode == RequestRetryMode.Legacy &&
                    //     config.DefaultConfigurationMode != DefaultConfigurationMode.Legacy)
                    //     continue;

                    singleArray[0] = property.GetMethod.Invoke(defaultConfig, emptyArray);
                    if (singleArray[0] != null)
                    {
                        property.SetMethod.Invoke(config, singleArray);
                    }
                }
            }

            // Setting RegionEndpoint only if ServiceURL was not set, because ServiceURL value will be lost otherwise
            if (options.Region != null)
            {
                config.ForceCustomRegion(options.Region.SystemName);
                if (string.IsNullOrEmpty(defaultConfig.ServiceURL))
                {
                    config.RegionEndpoint = options.Region;
                }
            }
            config.SetServiceURL(defaultConfig.ServiceURL);

            return config;
        }

        public AWSOptions GetNamedAWSOptionsOrDefault(string key)
        {
            if (key != null)
            {
                var servicesSection = s3Options.CurrentValue.ConfigurationSection.GetSection("Services");
                return servicesSection.GetAWSOptions(key);
            }

            return configuration.GetAWSOptions();
        }

        public async Task<AWSCredentials> GetWebIdentityCredentialsAsync(string serviceURL, JwtSecurityToken jwt, string policy)
        {
            if (jwt == null)
            {
                throw new ArgumentNullException(nameof(jwt));
            }
            if (jwt.ValidTo < DateTime.UtcNow)
            {
                throw new ArgumentException("JWT token is expired");
            }
            AmazonSecurityTokenServiceConfig amazonSecurityTokenServiceConfig = new AmazonSecurityTokenServiceConfig();
            amazonSecurityTokenServiceConfig.ServiceURL = serviceURL;
            var stsClient = new AmazonSecurityTokenServiceClient(new AnonymousAWSCredentials(), amazonSecurityTokenServiceConfig);

            var assumeRoleResult = await stsClient.AssumeRoleWithWebIdentityAsync(new AssumeRoleWithWebIdentityRequest
            {
                WebIdentityToken = jwt.RawData,
                // RoleArn = "arn:aws:iam::123456789012:role/RoleForTerradue",
                RoleSessionName = "MySession",
                DurationSeconds = Math.Max(900, jwt.ValidTo.Subtract(DateTime.UtcNow).Seconds),
                Policy = policy
            });
            return assumeRoleResult.Credentials;
        }

        public S3Configuration GetS3Configuration(S3Url s3Url)
        {
            return s3Options.CurrentValue?.GetS3Configuration(s3Url.ToString()).Value;
        }
    }
}
