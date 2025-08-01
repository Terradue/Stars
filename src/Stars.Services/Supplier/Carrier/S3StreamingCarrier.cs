// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: S3StreamingCarrier.cs

using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Logging;
using Polly;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Supplier;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Resources;
using Terradue.Stars.Services.Supplier.Destination;

namespace Terradue.Stars.Services.Supplier.Carrier
{
    public class S3StreamingCarrier : ICarrier
    {
        private readonly ILogger logger;
        private readonly IResourceServiceProvider resourceServiceProvider;
        private readonly IS3ClientFactory s3ClientFactory;
        private readonly Regex regEx = new Regex(@"^s3://(?'hostOrBucket'[^/]*)(/.*)?$");

        public S3StreamingCarrier(ILogger<S3StreamingCarrier> logger,
                                  IResourceServiceProvider resourceServiceProvider,
                                  IS3ClientFactory s3ClientFactory)
        {
            this.logger = logger;
            this.resourceServiceProvider = resourceServiceProvider;
            this.s3ClientFactory = s3ClientFactory;
            Priority = 75;

        }

        public int Priority { get; set; }
        public string Key { get => Id; set { } }

        public string Id => "S3Streaming";

        public bool CanDeliver(IResource route, IDestination destination)
        {
            if (route is IOrderable) return false;
            if (!(destination is S3ObjectDestination)) return false;
            if (route is IAsset) return true;
            if (!(route is IStreamResource)) return false;

            return true;
        }

        public async Task<IResource> DeliverAsync(IDelivery delivery, CancellationToken ct, bool overwrite = false)
        {
            // First create de destination
            S3Delivery s3Delivery = delivery as S3Delivery;
            S3Resource s3DestinationResource = await s3ClientFactory.CreateAsync(S3Url.ParseUri(s3Delivery.Destination.Uri));

            // Get the resource as a stream
            IStreamResource inputStreamResource = await resourceServiceProvider.GetStreamResourceAsync(s3Delivery.Resource, ct);

            if (!overwrite && inputStreamResource.ContentLength > 0 &&
               Convert.ToUInt64(s3DestinationResource.ContentLength) == inputStreamResource.ContentLength)
            {
                logger.LogDebug("Object {0} exists with the same size. Skipping transfer", s3DestinationResource.Uri);
                return s3DestinationResource;
            }

            s3DestinationResource = await StreamToS3Object(inputStreamResource, s3DestinationResource, ct, overwrite);
            return s3DestinationResource;
        }

        public IDelivery QuoteDelivery(IResource route, IDestination destination)
        {
            if (!CanDeliver(route, destination)) return null;

            // Let's make a cost as MB to download
            int cost = 1000;
            try
            {
                if (route.ContentLength > 0)
                    cost = Convert.ToInt32(route.ContentLength / 1024 / 1024);
            }
            catch (Exception e)
            {
                logger.LogWarning("Error trying to get size of the node {0} : {1}", route.Uri, e.Message);
            }

            return new S3Delivery(this, route, destination as S3ObjectDestination, cost);
        }

