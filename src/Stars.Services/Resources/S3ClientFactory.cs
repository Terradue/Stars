using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.Runtime.Internal;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stac;
using Terradue.Stars.Interface;
using Terradue.Stars.Services.Credentials;
using Terradue.Stars.Services.Plugins;
using Terradue.Stars.Services.Resources;

namespace Terradue.Stars.Services.Resources
{
    public class S3ClientFactory
    {
        private readonly ILogger<S3ClientFactory> logger;
        private readonly IOptionsMonitor<S3Options> s3Options;
        private readonly IConfiguration configuration;

        public S3ClientFactory(ILogger<S3ClientFactory> logger,
                                IOptionsMonitor<S3Options> s3Options,
                                IConfiguration configuration)
        {
            this.logger = logger;
            this.s3Options = s3Options;
            this.configuration = configuration;
        }

        public IAmazonS3 CreateS3Client(string serviceName)
        {
            var s3Section = configuration.GetSection("S3");
            var servicesSection = s3Section.GetSection("Services");
            AWSOptions awsOptions = servicesSection.GetAWSOptions(serviceName);
            if (awsOptions == null)
            {
                throw new Exception($"Service {serviceName} not found in S3 configuration");
            }
            return awsOptions.CreateServiceClient<IAmazonS3>();
        }

        public async Task<IAmazonS3> CreateS3ClientAsync(IAsset asset)
        {
            S3Url s3Url = S3Url.ParseUri(asset.Uri);
            AmazonS3Config amazonS3Config = GetAmazonS3Config(s3Url);
            var region = asset.Properties.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                                                  .GetProperty<string>(Stac.Extensions.Storage.StorageStacExtension.RegionField);
            if (!string.IsNullOrEmpty(region))
            {
                amazonS3Config.RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(region);
            }
            AWSCredentials credentials = CreateCredentials(s3Url);

            return await CreateS3ClientAsync(asset, credentials, amazonS3Config);
        }

        public string GetPersonalStoragePolicyName(IIdentityProvider identityProvider)
        {
            return string.Format(s3Options.CurrentValue.Policies.PersonalStoragePolicyId, identityProvider.Name);
        }

        private async Task<IAmazonS3> CreateS3ClientAsync(IAsset asset, AWSCredentials credentials, AmazonS3Config amazonS3Config)
        {
            var s3Url = S3Url.ParseUri(asset.Uri);
            IAmazonS3 client = new AmazonS3Client(credentials, amazonS3Config);
            GetObjectMetadataRequest gblr = new GetObjectMetadataRequest();
            gblr.BucketName = s3Url.Bucket;
            gblr.Key = s3Url.Key;
            if (asset.Properties.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                                                  .GetProperty<bool>(Stac.Extensions.Storage.StorageStacExtension.RequesterPaysField))
            {
                gblr.RequestPayer = RequestPayer.Requester;
            }
            if (s3Options.CurrentValue.AdaptClientRegion)
            {
                return await AdaptClientRegion(asset, credentials, amazonS3Config, client, gblr);
            }
            return client;
        }

        private async Task<IAmazonS3> AdaptClientRegion(IAsset asset, AWSCredentials credentials, AmazonS3Config amazonS3Config, IAmazonS3 client, GetObjectMetadataRequest gblr)
        {
            try
            {
                var metadataResponse = await client.GetObjectMetadataAsync(gblr);
                return client;
            }
            catch (AmazonS3Exception e)
            {
                if (e.InnerException is HttpErrorResponseException)
                {
                    var here = e.InnerException as HttpErrorResponseException;
                    if (here.Response.GetHeaderValue("x-amz-bucket-region") != amazonS3Config.RegionEndpoint.SystemName)
                    {
                        logger.LogInformation($"Bucket region {here.Response.GetHeaderValue("x-amz-bucket-region")} differs from configured region {amazonS3Config.RegionEndpoint.SystemName}. Auto-adapting.");
                        amazonS3Config.RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(here.Response.GetHeaderValue("x-amz-bucket-region"));
                        return await CreateS3ClientAsync(asset, credentials, amazonS3Config);
                    }
                }
            }
            return client;
        }

