using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
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

        internal async Task LoadMetadata(CancellationToken ct)
        {
            GetObjectMetadataRequest gomr = new GetObjectMetadataRequest();
            gomr.BucketName = s3Url.Bucket;
            gomr.Key = string.IsNullOrEmpty(s3Url.Key) ? "/" : s3Url.Key;
            if (requester_pays.HasValue && requester_pays.Value)
                gomr.RequestPayer = RequestPayer.Requester;
            ObjectMetadata = await Client.GetObjectMetadataAsync(gomr, ct);
        }

        public ContentType ContentType => ObjectMetadata?.Headers.ContentType != null ? new ContentType(ObjectMetadata?.Headers.ContentType) : null;

        public ResourceType ResourceType => ResourceType.Unknown;

        public ulong ContentLength => ObjectMetadata == null ? 0 : Convert.ToUInt64(ObjectMetadata.Headers.ContentLength);

        public ContentDisposition ContentDisposition => ObjectMetadata?.Headers.ContentDisposition != null ? new ContentDisposition(ObjectMetadata?.Headers.ContentDisposition.ToString()) : null;

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

        public async Task<Stream> GetStreamAsync(CancellationToken ct)
        {
            GetObjectRequest gor = new GetObjectRequest();
            gor.BucketName = s3Url.Bucket;
            gor.Key = s3Url.Key;
            if (requester_pays.HasValue && requester_pays.Value)
                gor.RequestPayer = RequestPayer.Requester;
            var gores = await Client.GetObjectAsync(gor, ct);
            return BlockingStream.StartBufferedStreamAsync(gores.ResponseStream, gores.ContentLength, ct);
        }

        public async Task<Stream> GetStreamAsync(long start, CancellationToken ct, long end = -1)
        {
            var rangeRequest = new GetObjectRequest()
            {
                BucketName = s3Url.Bucket,
                Key = s3Url.Key,
                ByteRange = new ByteRange(start, end)
            };
            if (requester_pays.HasValue && requester_pays.Value)
                rangeRequest.RequestPayer = RequestPayer.Requester;
            var gores = await Client.GetObjectAsync(rangeRequest, ct);
            return BlockingStream.StartBufferedStreamAsync(gores.ResponseStream, gores.ContentLength, ct);
        }

        internal bool SameBucket(S3Resource s3outputStreamResource)
        {
            return S3Uri.Bucket == s3outputStreamResource.S3Uri.Bucket;
        }

        internal async Task<S3Resource> CopyTo(S3Resource s3outputStreamResource, CancellationToken ct)
        {
            if (S3Uri.Endpoint != s3outputStreamResource.S3Uri.Endpoint)
            {
                throw new InvalidOperationException("Cannot copy between different endpoints");
            }
            if (this.ContentLength > (ulong)5 * 1024 * 1024 * 1024)
            {
                return await CopyToMultiPartAsync(s3outputStreamResource, ct);
            }
            await Client.CopyObjectAsync(s3Url.Bucket, s3Url.Key, s3outputStreamResource.S3Uri.Bucket, s3outputStreamResource.S3Uri.Key, ct);
            await s3outputStreamResource.LoadMetadata(ct);
            return s3outputStreamResource;
        }

        private async Task<S3Resource> CopyToMultiPartAsync(S3Resource s3outputStreamResource, CancellationToken ct)
        {
            // Create a list to store the upload part responses.
            List<UploadPartResponse> uploadResponses = new List<UploadPartResponse>();
            List<CopyPartResponse> copyResponses = new List<CopyPartResponse>();

            // Setup information required to initiate the multipart upload.
            InitiateMultipartUploadRequest initiateRequest =
                new InitiateMultipartUploadRequest
                {
                    BucketName = s3outputStreamResource.S3Uri.Bucket,
                    Key = s3outputStreamResource.S3Uri.Key
                };

            // Initiate the upload.
            InitiateMultipartUploadResponse initResponse =
                await Client.InitiateMultipartUploadAsync(initiateRequest);

            // Save the upload ID.
            String uploadId = initResponse.UploadId;

            // Get the size of the object.
            GetObjectMetadataRequest metadataRequest = new GetObjectMetadataRequest
            {
                BucketName = this.S3Uri.Bucket,
                Key = this.S3Uri.Key
            };

            GetObjectMetadataResponse metadataResponse =
                await Client.GetObjectMetadataAsync(metadataRequest);
            long objectSize = metadataResponse.ContentLength; // Length in bytes.

            // Copy the parts.
            long partSize = 5 * (long)Math.Pow(2, 20); // Part size is 5 MB.

            long bytePosition = 0;
            for (int i = 1; bytePosition < objectSize; i++)
            {
                CopyPartRequest copyRequest = new CopyPartRequest
                {
                    DestinationBucket = s3outputStreamResource.S3Uri.Bucket,
                    DestinationKey = s3outputStreamResource.S3Uri.Key,
                    SourceBucket = this.S3Uri.Bucket,
                    SourceKey = this.S3Uri.Key,
                    UploadId = uploadId,
                    FirstByte = bytePosition,
                    LastByte = bytePosition + partSize - 1 >= objectSize ? objectSize - 1 : bytePosition + partSize - 1,
                    PartNumber = i
                };

                copyResponses.Add(await Client.CopyPartAsync(copyRequest));

                bytePosition += partSize;
            }

            // Set up to complete the copy.
            CompleteMultipartUploadRequest completeRequest =
            new CompleteMultipartUploadRequest
            {
                BucketName = s3outputStreamResource.S3Uri.Bucket,
                Key = s3outputStreamResource.S3Uri.Key,
                UploadId = initResponse.UploadId
            };
            completeRequest.AddPartETags(copyResponses);

            // Complete the copy.
            CompleteMultipartUploadResponse completeUploadResponse =
                await Client.CompleteMultipartUploadAsync(completeRequest);

            await s3outputStreamResource.LoadMetadata(ct);
            return s3outputStreamResource;

        }

        public async Task DeleteAsync(CancellationToken ct)
        {
            await Client.DeleteObjectAsync(s3Url.Bucket, s3Url.Key, ct);
        }

        public override string ToString()
        {
            return s3Url.ToString();
        }
    }
}