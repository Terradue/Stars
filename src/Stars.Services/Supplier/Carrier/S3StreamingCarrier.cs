using System;
using System.IO;
using System.Threading.Tasks;
using Terradue.Stars.Services.Router;
using Terradue.Stars.Services.Supplier.Destination;
using Terradue.Stars.Interface.Supplier;
using Terradue.Stars.Interface.Supplier.Destination;
using Microsoft.Extensions.Logging;
using Terradue.Stars.Interface;
using System.Net;
using Amazon.S3.Transfer;
using System.Text.RegularExpressions;
using Terradue.Stars.Services.Resources;
using Microsoft.Extensions.Options;

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

        public async Task<IResource> Deliver(IDelivery delivery, bool overwrite = false)
        {
            // First create de destination
            S3Delivery s3Delivery = delivery as S3Delivery;
            S3Resource s3DestinationResource = await s3ClientFactory.CreateAsync(S3Url.ParseUri(s3Delivery.Destination.Uri));

            // Get the resource as a stream
            IStreamResource inputStreamResource = await resourceServiceProvider.GetStreamResourceAsync(s3Delivery.Resource);

            if (!overwrite && inputStreamResource.ContentLength > 0 &&
               Convert.ToUInt64(s3DestinationResource.ContentLength) == inputStreamResource.ContentLength)
            {
                logger.LogDebug("Object {0} exists with the same size. Skipping transfer", s3DestinationResource.Uri);
                return s3DestinationResource;
            }

            s3DestinationResource = await StreamToS3Object(inputStreamResource, s3DestinationResource, overwrite);
            // if (streamable.ContentLength > 0 && Convert.ToUInt64(s3Route.ContentLength) != streamable.ContentLength)
            // throw new InvalidDataException(string.Format("Data transferred size ({0}) does not correspond with stream content length ({1})", s3Route.ContentLength, streamable.ContentLength));
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

        public async Task<S3Resource> StreamToS3Object(IStreamResource inputStreamResource, S3Resource s3outputStreamResource, bool overwrite = false)
        {
            try
            {
                // in case source is also S3, try to make a copy
                if (inputStreamResource is S3Resource)
                {
                    S3Resource s3InputStreamResource = inputStreamResource as S3Resource;
                    if (s3InputStreamResource.SameBucket(s3outputStreamResource))
                    {
                        logger.LogDebug("Source and destination are in the same bucket. Copying object {0} to {1}", s3InputStreamResource.Uri, s3outputStreamResource.Uri);
                        return await s3InputStreamResource.CopyTo(s3outputStreamResource);
                    }
                }

                // If streamable cannot be ranged, pass by a blocking stream
                Stream sourceStream = await inputStreamResource.GetStreamAsync();
                int partSize = 10 * 1024 * 1024;
                bool uploadStream = false;
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
                    S3UploadStream s3UploadStream = new S3UploadStream(s3outputStreamResource.Client, s3outputStreamResource.S3Uri.Bucket, s3outputStreamResource.S3Uri.Key, partSize);
                    await StartSourceCopy(sourceStream, s3UploadStream, partSize);
                }
                else
                {
                    var tx = new TransferUtility(s3outputStreamResource.Client);
                    TransferUtilityUploadRequest ur = new TransferUtilityUploadRequest();
                    ur.PartSize = partSize;
                    ur.AutoResetStreamPosition = false;
                    SetRequestParametersWithUri(s3outputStreamResource.S3Uri, ur);

                    ur.InputStream = sourceStream;
                    ur.ContentType = inputStreamResource.ContentType.MediaType;
                    await tx.UploadAsync(ur);
                }

                // refresh metadata
                await s3outputStreamResource.LoadMetadata();
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

        public async Task StartSourceCopy(Stream sourceStream, Stream destStream, int chunkSize = 80 * 1024)
        {
            ulong totalRead = 0;
            int read = 0;
            var buffer = new byte[chunkSize];
            do
            {
                try
                {
                    read = await sourceStream.ReadAsync(buffer, 0, chunkSize).ConfigureAwait(false);
                    await destStream.WriteAsync(buffer, 0, read).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    logger.LogWarning(e.Message);
                }
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