        private async Task<IAmazonS3> AdaptClientRegion(S3Url s3Url, AWSCredentials credentials, AmazonS3Config amazonS3Config, IAmazonS3 client)
        {
            Task task = null;
            if (string.IsNullOrEmpty(s3Url.Key))
            {
                GetBucketLocationRequest gblr = new GetBucketLocationRequest();
                gblr.BucketName = s3Url.Bucket;
                task = client.GetBucketLocationAsync(gblr);
            }
            else
            {
                GetObjectMetadataRequest gomr = new GetObjectMetadataRequest();
                gomr.BucketName = s3Url.Bucket;
                gomr.Key = s3Url.Key;
                task = client.GetObjectMetadataAsync(gomr);
            }
            try
            {
                await task;
                return client;
            }
            catch (AmazonS3Exception e)
            {
                if (e.InnerException is HttpErrorResponseException)
                {
                    var here = e.InnerException as HttpErrorResponseException;
                    if (here.Response.StatusCode == System.Net.HttpStatusCode.Moved || here.Response.StatusCode == System.Net.HttpStatusCode.MovedPermanently)
                    {
                        if (here.Response.GetHeaderValue("x-amz-bucket-region") != amazonS3Config.RegionEndpoint?.SystemName)
                        {
                            logger.LogInformation($"Bucket region {here.Response.GetHeaderValue("x-amz-bucket-region")} differs from configured region {amazonS3Config.RegionEndpoint?.SystemName}. Auto-adapting.");
                            amazonS3Config.RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(here.Response.GetHeaderValue("x-amz-bucket-region"));
                            return await CreateS3ClientAsync(s3Url, credentials, amazonS3Config);
                        }
                    }
                }
                throw e;
            }
            return client;
        }

        private async Task<IAmazonS3> CreateS3ClientAsync(S3Url s3Url, AWSCredentials credentials, AmazonS3Config amazonS3Config)
        {
            IAmazonS3 client = new AmazonS3Client(credentials, amazonS3Config);

            if (s3Options.CurrentValue.AdaptClientRegion)
            {
                return await AdaptClientRegion(s3Url, credentials, amazonS3Config, client);
            }
            return client;
        }

        public IAmazonS3 CreateS3Client(AWSCredentials credentials, AmazonS3Config amazonS3Config)
        {
            IAmazonS3 client = new AmazonS3Client(credentials, amazonS3Config);
            return client;
        }

        public async Task<IAmazonS3> CreateS3ClientAsync(S3Url s3Url)
        {
            AmazonS3Config amazonS3Config = GetAmazonS3Config(s3Url);
            AWSCredentials credentials = CreateCredentials(s3Url);
            if (s3Options.CurrentValue.AdaptClientRegion)
            {
                return await CreateS3ClientAsync(s3Url, credentials, amazonS3Config);
            }

            return CreateS3Client(credentials, amazonS3Config);
        }

        public async Task<IAmazonS3> CreateS3ClientAsync(S3Url s3Url,
                                                         IIdentityProvider identityProvider)
        {
            AmazonS3Config amazonS3Config = GetAmazonS3Config(s3Url);
            AWSCredentials credentials = await GetWebIdentityCredentialsAsync(amazonS3Config.ServiceURL,
                                                                          identityProvider.GetJwtSecurityToken(),
                                                                          null);
            if (credentials == null)
                credentials = CreateCredentials(s3Url);

            if (s3Options.CurrentValue.AdaptClientRegion)
            {
                return await CreateS3ClientAsync(s3Url, credentials, amazonS3Config);
            }

            return CreateS3Client(credentials, amazonS3Config);
        }

        /// <summary>
        /// Creates the AWSCredentials using either the profile indicated from the AWSOptions object
        /// of the SDK fallback credentials search.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public AWSCredentials CreateCredentials(S3Url s3Url)
        {
            var s3Configuration = s3Options.CurrentValue.GetS3Configuration(s3Url.ToString());
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
                    AWSCredentials result;
                    if (chain.TryGetAWSCredentials(options.Profile, out result))
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

            var credentials = FallbackCredentialsFactory.GetCredentials();
            if (credentials == null)
            {
                logger?.LogError("Last effort to find AWS Credentials with AWS SDK's default credential search failed");
                throw new AmazonClientException("Failed to find AWS Credentials for constructing AWS service client");
            }
            else
            {
                logger?.LogInformation("Found credentials using the AWS SDK's default credential search");
            }

            return credentials;
        }

