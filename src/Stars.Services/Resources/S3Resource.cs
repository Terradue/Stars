using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Microsoft.Extensions.Options;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Services.Resources
{
    public class S3Resource : IStreamResource
    {
        private S3Url s3Url;

        private S3Resource(S3Url url)
        {
            this.s3Url = url;
        }

        public static async Task<S3Resource> CreateAsync(S3Url url,
                                               S3Options s3Options,
                                               IIdentityProvider identityProvider)
        {
            S3Resource s3Resource = new S3Resource(url);
            s3Resource.Client = await GetS3ClientAsync(url, s3Options, identityProvider);
            
            return s3Resource;
        }

        public static async Task<AmazonS3Client> GetS3ClientAsync(S3Url s3Url, S3Options s3Options, IIdentityProvider identityProvider = null)
        {
            // Find S3 config if any
            S3Configuration s3Config = s3Options.GetS3Configuration(s3Url.Url.ToString());
            Uri endpoint = s3Config?.EndpointUrl;
            // no endpoint, use default from url
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

            AWSCredentials s3Creds = await GetCredentialsAsync(endpoint.ToString(), s3Config, identityProvider);

            AmazonS3Config awsS3Config = new AmazonS3Config();
            awsS3Config.ServiceURL = endpoint.ToString();
            awsS3Config.AuthenticationRegion = region;
            awsS3Config.ForcePathStyle = true;
            return new Amazon.S3.AmazonS3Client(s3Creds, awsS3Config);
        }

        private static async Task<AWSCredentials> GetCredentialsAsync(string endpoint, S3Configuration s3Config, IIdentityProvider identityProvider = null)
        {
            // If there is a config for the S3 provider, we only use that one
            if (s3Config != null && !string.IsNullOrEmpty(s3Config.AccessKey) && !string.IsNullOrEmpty(s3Config.SecretKey))
            {
                return new BasicAWSCredentials(s3Config.AccessKey, s3Config.SecretKey);
            }
            else
            {
                // Add the STS credentials provider from user context
                if (identityProvider != null)
                {
                    return await GetWebIdentityCredentialsAsync(endpoint, identityProvider.GetJwtSecurityToken(), null);
                }
            }
            return new AnonymousAWSCredentials();
        }

        private static async Task<AWSCredentials> GetWebIdentityCredentialsAsync(string endpoint, JwtSecurityToken jwt, string policy)
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

        internal async Task CacheMetadata()
        {
            ObjectMetadata = await Client.GetObjectMetadataAsync(s3Url.Bucket, s3Url.Key);
        }

        public ContentType ContentType => new ContentType(ObjectMetadata.Headers.ContentType);

        public ResourceType ResourceType => ResourceType.Unknown;

        public ulong ContentLength => Convert.ToUInt64(ObjectMetadata.Headers.ContentLength);

        public ContentDisposition ContentDisposition => new ContentDisposition(ObjectMetadata.Headers.ContentDisposition.ToString());

        public Uri Uri => s3Url.Url;

        public bool CanBeRanged => true;

        public AmazonS3Client Client { get; private set; }
        
        public GetObjectMetadataResponse ObjectMetadata { get; private set; }

        public async Task<Stream> GetStreamAsync()
        {
            return (await Client.GetObjectAsync(s3Url.Bucket, s3Url.Key)).ResponseStream;
        }

        public async Task<Stream> GetStreamAsync(long start, long end = -1)
        {
            var rangeRequest = new GetObjectRequest()
            {
                BucketName = s3Url.Bucket,
                Key = s3Url.Key,
                ByteRange = new ByteRange(start, end)
            };
            return (await Client.GetObjectAsync(rangeRequest)).ResponseStream;
        }

        internal bool SameBucket(S3Resource s3outputStreamResource)
        {
            throw new NotImplementedException();
        }

        internal Task CopyTo(S3Resource s3outputStreamResource)
        {
            throw new NotImplementedException();
        }
    }
}