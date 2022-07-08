using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.Runtime.Internal;
using Amazon.S3;
using Amazon.S3.Model;

namespace Terradue.Stars.Services.Resources
{
    public class S3Client : IAmazonS3
    {
        private IAmazonS3 _client;
        private readonly S3Configuration _s3Config;
        private readonly IS3ClientFactory _s3ClientFactory;

        public S3Client(S3Configuration s3Config, IS3ClientFactory s3ClientFactory)
        {
            _client = new AmazonS3Client(s3Config.AWSCredentials, s3Config.AmazonS3Config);
            _s3Config = s3Config;
            _s3ClientFactory = s3ClientFactory;
        }

        public async Task AdaptS3Config(AmazonS3Exception exception)
        {
            if (exception.InnerException is HttpErrorResponseException)
            {
                var here = exception.InnerException as HttpErrorResponseException;
                if (here.Response.StatusCode == System.Net.HttpStatusCode.Moved || here.Response.StatusCode == System.Net.HttpStatusCode.MovedPermanently)
                {
                    if (here.Response.GetHeaderValue("x-amz-bucket-region") != _s3Config.AmazonS3Config.RegionEndpoint?.SystemName)
                    {
                        // logger.LogInformation($"Bucket region {here.Response.GetHeaderValue("x-amz-bucket-region")} differs from configured region {amazonS3Config.RegionEndpoint?.SystemName}. Auto-adapting.");
                        _s3Config.AmazonS3Config.RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(here.Response.GetHeaderValue("x-amz-bucket-region"));
                        _client = new AmazonS3Client(_s3Config.AWSCredentials, _s3Config.AmazonS3Config);
                        return;
                    }
                }
            }
        }

        public IS3PaginatorFactory Paginators => _client.Paginators;

        public IClientConfig Config => _client.Config;

        public S3Configuration S3Configuration => _s3Config;

        public Task<AbortMultipartUploadResponse> AbortMultipartUploadAsync(string bucketName, string key, string uploadId, CancellationToken cancellationToken = default)
        {
            return _client.AbortMultipartUploadAsync(bucketName, key, uploadId, cancellationToken);
        }

        public Task<AbortMultipartUploadResponse> AbortMultipartUploadAsync(AbortMultipartUploadRequest request, CancellationToken cancellationToken = default)
        {
            return _client.AbortMultipartUploadAsync(request, cancellationToken);
        }

        public Task<CompleteMultipartUploadResponse> CompleteMultipartUploadAsync(CompleteMultipartUploadRequest request, CancellationToken cancellationToken = default)
        {
            return _client.CompleteMultipartUploadAsync(request, cancellationToken);
        }

        public Task<CopyObjectResponse> CopyObjectAsync(string sourceBucket, string sourceKey, string destinationBucket, string destinationKey, CancellationToken cancellationToken = default)
        {
            return _client.CopyObjectAsync(sourceBucket, sourceKey, destinationBucket, destinationKey, cancellationToken);
        }

        public Task<CopyObjectResponse> CopyObjectAsync(string sourceBucket, string sourceKey, string sourceVersionId, string destinationBucket, string destinationKey, CancellationToken cancellationToken = default)
        {
            return _client.CopyObjectAsync(sourceBucket, sourceKey, sourceVersionId, destinationBucket, destinationKey, cancellationToken);
        }

        public Task<CopyObjectResponse> CopyObjectAsync(CopyObjectRequest request, CancellationToken cancellationToken = default)
        {
            return _client.CopyObjectAsync(request, cancellationToken);
        }

        public Task<CopyPartResponse> CopyPartAsync(string sourceBucket, string sourceKey, string destinationBucket, string destinationKey, string uploadId, CancellationToken cancellationToken = default)
        {
            return _client.CopyPartAsync(sourceBucket, sourceKey, destinationBucket, destinationKey, uploadId, cancellationToken);
        }

        public Task<CopyPartResponse> CopyPartAsync(string sourceBucket, string sourceKey, string sourceVersionId, string destinationBucket, string destinationKey, string uploadId, CancellationToken cancellationToken = default)
        {
            return _client.CopyPartAsync(sourceBucket, sourceKey, sourceVersionId, destinationBucket, destinationKey, uploadId, cancellationToken);
        }

        public Task<CopyPartResponse> CopyPartAsync(CopyPartRequest request, CancellationToken cancellationToken = default)
        {
            return _client.CopyPartAsync(request, cancellationToken);
        }

        public Task DeleteAsync(string bucketName, string objectKey, IDictionary<string, object> additionalProperties, CancellationToken cancellationToken = default)
        {
            return _client.DeleteAsync(bucketName, objectKey, additionalProperties, cancellationToken);
        }

        public Task<DeleteBucketAnalyticsConfigurationResponse> DeleteBucketAnalyticsConfigurationAsync(DeleteBucketAnalyticsConfigurationRequest request, CancellationToken cancellationToken = default)
        {
            return _client.DeleteBucketAnalyticsConfigurationAsync(request, cancellationToken);
        }

