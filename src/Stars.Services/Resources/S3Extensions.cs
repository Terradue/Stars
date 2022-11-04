using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Services.Resources
{
    public static class S3Extensions
    {

        public static async Task<S3Resource> CreateAsync(this IS3ClientFactory factory,
                                                         S3Url url,
                                                         IIdentityProvider identityProvider)
        {
            var Client = await factory.CreateS3ClientAsync(url, identityProvider);
            S3Resource s3Resource = new S3Resource(url, Client);
            var reqp = Environment.GetEnvironmentVariable("AWS_REQUEST_PAYER");
            if (!string.IsNullOrEmpty(reqp) && reqp.Equals("requester", StringComparison.InvariantCultureIgnoreCase))
                s3Resource.RequesterPays = true;
            return s3Resource;
        }

        public static async Task<S3Resource> CreateAsync(this IS3ClientFactory factory,
                                                         S3Url url)
        {
            var client = factory.CreateS3Client(url);
            S3Resource s3Resource = new S3Resource(url, client);
            var reqp = Environment.GetEnvironmentVariable("AWS_REQUEST_PAYER");
            if (!string.IsNullOrEmpty(reqp) && reqp.Equals("requester", StringComparison.InvariantCultureIgnoreCase))
                s3Resource.RequesterPays = true;
            return s3Resource;
        }

        public static async Task<S3Resource> CreateAsync(this IS3ClientFactory factory,
                                                         IAsset asset)
        {
            var Client = factory.CreateS3Client(S3Url.ParseUri(asset.Uri));
            S3Resource s3Resource = new S3Resource(asset, Client);
            var reqp = Environment.GetEnvironmentVariable("AWS_REQUEST_PAYER");
            if (!string.IsNullOrEmpty(reqp) && reqp.Equals("requester", StringComparison.InvariantCultureIgnoreCase))
                s3Resource.RequesterPays = true;
            return s3Resource;
        }

        public static async Task<S3Resource> CreateAndLoadAsync(this IS3ClientFactory factory,
                                                                S3Url url,
                                                                IIdentityProvider identityProvider,
                                                                CancellationToken ct)
        {
            var Client = await factory.CreateS3ClientAsync(url, identityProvider);
            S3Resource s3Resource = new S3Resource(url, Client);
            var reqp = Environment.GetEnvironmentVariable("AWS_REQUEST_PAYER");
            if (!string.IsNullOrEmpty(reqp) && reqp.Equals("requester", StringComparison.InvariantCultureIgnoreCase))
                s3Resource.RequesterPays = true;
            await s3Resource.LoadMetadata(ct);
            return s3Resource;
        }

        public static async Task<S3Resource> CreateAndLoadAsync(this IS3ClientFactory factory,
                                                                S3Url url,
                                                                CancellationToken ct)
        {
            var Client = factory.CreateS3Client(url);
            S3Resource s3Resource = new S3Resource(url, Client);
            var reqp = Environment.GetEnvironmentVariable("AWS_REQUEST_PAYER");
            if (!string.IsNullOrEmpty(reqp) && reqp.Equals("requester", StringComparison.InvariantCultureIgnoreCase))
                s3Resource.RequesterPays = true;
            await s3Resource.LoadMetadata(ct);
            return s3Resource;
        }

        public static async Task<S3Resource> CreateAndLoadAsync(this IS3ClientFactory factory,
                                                                IAsset asset,
                                                                CancellationToken ct)
        {
            var Client = factory.CreateS3Client(asset);
            S3Resource s3Resource = new S3Resource(asset, Client);
            var reqp = Environment.GetEnvironmentVariable("AWS_REQUEST_PAYER");
            if (!string.IsNullOrEmpty(reqp) && reqp.Equals("requester", StringComparison.InvariantCultureIgnoreCase))
                s3Resource.RequesterPays = true;
            await s3Resource.LoadMetadata(ct);
            return s3Resource;
        }

        public static string GeneratePreSignedURL(this IAmazonS3 client, S3Url s3Url, double duration = 168)
        {
            GetPreSignedUrlRequest request1 = new GetPreSignedUrlRequest
            {
                BucketName = s3Url.Bucket,
                Key = s3Url.Key,
                Expires = DateTime.UtcNow.AddHours(duration)
            };
            return client.GetPreSignedURL(request1);
        }


    }
}