        public async Task<S3Resource> StreamToS3Object(IStreamResource inputStreamResource, S3Resource s3outputStreamResource, CancellationToken ct, bool overwrite = false)
        {
            try
            {
                bool uploadStream = false;
                // in case source is also S3, try to make a copy
                if (inputStreamResource is S3Resource)
                {
                    uploadStream = true;
                    S3Resource s3InputStreamResource = inputStreamResource as S3Resource;
                    if (s3InputStreamResource.SameBucket(s3outputStreamResource))
                    {
                        logger.LogDebug("Source and destination are in the same bucket. Copying object {0} to {1}", s3InputStreamResource.Uri, s3outputStreamResource.Uri);
                        return await s3InputStreamResource.CopyTo(s3outputStreamResource, ct);
                    }
                }

                // If streamable cannot be ranged, pass by a blocking stream
                using (Stream sourceStream = await inputStreamResource.GetStreamAsync(ct))
                {
                    long safeContentLength = inputStreamResource.ContentLength > 50L * 1024 * 1024 * 1024
                        ? 50L * 1024 * 1024 * 1024
                        : (long)inputStreamResource.ContentLength;

                    long partSize = Math.Min(5L * 1024 * 1024 * 1024, Math.Max(50L * 1024 * 1024, safeContentLength / 10));

                    try
                    {
                        var l = sourceStream.Length;
                    }
                    catch (Exception e)
                    {
                        logger.LogWarning("Cannot get size of the resource at {0} : {1}. Trying upload stream", inputStreamResource.Uri, e.Message);
                        uploadStream = true;
                    }
                    if (inputStreamResource.ContentLength == 0 || uploadStream)
                    {
                        await Policy.Handle<AmazonS3Exception>(
                            ex => ex.StatusCode == HttpStatusCode.Conflict ||
                                  ex.StatusCode == HttpStatusCode.ServiceUnavailable ||
                                  ex.StatusCode == HttpStatusCode.RequestTimeout ||
                                  ex.StatusCode == HttpStatusCode.GatewayTimeout
                        )
                            .WaitAndRetryAsync(4, retryAttempt => TimeSpan.FromMilliseconds(Math.Pow(2, retryAttempt) * 500), (exception, timeSpan, retryCount, context) =>
                            {
                                logger.LogWarning("Error uploading stream to S3. Retrying in {0} seconds. Retry count {1}. Error: {2}", timeSpan.TotalSeconds, retryCount, exception.Message);
                                if (retryCount == 4)
                                    logger.LogError("Upload failed after 3 retries.");
                            }).ExecuteAsync(async () =>
                            {
                                await StreamUpload(s3outputStreamResource, sourceStream, partSize);
                            });
                    }
                    else
                    {
                        var tx = new TransferUtility(s3outputStreamResource.Client);
                        TransferUtilityUploadRequest ur = new TransferUtilityUploadRequest();
                        ur.PartSize = (int)partSize;
                        ur.AutoResetStreamPosition = false;
                        SetRequestParametersWithUri(s3outputStreamResource.S3Uri, ur);

                        ur.InputStream = sourceStream;
                        ur.ContentType = inputStreamResource.ContentType.MediaType;
                        await Policy.Handle<AmazonS3Exception>(
                            ex => ex.StatusCode == HttpStatusCode.Conflict ||
                                  ex.StatusCode == HttpStatusCode.ServiceUnavailable ||
                                  ex.StatusCode == HttpStatusCode.RequestTimeout ||
                                  ex.StatusCode == HttpStatusCode.GatewayTimeout
                        )
                            .WaitAndRetryAsync(4, retryAttempt => TimeSpan.FromMilliseconds(Math.Pow(2, retryAttempt) * 500), (exception, timeSpan, retryCount, context) =>
                            {
                                logger.LogWarning("Error uploading stream to S3. Retrying in {0} seconds. Retry count {1}. Error: {2}", timeSpan.TotalSeconds, retryCount, exception.Message);
                                if (retryCount == 4)
                                    logger.LogError("Upload failed after 3 retries.");
                            }).ExecuteAsync(async () =>
                            {
                                await tx.UploadAsync(ur);
                            });
                    }
                }

                // refresh metadata
                await s3outputStreamResource.LoadMetadata(ct);
                return s3outputStreamResource;
            }
            catch (WebException we)
            {
                try
                {
                    using (StreamReader sr = new StreamReader(we.Response.GetResponseStream()))
                        logger.LogDebug(sr.ReadToEnd());
                }
                catch { }
                throw;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        private async Task StreamUpload(S3Resource s3outputStreamResource, Stream sourceStream, long partSize)
        {
            S3UploadStream s3UploadStream = new S3UploadStream(s3outputStreamResource.Client, s3outputStreamResource.S3Uri.Bucket, s3outputStreamResource.S3Uri.Key, partSize);
            await StartSourceCopy(sourceStream, s3UploadStream, (int)Math.Min(partSize, 80 * 1024));
        }

        public async Task StartSourceCopy(Stream sourceStream, Stream destStream, int chunkSize = 80 * 1024)
        {
            ulong totalRead = 0;
            int read = 0;
            var buffer = new byte[chunkSize];
            do
            {
                read = await sourceStream.ReadAsync(buffer, 0, chunkSize).ConfigureAwait(false);
                await destStream.WriteAsync(buffer, 0, read).ConfigureAwait(false);
                totalRead += Convert.ToUInt32(read);
            } while (read > 0);
            destStream.Close();
        }

        private void SetRequestParametersWithUri(S3Url uri, TransferUtilityUploadRequest ur)
        {
            ur.BucketName = uri.Bucket;
            ur.Key = uri.Key;
        }
    }
}