        public Task<DeleteBucketResponse> DeleteBucketAsync(string bucketName, CancellationToken cancellationToken = default)
        {
            return _client.DeleteBucketAsync(bucketName, cancellationToken);
        }

        public Task<DeleteBucketResponse> DeleteBucketAsync(DeleteBucketRequest request, CancellationToken cancellationToken = default)
        {
            return _client.DeleteBucketAsync(request, cancellationToken);
        }

        public Task<DeleteBucketEncryptionResponse> DeleteBucketEncryptionAsync(DeleteBucketEncryptionRequest request, CancellationToken cancellationToken = default)
        {
            return _client.DeleteBucketEncryptionAsync(request, cancellationToken);
        }

        public Task<DeleteBucketIntelligentTieringConfigurationResponse> DeleteBucketIntelligentTieringConfigurationAsync(DeleteBucketIntelligentTieringConfigurationRequest request, CancellationToken cancellationToken = default)
        {
            return _client.DeleteBucketIntelligentTieringConfigurationAsync(request, cancellationToken);
        }

        public Task<DeleteBucketInventoryConfigurationResponse> DeleteBucketInventoryConfigurationAsync(DeleteBucketInventoryConfigurationRequest request, CancellationToken cancellationToken = default)
        {
            return _client.DeleteBucketInventoryConfigurationAsync(request, cancellationToken);
        }

        public Task<DeleteBucketMetricsConfigurationResponse> DeleteBucketMetricsConfigurationAsync(DeleteBucketMetricsConfigurationRequest request, CancellationToken cancellationToken = default)
        {
            return _client.DeleteBucketMetricsConfigurationAsync(request, cancellationToken);
        }

        public Task<DeleteBucketOwnershipControlsResponse> DeleteBucketOwnershipControlsAsync(DeleteBucketOwnershipControlsRequest request, CancellationToken cancellationToken = default)
        {
            return _client.DeleteBucketOwnershipControlsAsync(request, cancellationToken);
        }

        public Task<DeleteBucketPolicyResponse> DeleteBucketPolicyAsync(string bucketName, CancellationToken cancellationToken = default)
        {
            return _client.DeleteBucketPolicyAsync(bucketName, cancellationToken);
        }

        public Task<DeleteBucketPolicyResponse> DeleteBucketPolicyAsync(DeleteBucketPolicyRequest request, CancellationToken cancellationToken = default)
        {
            return _client.DeleteBucketPolicyAsync(request, cancellationToken);
        }

        public Task<DeleteBucketReplicationResponse> DeleteBucketReplicationAsync(DeleteBucketReplicationRequest request, CancellationToken cancellationToken = default)
        {
            return _client.DeleteBucketReplicationAsync(request, cancellationToken);
        }

        public Task<DeleteBucketTaggingResponse> DeleteBucketTaggingAsync(string bucketName, CancellationToken cancellationToken = default)
        {
            return _client.DeleteBucketTaggingAsync(bucketName, cancellationToken);
        }

        public Task<DeleteBucketTaggingResponse> DeleteBucketTaggingAsync(DeleteBucketTaggingRequest request, CancellationToken cancellationToken = default)
        {
            return _client.DeleteBucketTaggingAsync(request, cancellationToken);
        }

        public Task<DeleteBucketWebsiteResponse> DeleteBucketWebsiteAsync(string bucketName, CancellationToken cancellationToken = default)
        {
            return _client.DeleteBucketWebsiteAsync(bucketName, cancellationToken);
        }

        public Task<DeleteBucketWebsiteResponse> DeleteBucketWebsiteAsync(DeleteBucketWebsiteRequest request, CancellationToken cancellationToken = default)
        {
            return _client.DeleteBucketWebsiteAsync(request, cancellationToken);
        }

        public Task<DeleteCORSConfigurationResponse> DeleteCORSConfigurationAsync(string bucketName, CancellationToken cancellationToken = default)
        {
            return _client.DeleteCORSConfigurationAsync(bucketName, cancellationToken);
        }

        public Task<DeleteCORSConfigurationResponse> DeleteCORSConfigurationAsync(DeleteCORSConfigurationRequest request, CancellationToken cancellationToken = default)
        {
            return _client.DeleteCORSConfigurationAsync(request, cancellationToken);
        }

        public Task<DeleteLifecycleConfigurationResponse> DeleteLifecycleConfigurationAsync(string bucketName, CancellationToken cancellationToken = default)
        {
            return _client.DeleteLifecycleConfigurationAsync(bucketName, cancellationToken);
        }

        public Task<DeleteLifecycleConfigurationResponse> DeleteLifecycleConfigurationAsync(DeleteLifecycleConfigurationRequest request, CancellationToken cancellationToken = default)
        {
            return _client.DeleteLifecycleConfigurationAsync(request, cancellationToken);
        }

        public Task<DeleteObjectResponse> DeleteObjectAsync(string bucketName, string key, CancellationToken cancellationToken = default)
        {
            return _client.DeleteObjectAsync(bucketName, key, cancellationToken);
        }

