using System;
using System.Threading.Tasks;
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
                                                                IIdentityProvider identityProvider)
        {
            var Client = await factory.CreateS3ClientAsync(url, identityProvider);
            S3Resource s3Resource = new S3Resource(url, Client);
            var reqp = Environment.GetEnvironmentVariable("AWS_REQUEST_PAYER");
            if (!string.IsNullOrEmpty(reqp) && reqp.Equals("requester", StringComparison.InvariantCultureIgnoreCase))
                s3Resource.RequesterPays = true;
            await s3Resource.LoadMetadata();
            return s3Resource;
        }

        public static async Task<S3Resource> CreateAndLoadAsync(this IS3ClientFactory factory,
                                                                S3Url url)
        {
            var Client = factory.CreateS3Client(url);
            S3Resource s3Resource = new S3Resource(url, Client);
            var reqp = Environment.GetEnvironmentVariable("AWS_REQUEST_PAYER");
            if (!string.IsNullOrEmpty(reqp) && reqp.Equals("requester", StringComparison.InvariantCultureIgnoreCase))
                s3Resource.RequesterPays = true;
            await s3Resource.LoadMetadata();
            return s3Resource;
        }

        public static async Task<S3Resource> CreateAndLoadAsync(this IS3ClientFactory factory,
                                                                IAsset asset)
        {
            var Client = factory.CreateS3Client(asset);
            S3Resource s3Resource = new S3Resource(asset, Client);
            var reqp = Environment.GetEnvironmentVariable("AWS_REQUEST_PAYER");
            if (!string.IsNullOrEmpty(reqp) && reqp.Equals("requester", StringComparison.InvariantCultureIgnoreCase))
                s3Resource.RequesterPays = true;
            await s3Resource.LoadMetadata();
            return s3Resource;
        }


    }
}