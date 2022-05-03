using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Services.Router
{
    public class DefaultResourceServiceProvider : IResourceServiceProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public DefaultResourceServiceProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<IResource> CreateAsync(Uri url)
        {
            var client = _serviceProvider.GetService<HttpClient>();

            HttpResponseMessage response = await client.GetAsync(url);

            // S3 resource case
            if (response.Headers.Any(h => h.Key.StartsWith("x-amz")))
            {
                S3Url s3Url = S3Url.ParseUri(url);
                IOptionsMonitor<S3Options> s3OptionsMonitor = _serviceProvider.GetService<IOptionsMonitor<S3Options>>();
                AmazonS3Client s3Client = await GetS3ClientAsync(s3Url, s3OptionsMonitor, null);
                return new S3Route(s3Url, s3Client);
            }

            return new HttpRoute(url, client, response.Content.Headers);
        }

        protected async Task<AmazonS3Client> GetS3ClientAsync(S3Url s3Url, IOptionsMonitor<S3Options>? s3OptionsMonitor, JwtSecurityToken jwt = null)
        {
            S3Configuration s3Config = s3OptionsMonitor.CurrentValue?.GetS3Configuration(s3Url.Url.ToString());
            Uri endpoint = s3Config?.EndpointUrl;
            if (endpoint == null)
            {
                endpoint = s3Url.EndpointUrl;
            }
            if (endpoint == null)
            {
                throw new InvalidOperationException($"No S3 endpoint found for URL {s3Url.Url}");
            }

            string region = s3Config?.Region;
            if (string.IsNullOrEmpty(region))
            {
                region = s3Url.Region;
            }

            AWSCredentials s3Creds = await GetCredentialsAsync(endpoint.ToString(), s3Config, jwt);

            AmazonS3Config awsS3Config = new AmazonS3Config();
            awsS3Config.ServiceURL = endpoint.ToString();
            awsS3Config.AuthenticationRegion = region;
            awsS3Config.ForcePathStyle = true;
            return new Amazon.S3.AmazonS3Client(s3Creds, awsS3Config);
        }

        private async Task<AWSCredentials> GetCredentialsAsync(string endpoint, S3Configuration s3Config, JwtSecurityToken jwt)
        {
            // If there is a config for the S3 provider, we only use that one
            if (s3Config != null && !string.IsNullOrEmpty(s3Config.AccessKey) && !string.IsNullOrEmpty(s3Config.SecretKey))
            {
                return new BasicAWSCredentials(s3Config.AccessKey, s3Config.SecretKey);
            }
            else
            {
                // Add the STS credentials provider from user context
                if (jwt != null)
                {
                    return await GetWebIdentityCredentialsAsync(endpoint, jwt, null);
                }
            }
            return new AnonymousAWSCredentials();
        }

        private async Task<AWSCredentials> GetWebIdentityCredentialsAsync(string endpoint, JwtSecurityToken jwt, string policy)
        {
            AmazonSecurityTokenServiceConfig amazonSecurityTokenServiceConfig = new AmazonSecurityTokenServiceConfig();
            amazonSecurityTokenServiceConfig.ServiceURL = endpoint;
            var stsClient = new AmazonSecurityTokenServiceClient(new AnonymousAWSCredentials(), amazonSecurityTokenServiceConfig);

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
    }
}