        public Task<DeleteObjectResponse> DeleteObjectAsync(string bucketName, string key, string versionId, CancellationToken cancellationToken = default)
        {
            return _client.DeleteObjectAsync(bucketName, key, versionId, cancellationToken);
        }

        public Task<DeleteObjectResponse> DeleteObjectAsync(DeleteObjectRequest request, CancellationToken cancellationToken = default)
        {
            return _client.DeleteObjectAsync(request, cancellationToken);
        }

        public Task<DeleteObjectsResponse> DeleteObjectsAsync(DeleteObjectsRequest request, CancellationToken cancellationToken = default)
        {
            return _client.DeleteObjectsAsync(request, cancellationToken);
        }

        public Task<DeleteObjectTaggingResponse> DeleteObjectTaggingAsync(DeleteObjectTaggingRequest request, CancellationToken cancellationToken = default)
        {
            return _client.DeleteObjectTaggingAsync(request, cancellationToken);
        }

        public Task<DeletePublicAccessBlockResponse> DeletePublicAccessBlockAsync(DeletePublicAccessBlockRequest request, CancellationToken cancellationToken = default)
        {
            return _client.DeletePublicAccessBlockAsync(request, cancellationToken);
        }

        public Task DeletesAsync(string bucketName, IEnumerable<string> objectKeys, IDictionary<string, object> additionalProperties, CancellationToken cancellationToken = default)
        {
            return _client.DeletesAsync(bucketName, objectKeys, additionalProperties, cancellationToken);
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        public Task<bool> DoesS3BucketExistAsync(string bucketName)
        {
            return _client.DoesS3BucketExistAsync(bucketName);
        }

        public Task DownloadToFilePathAsync(string bucketName, string objectKey, string filepath, IDictionary<string, object> additionalProperties, CancellationToken cancellationToken = default)
        {
            return _client.DownloadToFilePathAsync(bucketName, objectKey, filepath, additionalProperties, cancellationToken);
        }

        public Task EnsureBucketExistsAsync(string bucketName)
        {
            return _client.EnsureBucketExistsAsync(bucketName);
        }

        public string GeneratePreSignedURL(string bucketName, string objectKey, DateTime expiration, IDictionary<string, object> additionalProperties)
        {
            return _client.GeneratePreSignedURL(bucketName, objectKey, expiration, additionalProperties);
        }

        public Task<GetACLResponse> GetACLAsync(string bucketName, CancellationToken cancellationToken = default)
        {
            return _client.GetACLAsync(bucketName, cancellationToken);
        }

        public Task<GetACLResponse> GetACLAsync(GetACLRequest request, CancellationToken cancellationToken = default)
        {
            return _client.GetACLAsync(request, cancellationToken);
        }

        public Task<IList<string>> GetAllObjectKeysAsync(string bucketName, string prefix, IDictionary<string, object> additionalProperties)
        {
            return _client.GetAllObjectKeysAsync(bucketName, prefix, additionalProperties);
        }

        public Task<GetBucketAccelerateConfigurationResponse> GetBucketAccelerateConfigurationAsync(string bucketName, CancellationToken cancellationToken = default)
        {
            return _client.GetBucketAccelerateConfigurationAsync(bucketName, cancellationToken);
        }

        public Task<GetBucketAccelerateConfigurationResponse> GetBucketAccelerateConfigurationAsync(GetBucketAccelerateConfigurationRequest request, CancellationToken cancellationToken = default)
        {
            return _client.GetBucketAccelerateConfigurationAsync(request, cancellationToken);
        }

        public Task<GetBucketAnalyticsConfigurationResponse> GetBucketAnalyticsConfigurationAsync(GetBucketAnalyticsConfigurationRequest request, CancellationToken cancellationToken = default)
        {
            return _client.GetBucketAnalyticsConfigurationAsync(request, cancellationToken);
        }

        public Task<GetBucketEncryptionResponse> GetBucketEncryptionAsync(GetBucketEncryptionRequest request, CancellationToken cancellationToken = default)
        {
            return _client.GetBucketEncryptionAsync(request, cancellationToken);
        }

        public Task<GetBucketIntelligentTieringConfigurationResponse> GetBucketIntelligentTieringConfigurationAsync(GetBucketIntelligentTieringConfigurationRequest request, CancellationToken cancellationToken = default)
        {
            return _client.GetBucketIntelligentTieringConfigurationAsync(request, cancellationToken);
        }

        public Task<GetBucketInventoryConfigurationResponse> GetBucketInventoryConfigurationAsync(GetBucketInventoryConfigurationRequest request, CancellationToken cancellationToken = default)
        {
            return _client.GetBucketInventoryConfigurationAsync(request, cancellationToken);
        }

        public Task<GetBucketLocationResponse> GetBucketLocationAsync(string bucketName, CancellationToken cancellationToken = default)
        {
            return _client.GetBucketLocationAsync(bucketName, cancellationToken);
        }

        public Task<GetBucketLocationResponse> GetBucketLocationAsync(GetBucketLocationRequest request, CancellationToken cancellationToken = default)
        {
            return _client.GetBucketLocationAsync(request, cancellationToken);
        }

        public Task<GetBucketLoggingResponse> GetBucketLoggingAsync(string bucketName, CancellationToken cancellationToken = default)
        {
            return _client.GetBucketLoggingAsync(bucketName, cancellationToken);
        }

        public Task<GetBucketLoggingResponse> GetBucketLoggingAsync(GetBucketLoggingRequest request, CancellationToken cancellationToken = default)
        {
            return _client.GetBucketLoggingAsync(request, cancellationToken);
        }

        public Task<GetBucketMetricsConfigurationResponse> GetBucketMetricsConfigurationAsync(GetBucketMetricsConfigurationRequest request, CancellationToken cancellationToken = default)
        {
            return _client.GetBucketMetricsConfigurationAsync(request, cancellationToken);
        }

        public Task<GetBucketNotificationResponse> GetBucketNotificationAsync(string bucketName, CancellationToken cancellationToken = default)
        {
            return _client.GetBucketNotificationAsync(bucketName, cancellationToken);
        }

        public Task<GetBucketNotificationResponse> GetBucketNotificationAsync(GetBucketNotificationRequest request, CancellationToken cancellationToken = default)
        {
            return _client.GetBucketNotificationAsync(request, cancellationToken);
        }

        public Task<GetBucketOwnershipControlsResponse> GetBucketOwnershipControlsAsync(GetBucketOwnershipControlsRequest request, CancellationToken cancellationToken = default)
        {
            return _client.GetBucketOwnershipControlsAsync(request, cancellationToken);
        }

        public Task<GetBucketPolicyResponse> GetBucketPolicyAsync(string bucketName, CancellationToken cancellationToken = default)
        {
            return _client.GetBucketPolicyAsync(bucketName, cancellationToken);
        }

        public Task<GetBucketPolicyResponse> GetBucketPolicyAsync(GetBucketPolicyRequest request, CancellationToken cancellationToken = default)
        {
            return _client.GetBucketPolicyAsync(request, cancellationToken);
        }

        public Task<GetBucketPolicyStatusResponse> GetBucketPolicyStatusAsync(GetBucketPolicyStatusRequest request, CancellationToken cancellationToken = default)
        {
            return _client.GetBucketPolicyStatusAsync(request, cancellationToken);
        }

        public Task<GetBucketReplicationResponse> GetBucketReplicationAsync(GetBucketReplicationRequest request, CancellationToken cancellationToken = default)
        {
            return _client.GetBucketReplicationAsync(request, cancellationToken);
        }

        public Task<GetBucketRequestPaymentResponse> GetBucketRequestPaymentAsync(string bucketName, CancellationToken cancellationToken = default)
        {
            return _client.GetBucketRequestPaymentAsync(bucketName, cancellationToken);
        }

        public Task<GetBucketRequestPaymentResponse> GetBucketRequestPaymentAsync(GetBucketRequestPaymentRequest request, CancellationToken cancellationToken = default)
        {
            return _client.GetBucketRequestPaymentAsync(request, cancellationToken);
        }

        public Task<GetBucketTaggingResponse> GetBucketTaggingAsync(GetBucketTaggingRequest request, CancellationToken cancellationToken = default)
        {
            return _client.GetBucketTaggingAsync(request, cancellationToken);
        }

        public Task<GetBucketVersioningResponse> GetBucketVersioningAsync(string bucketName, CancellationToken cancellationToken = default)
        {
            return _client.GetBucketVersioningAsync(bucketName, cancellationToken);
        }

        public Task<GetBucketVersioningResponse> GetBucketVersioningAsync(GetBucketVersioningRequest request, CancellationToken cancellationToken = default)
        {
            return _client.GetBucketVersioningAsync(request, cancellationToken);
        }

        public Task<GetBucketWebsiteResponse> GetBucketWebsiteAsync(string bucketName, CancellationToken cancellationToken = default)
        {
            return _client.GetBucketWebsiteAsync(bucketName, cancellationToken);
        }

        public Task<GetBucketWebsiteResponse> GetBucketWebsiteAsync(GetBucketWebsiteRequest request, CancellationToken cancellationToken = default)
        {
            return _client.GetBucketWebsiteAsync(request, cancellationToken);
        }

        public Task<GetCORSConfigurationResponse> GetCORSConfigurationAsync(string bucketName, CancellationToken cancellationToken = default)
        {
            return _client.GetCORSConfigurationAsync(bucketName, cancellationToken);
        }

        public Task<GetCORSConfigurationResponse> GetCORSConfigurationAsync(GetCORSConfigurationRequest request, CancellationToken cancellationToken = default)
        {
            return _client.GetCORSConfigurationAsync(request, cancellationToken);
        }

        public Task<GetLifecycleConfigurationResponse> GetLifecycleConfigurationAsync(string bucketName, CancellationToken cancellationToken = default)
        {
            return _client.GetLifecycleConfigurationAsync(bucketName, cancellationToken);
        }

        public Task<GetLifecycleConfigurationResponse> GetLifecycleConfigurationAsync(GetLifecycleConfigurationRequest request, CancellationToken cancellationToken = default)
        {
            return _client.GetLifecycleConfigurationAsync(request, cancellationToken);
        }

        public Task<GetObjectResponse> GetObjectAsync(string bucketName, string key, CancellationToken cancellationToken = default)
        {
            return _client.GetObjectAsync(bucketName, key, cancellationToken);
        }

        public Task<GetObjectResponse> GetObjectAsync(string bucketName, string key, string versionId, CancellationToken cancellationToken = default)
        {
            return _client.GetObjectAsync(bucketName, key, versionId, cancellationToken);
        }

        public Task<GetObjectResponse> GetObjectAsync(GetObjectRequest request, CancellationToken cancellationToken = default)
        {
            return _client.GetObjectAsync(request, cancellationToken);
        }

        public Task<GetObjectAttributesResponse> GetObjectAttributesAsync(GetObjectAttributesRequest request, CancellationToken cancellationToken = default)
        {
            return _client.GetObjectAttributesAsync(request, cancellationToken);
        }

        public Task<GetObjectLegalHoldResponse> GetObjectLegalHoldAsync(GetObjectLegalHoldRequest request, CancellationToken cancellationToken = default)
        {
            return _client.GetObjectLegalHoldAsync(request, cancellationToken);
        }

        public Task<GetObjectLockConfigurationResponse> GetObjectLockConfigurationAsync(GetObjectLockConfigurationRequest request, CancellationToken cancellationToken = default)
        {
            return _client.GetObjectLockConfigurationAsync(request, cancellationToken);
        }

        public Task<GetObjectMetadataResponse> GetObjectMetadataAsync(string bucketName, string key, CancellationToken cancellationToken = default)
        {
            return _client.GetObjectMetadataAsync(bucketName, key, cancellationToken);
        }

        public Task<GetObjectMetadataResponse> GetObjectMetadataAsync(string bucketName, string key, string versionId, CancellationToken cancellationToken = default)
        {
            return _client.GetObjectMetadataAsync(bucketName, key, versionId, cancellationToken);
        }

        public Task<GetObjectMetadataResponse> GetObjectMetadataAsync(GetObjectMetadataRequest request, CancellationToken cancellationToken = default)
        {
            return _client.GetObjectMetadataAsync(request, cancellationToken);
        }

        public Task<GetObjectRetentionResponse> GetObjectRetentionAsync(GetObjectRetentionRequest request, CancellationToken cancellationToken = default)
        {
            return _client.GetObjectRetentionAsync(request, cancellationToken);
        }

        public Task<Stream> GetObjectStreamAsync(string bucketName, string objectKey, IDictionary<string, object> additionalProperties, CancellationToken cancellationToken = default)
        {
            return _client.GetObjectStreamAsync(bucketName, objectKey, additionalProperties, cancellationToken);
        }

        public Task<GetObjectTaggingResponse> GetObjectTaggingAsync(GetObjectTaggingRequest request, CancellationToken cancellationToken = default)
        {
            return _client.GetObjectTaggingAsync(request, cancellationToken);
        }

        public Task<GetObjectTorrentResponse> GetObjectTorrentAsync(string bucketName, string key, CancellationToken cancellationToken = default)
        {
            return _client.GetObjectTorrentAsync(bucketName, key, cancellationToken);
        }

        public Task<GetObjectTorrentResponse> GetObjectTorrentAsync(GetObjectTorrentRequest request, CancellationToken cancellationToken = default)
        {
            return _client.GetObjectTorrentAsync(request, cancellationToken);
        }

        public string GetPreSignedURL(GetPreSignedUrlRequest request)
        {
            return _client.GetPreSignedURL(request);
        }

        public Task<GetPublicAccessBlockResponse> GetPublicAccessBlockAsync(GetPublicAccessBlockRequest request, CancellationToken cancellationToken = default)
        {
            return _client.GetPublicAccessBlockAsync(request, cancellationToken);
        }

        public Task<InitiateMultipartUploadResponse> InitiateMultipartUploadAsync(string bucketName, string key, CancellationToken cancellationToken = default)
        {
            return _client.InitiateMultipartUploadAsync(bucketName, key, cancellationToken);
        }

        public Task<InitiateMultipartUploadResponse> InitiateMultipartUploadAsync(InitiateMultipartUploadRequest request, CancellationToken cancellationToken = default)
        {
            return _client.InitiateMultipartUploadAsync(request, cancellationToken);
        }

        public Task<ListBucketAnalyticsConfigurationsResponse> ListBucketAnalyticsConfigurationsAsync(ListBucketAnalyticsConfigurationsRequest request, CancellationToken cancellationToken = default)
        {
            return _client.ListBucketAnalyticsConfigurationsAsync(request, cancellationToken);
        }

        public Task<ListBucketIntelligentTieringConfigurationsResponse> ListBucketIntelligentTieringConfigurationsAsync(ListBucketIntelligentTieringConfigurationsRequest request, CancellationToken cancellationToken = default)
        {
            return _client.ListBucketIntelligentTieringConfigurationsAsync(request, cancellationToken);
        }

        public Task<ListBucketInventoryConfigurationsResponse> ListBucketInventoryConfigurationsAsync(ListBucketInventoryConfigurationsRequest request, CancellationToken cancellationToken = default)
        {
            return _client.ListBucketInventoryConfigurationsAsync(request, cancellationToken);
        }

        public Task<ListBucketMetricsConfigurationsResponse> ListBucketMetricsConfigurationsAsync(ListBucketMetricsConfigurationsRequest request, CancellationToken cancellationToken = default)
        {
            return _client.ListBucketMetricsConfigurationsAsync(request, cancellationToken);
        }

        public Task<ListBucketsResponse> ListBucketsAsync(CancellationToken cancellationToken = default)
        {
            return _client.ListBucketsAsync(cancellationToken);
        }

        public Task<ListBucketsResponse> ListBucketsAsync(ListBucketsRequest request, CancellationToken cancellationToken = default)
        {
            return _client.ListBucketsAsync(request, cancellationToken);
        }

        public Task<ListMultipartUploadsResponse> ListMultipartUploadsAsync(string bucketName, CancellationToken cancellationToken = default)
        {
            return _client.ListMultipartUploadsAsync(bucketName, cancellationToken);
        }

        public Task<ListMultipartUploadsResponse> ListMultipartUploadsAsync(string bucketName, string prefix, CancellationToken cancellationToken = default)
        {
            return _client.ListMultipartUploadsAsync(bucketName, prefix, cancellationToken);
        }

        public Task<ListMultipartUploadsResponse> ListMultipartUploadsAsync(ListMultipartUploadsRequest request, CancellationToken cancellationToken = default)
        {
            return _client.ListMultipartUploadsAsync(request, cancellationToken);
        }

        public Task<ListObjectsResponse> ListObjectsAsync(string bucketName, CancellationToken cancellationToken = default)
        {
            return _client.ListObjectsAsync(bucketName, cancellationToken);
        }

        public Task<ListObjectsResponse> ListObjectsAsync(string bucketName, string prefix, CancellationToken cancellationToken = default)
        {
            return _client.ListObjectsAsync(bucketName, prefix, cancellationToken);
        }

        public Task<ListObjectsResponse> ListObjectsAsync(ListObjectsRequest request, CancellationToken cancellationToken = default)
        {
            return _client.ListObjectsAsync(request, cancellationToken);
        }

        public async Task<ListObjectsV2Response> ListObjectsV2Async(ListObjectsV2Request request, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _client.ListObjectsV2Async(request, cancellationToken);
            }
            catch (Amazon.S3.AmazonS3Exception s3ex)
            {
                if (s3ex.ErrorCode == "PermanentRedirect")
                {
                    await AdaptS3Config(s3ex);
                    return await _client.ListObjectsV2Async(request, cancellationToken);
                }
                else
                {
                    throw;
                }
            }
        }

