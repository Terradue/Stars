using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
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
using Stac;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Services.Resources
{
    public class S3Resource : IStreamResource, IDeletableResource
    {
        private S3Url s3Url;
        private bool? requester_pays;

        public S3Resource(S3Url url, IAmazonS3 client)
        {
            this.s3Url = url;
            Client = client;
        }

        public S3Resource(IAsset asset, IAmazonS3 client)
        {
            this.s3Url = S3Url.ParseUri(asset.Uri);
            this.requester_pays = asset.Properties.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                                                  .GetProperty<bool?>(Stac.Extensions.Storage.StorageStacExtension.RequesterPaysField);
            Client = client;
        }

        internal async Task LoadMetadata()
        {
            GetObjectMetadataRequest gomr = new GetObjectMetadataRequest();
            gomr.BucketName = s3Url.Bucket;
            gomr.Key = s3Url.Key;
            if (requester_pays.HasValue && requester_pays.Value)
                gomr.RequestPayer = RequestPayer.Requester;
            ObjectMetadata = await Client.GetObjectMetadataAsync(gomr);

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

        public bool? RequesterPays
        {
            get
            {
                return requester_pays;
            }

            set
            {
                requester_pays = value;
            }
        }

        public async Task<Stream> GetStreamAsync()
        {
            GetObjectRequest gor = new GetObjectRequest();
            gor.BucketName = s3Url.Bucket;
            gor.Key = s3Url.Key;
            if (requester_pays.HasValue && requester_pays.Value)
                gor.RequestPayer = RequestPayer.Requester;
            return (await Client.GetObjectAsync(gor)).ResponseStream;
        }

        public async Task<Stream> GetStreamAsync(long start, long end = -1)
        {
            var rangeRequest = new GetObjectRequest()
            {
                BucketName = s3Url.Bucket,
                Key = s3Url.Key,
                ByteRange = new ByteRange(start, end)
            };
            if (requester_pays.HasValue && requester_pays.Value)
                rangeRequest.RequestPayer = RequestPayer.Requester;
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

        public override string ToString()
        {
            return s3Url.ToString();
        }
    }
}