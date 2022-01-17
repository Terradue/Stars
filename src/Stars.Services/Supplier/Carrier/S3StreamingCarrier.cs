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
using System.Net.S3;
using Amazon.S3.Transfer;
using Amazon.Extensions.NETCore.Setup;
using System.Text.RegularExpressions;

namespace Terradue.Stars.Services.Supplier.Carrier
{
    public class S3StreamingCarrier : ICarrier
    {
        private readonly ILogger logger;
        private readonly AWSOptions options;
        private readonly S3BucketsOptions s3BucketsConfiguration;

        private readonly Regex regEx = new Regex(@"^s3://(?'hostOrBucket'[^/]*)(/.*)?$");

        public S3StreamingCarrier(ILogger<S3StreamingCarrier> logger, Amazon.Extensions.NETCore.Setup.AWSOptions options, S3BucketsOptions s3BucketsConfiguration = null)
        {
            this.logger = logger;
            this.options = options;
            this.s3BucketsConfiguration = s3BucketsConfiguration;
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
            if (!(route is IStreamable)) return false;

            return true;
        }

        public async Task<IResource> Deliver(IDelivery delivery, bool overwrite = false)
        {
            S3Delivery s3Delivery = delivery as S3Delivery;
            WebRoute s3Route = WebRoute.Create(s3Delivery.Destination.Uri);

            IStreamable streamable = delivery.Route as IStreamable;
            if (streamable == null && delivery.Route is IAsset)
                streamable = (delivery.Route as IAsset).GetStreamable();

            if (streamable == null)
                throw new InvalidDataException(string.Format("There is no streamable content in {0}", delivery.Route.Uri));

            if (!overwrite && streamable.ContentLength > 0 &&
               Convert.ToUInt64(s3Route.ContentLength) == streamable.ContentLength)
            {
                logger.LogDebug("Object {0} exists with the same size. Skipping transfer", s3Route.Uri);
                return s3Route;
            }
            s3Route = await StreamToS3Object(streamable, s3Route, overwrite);
            // if (streamable.ContentLength > 0 && Convert.ToUInt64(s3Route.ContentLength) != streamable.ContentLength)
            // throw new InvalidDataException(string.Format("Data transferred size ({0}) does not correspond with stream content length ({1})", s3Route.ContentLength, streamable.ContentLength));
            return s3Route;
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

        public async Task<WebRoute> StreamToS3Object(IStreamable streamable, WebRoute s3Resource, bool overwrite = false)
        {
            try
            {
                // in case source is also S3
                if (streamable is WebRoute)
                {
                    await (streamable as WebRoute).CacheHeadersAsync();
                    if ((streamable as WebRoute).Request is S3WebRequest)
                    {
                        S3WebRequest s3CopyWebRequest = (S3WebRequest)(streamable as WebRoute).Request.CloneRequest(streamable.Uri);
                        s3CopyWebRequest.Method = System.Net.S3.S3RequestMethods.Copy;
                        s3CopyWebRequest.CopyTo = s3Resource.Uri;
                        await s3CopyWebRequest.GetResponseAsync().ConfigureAwait(false);
                        return WebRoute.Create(s3Resource.Uri);
                    }
                }

                // If streamable cannot be ranged, pass by a blocking stream
                Stream sourceStream = await streamable.GetStreamAsync();
                int partSize = 10 * 1024 * 1024;
                S3WebRequest s3WebRequest = (S3WebRequest)(s3Resource.Request as S3WebRequest).CloneRequest(s3Resource.Uri);
                bool uploadStream = false;
                try
                {
                    var l = sourceStream.Length;
                }
                catch (Exception e)
                {
                    logger.LogWarning("Error trying to get size of the node {0} : {1}. Trying upload stream", streamable.Uri, e.Message);
                    uploadStream = true;
                }
                if (streamable.ContentLength == 0 || uploadStream)
                {
                    S3UploadStream s3UploadStream = new S3UploadStream(s3WebRequest.S3Client, S3UriParser.GetBucketName(s3Resource.Uri), S3UriParser.GetKey(s3Resource.Uri), partSize);
                    await StartSourceCopy(sourceStream, s3UploadStream, partSize);
                }
                else
                {

                    var tx = new TransferUtility(s3WebRequest.S3Client);
                    TransferUtilityUploadRequest ur = new TransferUtilityUploadRequest();
                    ur.PartSize = partSize;
                    ur.AutoResetStreamPosition = false;
                    SetRequestParametersWithUri(s3Resource.Uri, ur);

                    ur.InputStream = sourceStream;
                    ur.ContentType = streamable.ContentType.MediaType;
                    await tx.UploadAsync(ur);
                }

                var s3route = WebRoute.Create(s3Resource.Uri);
                await s3route.CacheHeadersAsync();
                return s3route;
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
                    read = await sourceStream.ReadAsync(buffer, 0, chunkSize);
                    await destStream.WriteAsync(buffer, 0, read);
                }
                catch (Exception e)
                {
                    logger.LogWarning(e.Message);
                }
                totalRead += Convert.ToUInt32(read);
            } while (read > 0);
            destStream.Close();
        }

        private void SetRequestParametersWithUri(Uri uri, TransferUtilityUploadRequest ur)
        {
            ur.BucketName = S3UriParser.GetBucketName(uri);
            ur.Key = S3UriParser.GetKey(uri);
        }
    }

}