        public Task<ListPartsResponse> ListPartsAsync(string bucketName, string key, string uploadId, CancellationToken cancellationToken = default)
        {
            return _client.ListPartsAsync(bucketName, key, uploadId, cancellationToken);
        }

        public Task<ListPartsResponse> ListPartsAsync(ListPartsRequest request, CancellationToken cancellationToken = default)
        {
            return _client.ListPartsAsync(request, cancellationToken);
        }

        public Task<ListVersionsResponse> ListVersionsAsync(string bucketName, CancellationToken cancellationToken = default)
        {
            return _client.ListVersionsAsync(bucketName, cancellationToken);
        }

        public Task<ListVersionsResponse> ListVersionsAsync(string bucketName, string prefix, CancellationToken cancellationToken = default)
        {
            return _client.ListVersionsAsync(bucketName, prefix, cancellationToken);
        }

        public Task<ListVersionsResponse> ListVersionsAsync(ListVersionsRequest request, CancellationToken cancellationToken = default)
        {
            return _client.ListVersionsAsync(request, cancellationToken);
        }

        public Task MakeObjectPublicAsync(string bucketName, string objectKey, bool enable)
        {
            return _client.MakeObjectPublicAsync(bucketName, objectKey, enable);
        }

        public Task<PutACLResponse> PutACLAsync(PutACLRequest request, CancellationToken cancellationToken = default)
        {
            return _client.PutACLAsync(request, cancellationToken);
        }

