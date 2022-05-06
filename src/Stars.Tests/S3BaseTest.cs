using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using Terradue.Stars.Services.Resources;

namespace Stars.Tests
{
    public abstract class S3BaseTest
    {
        protected readonly S3ClientFactory s3ClientFactory;
        protected S3BaseTest(S3ClientFactory s3ClientFactory)
        {
            this.s3ClientFactory = s3ClientFactory;
        }

        protected async Task CreateBucketAsync(string s3bucketUri)
        {
            var s3uri = S3Url.Parse(s3bucketUri);
            var client = s3ClientFactory.CreateS3Client(s3uri);
            var response = await client.PutBucketAsync(s3uri.Bucket);
        }

        protected async Task CopyLocalDataToBucketAsync(string filename, string s3destination)
        {
            var s3uri = S3Url.Parse(s3destination);
            var client = s3ClientFactory.CreateS3Client(s3uri);

            PutObjectRequest request = new PutObjectRequest();
            request.BucketName = s3uri.Bucket;
            request.Key = s3uri.Key;
            request.InputStream = File.Open(filename, FileMode.Open, FileAccess.Read);

            var response = await client.PutObjectAsync(request);

        }

    }
}