        public AmazonS3Config GetAmazonS3Config(S3Url s3Url)
        {
            var s3Configuration = s3Options.CurrentValue.GetS3Configuration(s3Url.ToString());
            AWSOptions awsOptions = GetNamedAWSOptionsOrDefault(s3Configuration.Key);

            if (awsOptions == null)
            {
                throw new Exception($"Service for {s3Url} not found in S3 configuration");
            }

            // If AWSOptions has no ServiceURL, try to use the derived one from the S3Url
            if (string.IsNullOrEmpty(awsOptions.DefaultClientConfig.ServiceURL) && s3Url.EndpointUrl != null)
            {
                awsOptions.DefaultClientConfig.ServiceURL = s3Url.EndpointUrl.ToString();
            }

            if (!string.IsNullOrEmpty(s3Configuration.Value?.AccessKey) && !string.IsNullOrEmpty(s3Configuration.Value?.SecretKey))
            {
                awsOptions.Credentials = new BasicAWSCredentials(s3Configuration.Value.AccessKey, s3Configuration.Value.SecretKey);
            }

            AmazonS3Config amazonS3Config = CreateS3Configuration(awsOptions);

            if (s3Configuration.Value?.ForcePathStyle == true || s3Url.PathStyle)
            {
                amazonS3Config.ForcePathStyle = true;
            }

            if (string.IsNullOrEmpty(amazonS3Config.ServiceURL))
            {
                logger?.LogInformation($"No Service URL configured, defaulting to {Amazon.RegionEndpoint.USEast1}");
                amazonS3Config.RegionEndpoint = Amazon.RegionEndpoint.USEast1;
                // amazonS3Config.ServiceURL = "https://s3.us-east-1.amazonaws.com";
            }

            amazonS3Config.AllowAutoRedirect = true;
            amazonS3Config.RetryMode = RequestRetryMode.Standard;

            return amazonS3Config;
        }

        /// <summary>
        /// Creates the ClientConfig object for the service client.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        private static AmazonS3Config CreateS3Configuration(AWSOptions options)
        {
            AmazonS3Config config = new AmazonS3Config();

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
            if (options.Region != null && string.IsNullOrEmpty(defaultConfig.ServiceURL))
            {
                config.RegionEndpoint = options.Region;
            }

            return config;
        }

        public AWSOptions GetNamedAWSOptionsOrDefault(string key)
        {
            if (key != null)
            {
                var servicesSection = s3Options.CurrentValue.ConfigurationSection.GetSection("Services");
                return servicesSection.GetAWSOptions(key);
            }

            return s3Options.CurrentValue.RootConfiguration.GetAWSOptions();
        }


        public async Task<AWSCredentials> GetWebIdentityCredentialsAsync(string serviceURL, JwtSecurityToken jwt, string policy)
        {
            AmazonSecurityTokenServiceConfig amazonSecurityTokenServiceConfig = new AmazonSecurityTokenServiceConfig();
            amazonSecurityTokenServiceConfig.ServiceURL = serviceURL;
            var stsClient = new AmazonSecurityTokenServiceClient(new AnonymousAWSCredentials(), amazonSecurityTokenServiceConfig);

            try
            {
                var assumeRoleResult = await stsClient.AssumeRoleWithWebIdentityAsync(new AssumeRoleWithWebIdentityRequest
                {
                    WebIdentityToken = jwt.RawData,
                    RoleArn = "arn:aws:iam::123456789012:role/RoleForTerradue",
                    RoleSessionName = "MySession",
                    DurationSeconds = 3600,
                    Policy = policy
                });
                return assumeRoleResult.Credentials;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Cannot get the Web Identity");
                return null;
            }


        }

    }
}