        public Task<PutBucketAccelerateConfigurationResponse> PutBucketAccelerateConfigurationAsync(PutBucketAccelerateConfigurationRequest request, CancellationToken cancellationToken = default)
        {
            return _client.PutBucketAccelerateConfigurationAsync(request, cancellationToken);
        }

        public Task<PutBucketAnalyticsConfigurationResponse> PutBucketAnalyticsConfigurationAsync(PutBucketAnalyticsConfigurationRequest request, CancellationToken cancellationToken = default)
        {
            return _client.PutBucketAnalyticsConfigurationAsync(request, cancellationToken);
        }

        public Task<PutBucketResponse> PutBucketAsync(string bucketName, CancellationToken cancellationToken = default)
        {
            return _client.PutBucketAsync(bucketName, cancellationToken);
        }

        public Task<PutBucketResponse> PutBucketAsync(PutBucketRequest request, CancellationToken cancellationToken = default)
        {
            return _client.PutBucketAsync(request, cancellationToken);
        }

        public Task<PutBucketEncryptionResponse> PutBucketEncryptionAsync(PutBucketEncryptionRequest request, CancellationToken cancellationToken = default)
        {
            return _client.PutBucketEncryptionAsync(request, cancellationToken);
        }

        public Task<PutBucketIntelligentTieringConfigurationResponse> PutBucketIntelligentTieringConfigurationAsync(PutBucketIntelligentTieringConfigurationRequest request, CancellationToken cancellationToken = default)
        {
            return _client.PutBucketIntelligentTieringConfigurationAsync(request, cancellationToken);
        }

