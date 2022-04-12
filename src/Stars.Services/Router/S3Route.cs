using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Services.Router
{
    internal class S3Route : IResource, IStreamable
    {
        private S3Url s3Url;
        public AmazonS3Client client;

        public S3Route(S3Url url, AmazonS3Client client)
        {
            this.client = client;
        }

        private async Task CacheMetadata()
        {
            ObjectMetadata = await Client.GetObjectMetadataAsync(s3Url.Bucket, s3Url.Key);
        }

        public ContentType ContentType => new ContentType(ObjectMetadata.Headers.ContentType);

        public ResourceType ResourceType => ResourceType.Unknown;

        public ulong ContentLength => Convert.ToUInt64(ObjectMetadata.Headers.ContentLength);

        public ContentDisposition ContentDisposition => new ContentDisposition(ObjectMetadata.Headers.ContentDisposition.ToString());

        public Uri Uri => s3Url.Url;

        public bool CanBeRanged => true;

        public AmazonS3Client Client => client;
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
    }
}