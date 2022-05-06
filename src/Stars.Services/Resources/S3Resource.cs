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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Services.Resources
{
    public class S3Resource : IStreamResource, IDeletableResource
    {
        private S3Url s3Url;

        public S3Resource(S3Url url, IAmazonS3 client)
        {
            this.s3Url = url;
            Client = client;
        }

        internal async Task LoadMetadata()
        {
            ObjectMetadata = await Client.GetObjectMetadataAsync(s3Url.Bucket, s3Url.Key);
        }

        public ContentType ContentType => new ContentType(ObjectMetadata?.Headers.ContentType);

        public ResourceType ResourceType => ResourceType.Unknown;

        public ulong ContentLength => ObjectMetadata == null ? 0 : Convert.ToUInt64(ObjectMetadata.Headers.ContentLength);

        public ContentDisposition ContentDisposition => new ContentDisposition(ObjectMetadata?.Headers.ContentDisposition.ToString());

        public S3Url S3Uri => s3Url;

        public Uri Uri => s3Url.Uri;

        public bool CanBeRanged => true;

        public IAmazonS3 Client { get; private set; }

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
            return S3Uri.Bucket == s3outputStreamResource.S3Uri.Bucket;
        }

        internal async Task<S3Resource> CopyTo(S3Resource s3outputStreamResource)
        {
            if (S3Uri.Endpoint != s3outputStreamResource.S3Uri.Endpoint)
            {
                throw new InvalidOperationException("Cannot copy between different endpoints");
            }
            await Client.CopyObjectAsync(s3Url.Bucket, s3Url.Key, s3outputStreamResource.S3Uri.Bucket, s3outputStreamResource.S3Uri.Key);
            return s3outputStreamResource;
        }

        public async Task Delete()
        {
            await Client.DeleteObjectAsync(s3Url.Bucket, s3Url.Key);
        }
    }
}