        public Task<PutBucketInventoryConfigurationResponse> PutBucketInventoryConfigurationAsync(PutBucketInventoryConfigurationRequest request, CancellationToken cancellationToken = default)
        {
            return _client.PutBucketInventoryConfigurationAsync(request, cancellationToken);
        }

        public Task<PutBucketLoggingResponse> PutBucketLoggingAsync(PutBucketLoggingRequest request, CancellationToken cancellationToken = default)
        {
            return _client.PutBucketLoggingAsync(request, cancellationToken);
        }

        public Task<PutBucketMetricsConfigurationResponse> PutBucketMetricsConfigurationAsync(PutBucketMetricsConfigurationRequest request, CancellationToken cancellationToken = default)
        {
            return _client.PutBucketMetricsConfigurationAsync(request, cancellationToken);
        }

        public Task<PutBucketNotificationResponse> PutBucketNotificationAsync(PutBucketNotificationRequest request, CancellationToken cancellationToken = default)
        {
            return _client.PutBucketNotificationAsync(request, cancellationToken);
        }

        public Task<PutBucketOwnershipControlsResponse> PutBucketOwnershipControlsAsync(PutBucketOwnershipControlsRequest request, CancellationToken cancellationToken = default)
        {
            return _client.PutBucketOwnershipControlsAsync(request, cancellationToken);
        }

        public Task<PutBucketPolicyResponse> PutBucketPolicyAsync(string bucketName, string policy, CancellationToken cancellationToken = default)
        {
            return _client.PutBucketPolicyAsync(bucketName, policy, cancellationToken);
        }

        public Task<PutBucketPolicyResponse> PutBucketPolicyAsync(string bucketName, string policy, string contentMD5, CancellationToken cancellationToken = default)
        {
            return _client.PutBucketPolicyAsync(bucketName, policy, contentMD5, cancellationToken);
        }

        public Task<PutBucketPolicyResponse> PutBucketPolicyAsync(PutBucketPolicyRequest request, CancellationToken cancellationToken = default)
        {
            return _client.PutBucketPolicyAsync(request, cancellationToken);
        }

        public Task<PutBucketReplicationResponse> PutBucketReplicationAsync(PutBucketReplicationRequest request, CancellationToken cancellationToken = default)
        {
            return _client.PutBucketReplicationAsync(request, cancellationToken);
        }

        public Task<PutBucketRequestPaymentResponse> PutBucketRequestPaymentAsync(string bucketName, RequestPaymentConfiguration requestPaymentConfiguration, CancellationToken cancellationToken = default)
        {
            return _client.PutBucketRequestPaymentAsync(bucketName, requestPaymentConfiguration, cancellationToken);
        }

        public Task<PutBucketRequestPaymentResponse> PutBucketRequestPaymentAsync(PutBucketRequestPaymentRequest request, CancellationToken cancellationToken = default)
        {
            return _client.PutBucketRequestPaymentAsync(request, cancellationToken);
        }

        public Task<PutBucketTaggingResponse> PutBucketTaggingAsync(string bucketName, List<Tag> tagSet, CancellationToken cancellationToken = default)
        {
            return _client.PutBucketTaggingAsync(bucketName, tagSet, cancellationToken);
        }

        public Task<PutBucketTaggingResponse> PutBucketTaggingAsync(PutBucketTaggingRequest request, CancellationToken cancellationToken = default)
        {
            return _client.PutBucketTaggingAsync(request, cancellationToken);
        }

        public Task<PutBucketVersioningResponse> PutBucketVersioningAsync(PutBucketVersioningRequest request, CancellationToken cancellationToken = default)
        {
            return _client.PutBucketVersioningAsync(request, cancellationToken);
        }

        public Task<PutBucketWebsiteResponse> PutBucketWebsiteAsync(string bucketName, WebsiteConfiguration websiteConfiguration, CancellationToken cancellationToken = default)
        {
            return _client.PutBucketWebsiteAsync(bucketName, websiteConfiguration, cancellationToken);
        }

        public Task<PutBucketWebsiteResponse> PutBucketWebsiteAsync(PutBucketWebsiteRequest request, CancellationToken cancellationToken = default)
        {
            return _client.PutBucketWebsiteAsync(request, cancellationToken);
        }

        public Task<PutCORSConfigurationResponse> PutCORSConfigurationAsync(string bucketName, CORSConfiguration configuration, CancellationToken cancellationToken = default)
        {
            return _client.PutCORSConfigurationAsync(bucketName, configuration, cancellationToken);
        }

        public Task<PutCORSConfigurationResponse> PutCORSConfigurationAsync(PutCORSConfigurationRequest request, CancellationToken cancellationToken = default)
        {
            return _client.PutCORSConfigurationAsync(request, cancellationToken);
        }

        public Task<PutLifecycleConfigurationResponse> PutLifecycleConfigurationAsync(string bucketName, LifecycleConfiguration configuration, CancellationToken cancellationToken = default)
        {
            return _client.PutLifecycleConfigurationAsync(bucketName, configuration, cancellationToken);
        }

        public Task<PutLifecycleConfigurationResponse> PutLifecycleConfigurationAsync(PutLifecycleConfigurationRequest request, CancellationToken cancellationToken = default)
        {
            return _client.PutLifecycleConfigurationAsync(request, cancellationToken);
        }

        public Task<PutObjectResponse> PutObjectAsync(PutObjectRequest request, CancellationToken cancellationToken = default)
        {
            return _client.PutObjectAsync(request, cancellationToken);
        }

        public Task<PutObjectLegalHoldResponse> PutObjectLegalHoldAsync(PutObjectLegalHoldRequest request, CancellationToken cancellationToken = default)
        {
            return _client.PutObjectLegalHoldAsync(request, cancellationToken);
        }

        public Task<PutObjectLockConfigurationResponse> PutObjectLockConfigurationAsync(PutObjectLockConfigurationRequest request, CancellationToken cancellationToken = default)
        {
            return _client.PutObjectLockConfigurationAsync(request, cancellationToken);
        }

        public Task<PutObjectRetentionResponse> PutObjectRetentionAsync(PutObjectRetentionRequest request, CancellationToken cancellationToken = default)
        {
            return _client.PutObjectRetentionAsync(request, cancellationToken);
        }

        public Task<PutObjectTaggingResponse> PutObjectTaggingAsync(PutObjectTaggingRequest request, CancellationToken cancellationToken = default)
        {
            return _client.PutObjectTaggingAsync(request, cancellationToken);
        }

        public Task<PutPublicAccessBlockResponse> PutPublicAccessBlockAsync(PutPublicAccessBlockRequest request, CancellationToken cancellationToken = default)
        {
            return _client.PutPublicAccessBlockAsync(request, cancellationToken);
        }

        public Task<RestoreObjectResponse> RestoreObjectAsync(string bucketName, string key, CancellationToken cancellationToken = default)
        {
            return _client.RestoreObjectAsync(bucketName, key, cancellationToken);
        }

        public Task<RestoreObjectResponse> RestoreObjectAsync(string bucketName, string key, int days, CancellationToken cancellationToken = default)
        {
            return _client.RestoreObjectAsync(bucketName, key, days, cancellationToken);
        }

        public Task<RestoreObjectResponse> RestoreObjectAsync(string bucketName, string key, string versionId, CancellationToken cancellationToken = default)
        {
            return _client.RestoreObjectAsync(bucketName, key, versionId, cancellationToken);
        }

        public Task<RestoreObjectResponse> RestoreObjectAsync(string bucketName, string key, string versionId, int days, CancellationToken cancellationToken = default)
        {
            return _client.RestoreObjectAsync(bucketName, key, versionId, days, cancellationToken);
        }

        public Task<RestoreObjectResponse> RestoreObjectAsync(RestoreObjectRequest request, CancellationToken cancellationToken = default)
        {
            return _client.RestoreObjectAsync(request, cancellationToken);
        }

        public Task<SelectObjectContentResponse> SelectObjectContentAsync(SelectObjectContentRequest request, CancellationToken cancellationToken = default)
        {
            return _client.SelectObjectContentAsync(request, cancellationToken);
        }

        public Task UploadObjectFromFilePathAsync(string bucketName, string objectKey, string filepath, IDictionary<string, object> additionalProperties, CancellationToken cancellationToken = default)
        {
            return _client.UploadObjectFromFilePathAsync(bucketName, objectKey, filepath, additionalProperties, cancellationToken);
        }

        public Task UploadObjectFromStreamAsync(string bucketName, string objectKey, Stream stream, IDictionary<string, object> additionalProperties, CancellationToken cancellationToken = default)
        {
            return _client.UploadObjectFromStreamAsync(bucketName, objectKey, stream, additionalProperties, cancellationToken);
        }

        public Task<UploadPartResponse> UploadPartAsync(UploadPartRequest request, CancellationToken cancellationToken = default)
        {
            return _client.UploadPartAsync(request, cancellationToken);
        }

        public Task<WriteGetObjectResponseResponse> WriteGetObjectResponseAsync(WriteGetObjectResponseRequest request, CancellationToken cancellationToken = default)
        {
            return _client.WriteGetObjectResponseAsync(request, cancellationToken);
        }
    }
}